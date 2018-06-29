using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States.Push;
using Toggl.Foundation.Tests.Sync.States.Push.BaseStates;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.ApiClients;
using Xunit;
using static Toggl.Foundation.Sync.PushSyncOperation;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public sealed class CreateEntityStateTests : BasePushEntityStateTests
    {
        private readonly ICreatingApiClient<ITestModel> api 
            = Substitute.For<ICreatingApiClient<ITestModel>>();

        private readonly IBaseDataSource<IThreadSafeTestModel> dataSource;

        private readonly ITestAnalyticsService analyticsService
            = Substitute.For<ITestAnalyticsService>();

        protected override PushSyncOperation Operation => PushSyncOperation.Create;

        public CreateEntityStateTests()
            : this(Substitute.For<IBaseDataSource<IThreadSafeTestModel>>())
        {
        }

        private CreateEntityStateTests(IBaseDataSource<IThreadSafeTestModel> dataSource)
        {
            this.dataSource = dataSource;
            SyncAnalyticsExtensions.SearchStrategy = TestSyncAnalyticsExtensionsSearchStrategy;
        }

        [Fact, LogIfTooSlow]
        public void ReturnsSuccessfulTransitionWhenEverythingWorks()
        {
            var state = (CreateEntityState<ITestModel, IThreadSafeTestModel>)CreateState();
            var entity = new TestModel(-1, SyncStatus.SyncFailed);
            var withPositiveId = new TestModel(Math.Abs(entity.Id), SyncStatus.InSync);
            api.Create(Arg.Any<ITestModel>())
                .Returns(Observable.Return(withPositiveId));
            dataSource.Overwrite(Arg.Any<IThreadSafeTestModel>(), Arg.Any<IThreadSafeTestModel>())
                .Returns(x => Observable.Return((IThreadSafeTestModel)x[1]));

            var transition = state.Start(entity).SingleAsync().Wait();
            var persistedEntity = ((Transition<IThreadSafeTestModel>)transition).Parameter;

            transition.Result.Should().Be(state.CreatingFinished);
            persistedEntity.Id.Should().NotBe(entity.Id);
            persistedEntity.Id.Should().BeGreaterThan(0);
            persistedEntity.SyncStatus.Should().Be(SyncStatus.InSync);
        }

        [Fact, LogIfTooSlow]
        public void UpdateIsCalledWithCorrectParameters()
        {
            var state = (CreateEntityState<ITestModel, IThreadSafeTestModel>)CreateState();
            var entity = new TestModel(-1, SyncStatus.SyncFailed);
            var withPositiveId = new TestModel(Math.Abs(entity.Id), SyncStatus.InSync);
            api.Create(entity)
                .Returns(Observable.Return(withPositiveId));

            state.Start(entity).SingleAsync().Wait();

            dataSource
                .Received()
                .Overwrite(Arg.Is<IThreadSafeTestModel>(model => model.Id == entity.Id), Arg.Is<IThreadSafeTestModel>(model => model.Id == withPositiveId.Id));
        }

        [Fact, LogIfTooSlow]
        public void TracksEntitySyncStatusInCaseOfSuccess()
        {
            var exception = new Exception();
            var state = CreateState();
            var entity = new TestModel(-1, SyncStatus.SyncFailed);
            var withPositiveId = new TestModel(Math.Abs(entity.Id), SyncStatus.InSync);
            api.Create(Arg.Any<ITestModel>())
               .Returns(Observable.Return(withPositiveId));
            dataSource.Overwrite(Arg.Any<IThreadSafeTestModel>(), Arg.Any<IThreadSafeTestModel>())
                .Returns(x => Observable.Return((IThreadSafeTestModel)x[1]));

            state.Start(entity).Wait();

            analyticsService.EntitySyncStatus.Received().Track(
                entity.GetSafeTypeName(),
                $"{Create}:{Resources.Success}");
        }

        [Fact, LogIfTooSlow]
        public void TracksEntitySyncedInCaseOfSuccess()
        {
            var exception = new Exception();
            var state = CreateState();
            var entity = new TestModel(-1, SyncStatus.SyncFailed);
            var withPositiveId = new TestModel(Math.Abs(entity.Id), SyncStatus.InSync);
            api.Create(Arg.Any<ITestModel>())
               .Returns(Observable.Return(withPositiveId));
            dataSource.Overwrite(Arg.Any<IThreadSafeTestModel>(), Arg.Any<IThreadSafeTestModel>())
                .Returns(x => Observable.Return((IThreadSafeTestModel)x[1]));

            state.Start(entity).Wait();

            analyticsService.EntitySynced.Received().Track(Create, entity.GetSafeTypeName());
        }

        [Fact, LogIfTooSlow]
        public void TracksEntitySyncStatusInCaseOfFailure()
        {
            var exception = new Exception();
            var state = CreateState();
            var entity = Substitute.For<IThreadSafeTestModel>();
            PrepareApiCallFunctionToThrow(exception);

            state.Start(entity).Wait();

            analyticsService.EntitySyncStatus.Received().Track(
                entity.GetSafeTypeName(),
                $"{Create}:{Resources.Failure}");
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(EntityTypes), MemberType = typeof(BasePushEntityStateTests))]
        public void TracksEntitySyncErrorInCaseOfFailure(Type entityType)
        {
            var exception = new Exception("SomeRandomMessage");
            var entity = (IThreadSafeTestModel)Substitute.For(new[] { entityType }, new object[0]);
            var state = new CreateEntityState<ITestModel, IThreadSafeTestModel>(api, dataSource, analyticsService, _ => null);
            var expectedMessage = $"{Create}:{exception.Message}";
            var analyticsEvent = entity.GetType().ToSyncErrorAnalyticsEvent(analyticsService);
            PrepareApiCallFunctionToThrow(exception);

            state.Start(entity).Wait();

            analyticsEvent.Received().Track(expectedMessage);
        }

        protected override BasePushEntityState<IThreadSafeTestModel> CreateState()
            => new CreateEntityState<ITestModel, IThreadSafeTestModel>(api, dataSource, analyticsService, TestModel.From);

        protected override void PrepareApiCallFunctionToThrow(Exception e)
        {
            api.Create(Arg.Any<ITestModel>())
                .Returns(_ => Observable.Throw<ITestModel>(e));
        }

        protected override void PrepareDatabaseOperationToThrow(Exception e)
        {
            dataSource.Overwrite(Arg.Any<IThreadSafeTestModel>(), Arg.Any<IThreadSafeTestModel>())
                .Returns(_ => Observable.Throw<IThreadSafeTestModel>(e));
        }
    }
}
