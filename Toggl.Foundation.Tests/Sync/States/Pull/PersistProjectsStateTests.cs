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
using Project = Toggl.Foundation.Tests.Mocks.MockProject;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistProjectsStateTests : PersistStateTests
    {
        public PersistProjectsStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod
            : TheStartMethod<PersistProjectsState, IProject, IDatabaseProject>
        {
            protected override PersistProjectsState CreateState(IRepository<IDatabaseProject> repository, ISinceParameterRepository sinceParameterRepository)
                => new PersistProjectsState(repository, sinceParameterRepository);

            protected override List<IProject> CreateListWithOneItem(DateTimeOffset? at = null)
                => new List<IProject> { new Project { At = at ?? Now, Name = Guid.NewGuid().ToString() } };

            protected override FetchObservables CreateObservablesWhichFetchesTwice()
                => CreateFetchObservables(
                    null, new SinceParameters(null),
                    projects: Observable.Create<List<IProject>>(observer =>
                    {
                        observer.OnNext(new List<IProject>());
                        observer.OnNext(new List<IProject>());
                        return () => { };
                    }));

            protected override bool OtherSinceDatesDidntChange(ISinceParameters old, ISinceParameters next, DateTimeOffset at)
                => next.Workspaces == old.Workspaces
                   && next.Clients == old.Clients
                   && next.Projects == at
                   && next.Tags == old.Tags
                   && next.Tasks == old.Tasks
                   && next.TimeEntries == old.TimeEntries;

            protected override FetchObservables CreateObservables(
                ISinceParameters since = null,
                List<IProject> projects = null)
            => new FetchObservables(
                since ?? new SinceParameters(null),
                Observable.Return(new List<IWorkspace>()),
                Observable.Return(new List<IWorkspaceFeatureCollection>()),
                Observable.Return(Substitute.For<IUser>()),
                Observable.Return(new List<IClient>()),
                Observable.Return(projects),
                Observable.Return(new List<ITimeEntry>()),
                Observable.Return(new List<ITag>()),
                Observable.Return(new List<ITask>()),
                Observable.Return(Substitute.For<IPreferences>()));

            protected override bool IsDeletedOnServer(IProject entity) => entity.ServerDeletedAt.HasValue;

            protected override IDatabaseProject Clean(IProject entity) => Models.Project.Clean(entity);

            protected override List<IProject> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset at)
                => new List<IProject>
                {
                    new Project { At = at.AddDays(-1), Name = Guid.NewGuid().ToString() },
                    new Project { At = at.AddDays(-3), Name = Guid.NewGuid().ToString() },
                    new Project { At = at, ServerDeletedAt = at, Name = Guid.NewGuid().ToString() },
                    new Project { At = at.AddDays(-2), Name = Guid.NewGuid().ToString() }
                };

            protected override Func<IDatabaseProject, bool> ArePersistedAndClean(List<IProject> entities)
                => persisted => persisted.SyncStatus == SyncStatus.InSync && entities.Any(w => w.Name == persisted.Name);
        }
    }
}
