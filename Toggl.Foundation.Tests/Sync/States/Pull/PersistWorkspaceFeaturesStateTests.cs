using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using WorkspaceFeatureCollection = Toggl.Foundation.Tests.Mocks.MockWorkspaceFeatureCollection;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistWorkspaceFeaturesStateTests : PersistStateTests
    {
        public PersistWorkspaceFeaturesStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod
            : TheStartMethod<PersistWorkspacesFeaturesState, IWorkspaceFeatureCollection, IDatabaseWorkspaceFeatureCollection>
        {
            public override void SelectsTheLatestAtValue() { } // skip this test

            public override void UpdatesSinceParametersOfTheFetchedEntity() { } // skip this test

            protected override PersistWorkspacesFeaturesState CreateState(IRepository<IDatabaseWorkspaceFeatureCollection> repository, ISinceParameterRepository sinceParameterRepository)
                => new PersistWorkspacesFeaturesState(repository, sinceParameterRepository);

            protected override List<IWorkspaceFeatureCollection> CreateListWithOneItem(DateTimeOffset? at = null)
                => new List<IWorkspaceFeatureCollection> { new WorkspaceFeatureCollection() };

            protected override FetchObservables CreateObservablesWhichFetchesTwice()
                => CreateFetchObservables(
                    null, new SinceParameters(null),
                    workspaceFeatures: Observable.Create<List<IWorkspaceFeatureCollection>>(observer =>
                    {
                        observer.OnNext(new List<IWorkspaceFeatureCollection>());
                        observer.OnNext(new List<IWorkspaceFeatureCollection>());
                        return () => { };
                    }));

            protected override bool OtherSinceDatesDidntChange(ISinceParameters old, ISinceParameters next, DateTimeOffset at)
                => next.Workspaces == old.Workspaces
                   && next.Projects == old.Projects
                   && next.Clients == old.Clients
                   && next.Tags == old.Tags
                   && next.Tasks == old.Tasks
                   && next.TimeEntries == old.TimeEntries;

            protected override FetchObservables CreateObservables(
                ISinceParameters since = null,
                List<IWorkspaceFeatureCollection> workspaceFeatures = null)
            => new FetchObservables(
                since ?? new SinceParameters(null),
                Observable.Return(new List<IWorkspace>()),
                Observable.Return(workspaceFeatures),
                Observable.Return(Substitute.For<IUser>()),
                Observable.Return(new List<IClient>()),
                Observable.Return(new List<IProject>()),
                Observable.Return(new List<ITimeEntry>()),
                Observable.Return(new List<ITag>()),
                Observable.Return(new List<ITask>()),
                Observable.Return(Substitute.For<IPreferences>()));

            protected override List<IWorkspaceFeatureCollection> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset at)
                => new List<IWorkspaceFeatureCollection>
                {
                    new WorkspaceFeatureCollection { WorkspaceId = 123 },
                    new WorkspaceFeatureCollection { WorkspaceId = 456 }
                };

            protected override bool IsDeletedOnServer(IWorkspaceFeatureCollection entity) => false;

            protected override IDatabaseWorkspaceFeatureCollection Clean(IWorkspaceFeatureCollection features) => Models.WorkspaceFeatureCollection.From(features);

            protected override Func<IDatabaseWorkspaceFeatureCollection, bool> ArePersistedAndClean(List<IWorkspaceFeatureCollection> entities)
                => persisted => entities.Any(w => w.WorkspaceId == persisted.WorkspaceId);
        }
    }
}
