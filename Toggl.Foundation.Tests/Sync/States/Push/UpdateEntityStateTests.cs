﻿using System;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Foundation.Sync.States.Push;
using Toggl.Foundation.Tests.Helpers;
using Toggl.Foundation.Tests.Sync.States.Push.BaseStates;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.ApiClients;
using Toggl.Ultrawave.Exceptions;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public sealed class UpdateEntityStateTests : BasePushEntityStateTests
    {
        private readonly IUpdatingApiClient<ITestModel> api
            = Substitute.For<IUpdatingApiClient<ITestModel>>();

        private readonly IDataSource<IThreadSafeTestModel, IDatabaseTestModel, IDatabaseTestModel> dataSource
            = Substitute.For<IDataSource<IThreadSafeTestModel, IDatabaseTestModel, IDatabaseTestModel>>();

        [Fact, LogIfTooSlow]
        public void ReturnsTheFailTransitionWhenEntityIsNull()
        {
            var state = (UpdateEntityState<ITestModel, IThreadSafeTestModel, IDatabaseTestModel>)CreateState();
            var transition = state.Start(null).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.UnknownError);
            parameter.Reason.Should().BeOfType<ArgumentNullException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ServerExceptions), MemberType = typeof(ApiExceptions))]
        public void ReturnsTheServerErrorTransitionWhenHttpFailsWithServerError(ServerErrorException exception)
        {
            var state = (UpdateEntityState<ITestModel, IThreadSafeTestModel, IDatabaseTestModel>)CreateState();
            var entity = new TestModel(1, SyncStatus.InSync);
            api.Update(Arg.Any<ITestModel>())
                .Returns(_ => Observable.Throw<ITestModel>(exception));

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.ServerError);
            parameter.Reason.Should().BeAssignableTo<ServerErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ClientExceptionsWhichAreNotReThrownInSyncStates), MemberType = typeof(ApiExceptions))]
        public void ReturnsTheClientErrorTransitionWhenHttpFailsWithClientError(ClientErrorException exception)
        {
            var state = (UpdateEntityState<ITestModel, IThreadSafeTestModel, IDatabaseTestModel>)CreateState();
            var entity = new TestModel(1, SyncStatus.InSync);
            api.Update(Arg.Any<ITestModel>())
                .Returns(_ => Observable.Throw<ITestModel>(exception));

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.ClientError);
            parameter.Reason.Should().BeAssignableTo<ClientErrorException>();
        }

        [Fact, LogIfTooSlow]
        public void ReturnsTheUnknownErrorTransitionWhenHttpFailsWithNonApiError()
        {
            var state = (UpdateEntityState<ITestModel, IThreadSafeTestModel, IDatabaseTestModel>)CreateState();
            var entity = new TestModel(1, SyncStatus.InSync);
            api.Update(Arg.Any<ITestModel>())
                .Returns(_ => Observable.Throw<ITestModel>(new TestException()));

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.UnknownError);
            parameter.Reason.Should().BeOfType<TestException>();
        }

        [Fact, LogIfTooSlow]
        public void ReturnsTheFailTransitionWhenDatabaseOperationFails()
        {
            var state = (UpdateEntityState<ITestModel, IThreadSafeTestModel, IDatabaseTestModel>)CreateState();
            var entity = new TestModel(1, SyncStatus.InSync);
            dataSource
                .OverwriteIfOriginalDidNotChange(Arg.Any<IThreadSafeTestModel>(), Arg.Any<IThreadSafeTestModel>())
                .Returns(Observable.Throw<IConflictResolutionResult<IThreadSafeTestModel>>(new TestException()));

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<(Exception Reason, IThreadSafeTestModel)>)transition).Parameter;

            transition.Result.Should().Be(state.UnknownError);
            parameter.Reason.Should().BeOfType<TestException>();
        }

        [Fact, LogIfTooSlow]
        public void UpdateApiCallIsCalledWithTheInputEntity()
        {
            var state = (UpdateEntityState<ITestModel, IThreadSafeTestModel, IDatabaseTestModel>)CreateState();
            var entity = new TestModel(1, SyncStatus.InSync);
            api.Update(entity)
                .Returns(Observable.Return(Substitute.For<ITestModel>()));
            dataSource
                .OverwriteIfOriginalDidNotChange(Arg.Any<IThreadSafeTestModel>(), Arg.Any<IThreadSafeTestModel>())
                .Returns(Observable.Return(new UpdateResult<IThreadSafeTestModel>(entity.Id, entity)));

            state.Start(entity).SingleAsync().Wait();

            api.Received().Update(Arg.Is(entity));
        }

        [Fact, LogIfTooSlow]
        public void ReturnsTheEntityChangedTransitionWhenEntityChangesLocally()
        {
            var state = (UpdateEntityState<ITestModel, IThreadSafeTestModel, IDatabaseTestModel>)CreateState();
            var at = new DateTimeOffset(2017, 9, 1, 12, 34, 56, TimeSpan.Zero);
            var entity = new TestModel { Id = 1, At = at, SyncStatus = SyncStatus.SyncNeeded };
            api.Update(Arg.Any<ITestModel>())
                .Returns(Observable.Return(entity));
            dataSource
                .OverwriteIfOriginalDidNotChange(Arg.Any<IThreadSafeTestModel>(), Arg.Any<IThreadSafeTestModel>())
                .Returns(Observable.Return(new IgnoreResult<IThreadSafeTestModel>(entity.Id)));

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<IThreadSafeTestModel>)transition).Parameter;

            transition.Result.Should().Be(state.EntityChanged);
            parameter.Id.Should().Be(entity.Id);
        }

        [Fact, LogIfTooSlow]
        public void ReturnsTheUpdatingSuccessfulTransitionWhenEntityDoesNotChangeLocallyAndAllFunctionsAreCalledWithCorrectParameters()
        {
            var state = (UpdateEntityState<ITestModel, IThreadSafeTestModel, IDatabaseTestModel>)CreateState();
            var at = new DateTimeOffset(2017, 9, 1, 12, 34, 56, TimeSpan.Zero);
            var entity = new TestModel { Id = 1, At = at, SyncStatus = SyncStatus.SyncNeeded };
            var serverEntity = new TestModel { Id = 2, At = at, SyncStatus = SyncStatus.SyncNeeded };
            var localEntity = new TestModel { Id = 3, At = at, SyncStatus = SyncStatus.SyncNeeded };
            var updatedEntity = new TestModel { Id = 4, At = at, SyncStatus = SyncStatus.SyncNeeded };
            api.Update(entity)
                .Returns(Observable.Return(serverEntity));
            dataSource
                .GetById(entity.Id)
                .Returns(Observable.Return(localEntity));
            dataSource
                .OverwriteIfOriginalDidNotChange(Arg.Any<IThreadSafeTestModel>(), Arg.Any<IThreadSafeTestModel>())
                .Returns(Observable.Return(new UpdateResult<IThreadSafeTestModel>(entity.Id, updatedEntity)));

            var transition = state.Start(entity).SingleAsync().Wait();
            var parameter = ((Transition<IThreadSafeTestModel>)transition).Parameter;

            transition.Result.Should().Be(state.UpdatingSucceeded);
            parameter.Should().BeEquivalentTo(updatedEntity, options => options.IncludingProperties());
            dataSource.Received()
                .OverwriteIfOriginalDidNotChange(
                    Arg.Is<IThreadSafeTestModel>(theOriginalEntity => theOriginalEntity.Id == entity.Id), Arg.Is<IThreadSafeTestModel>(theUpdatedEntity => theUpdatedEntity.Id == serverEntity.Id));
        }

        protected override BasePushEntityState<IThreadSafeTestModel, IDatabaseTestModel> CreateState()
            => new UpdateEntityState<ITestModel, IThreadSafeTestModel, IDatabaseTestModel>(api, dataSource, TestModel.From);

        protected override void PrepareApiCallFunctionToThrow(Exception e)
        {
            api.Update(Arg.Any<ITestModel>())
                .Returns(_ => Observable.Throw<ITestModel>(e));
        }

        protected override void PrepareDatabaseOperationToThrow(Exception e)
        {
            dataSource.OverwriteIfOriginalDidNotChange(Arg.Any<IThreadSafeTestModel>(), Arg.Any<IThreadSafeTestModel>())
                .Returns(_ => Observable.Throw<IConflictResolutionResult<IThreadSafeTestModel>>(e));
        }
    }
}
