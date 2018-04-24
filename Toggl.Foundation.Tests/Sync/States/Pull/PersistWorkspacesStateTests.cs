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
using Workspace = Toggl.Foundation.Tests.Mocks.MockWorkspace;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistWorkspacesStateTests : PersistStateTests
    {
        public PersistWorkspacesStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod
            : TheStartMethod<PersistWorkspacesState, IWorkspace, IDatabaseWorkspace>
        {
            protected override PersistWorkspacesState CreateState(IRepository<IDatabaseWorkspace> repository, ISinceParameterRepository sinceParameterRepository)
                => new PersistWorkspacesState(repository, sinceParameterRepository);

            protected override List<IWorkspace> CreateListWithOneItem(DateTimeOffset? at = null)
                => new List<IWorkspace> { new Workspace { At = at ?? DateTimeOffset.Now, Name = Guid.NewGuid().ToString() } };

            protected override FetchObservables CreateObservablesWhichFetchesTwice()
                => CreateFetchObservables(
                    null, new SinceParameters(null),
                    Observable.Create<List<IWorkspace>>(observer =>
                    {
                        observer.OnNext(new List<IWorkspace>());
                        observer.OnNext(new List<IWorkspace>());
                        return () => { };
                    }));

            protected override bool OtherSinceDatesDidntChange(ISinceParameters old, ISinceParameters next, DateTimeOffset at)
                => next.Workspaces == at
                   && next.Projects == old.Projects
                   && next.Clients == old.Clients
                   && next.Tags == old.Tags
                   && next.Tasks == old.Tasks
                   && next.TimeEntries == old.TimeEntries;

            protected override FetchObservables CreateObservables(
                ISinceParameters since = null,
                List<IWorkspace> workspaces = null)
            => new FetchObservables(
                since ?? new SinceParameters(null),
                Observable.Return(workspaces),
                Observable.Return(new List<IWorkspaceFeatureCollection>()),
                Observable.Return(Substitute.For<IUser>()),
                Observable.Return(new List<IClient>()),
                Observable.Return(new List<IProject>()),
                Observable.Return(new List<ITimeEntry>()),
                Observable.Return(new List<ITag>()),
                Observable.Return(new List<ITask>()),
                Observable.Return(Substitute.For<IPreferences>()));

            protected override List<IWorkspace> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset at)
                => new List<IWorkspace>
                {
                    new Workspace { At = at.AddDays(-1), Name = Guid.NewGuid().ToString() },
                    new Workspace { At = at.AddDays(-3), Name = Guid.NewGuid().ToString() },
                    new Workspace { At = at, ServerDeletedAt = at, Name = Guid.NewGuid().ToString() },
                    new Workspace { At = at.AddDays(-2), Name = Guid.NewGuid().ToString() }
                };

            protected override bool IsDeletedOnServer(IWorkspace entity) => entity.ServerDeletedAt.HasValue;

            protected override IDatabaseWorkspace Clean(IWorkspace workspace) => Models.Workspace.Clean(workspace);

            protected override Func<IDatabaseWorkspace, bool> ArePersistedAndClean(List<IWorkspace> entities)
                => persisted => persisted.SyncStatus == SyncStatus.InSync && entities.Any(w => w.Name == persisted.Name);
        }
    }
}
