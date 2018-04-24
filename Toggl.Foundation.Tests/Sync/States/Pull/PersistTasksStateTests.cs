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
using Task = Toggl.Foundation.Tests.Mocks.MockTask;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistTasksStateTests : PersistStateTests
    {
        public PersistTasksStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod
            : TheStartMethod<PersistTasksState, ITask, IDatabaseTask>
        {
            protected override PersistTasksState CreateState(IRepository<IDatabaseTask> repository, ISinceParameterRepository sinceParameterRepository)
                => new PersistTasksState(repository, sinceParameterRepository);

            protected override List<ITask> CreateListWithOneItem(DateTimeOffset? at = null)
                => new List<ITask> { new Task { At = at ?? Now, Name = Guid.NewGuid().ToString() } };

            protected override FetchObservables CreateObservablesWhichFetchesTwice()
                => CreateFetchObservables(
                    null, new SinceParameters(null),
                    tasks: Observable.Create<List<ITask>>(observer =>
                    {
                        observer.OnNext(new List<ITask>());
                        observer.OnNext(new List<ITask>());
                        return () => { };
                    }));

            protected override bool OtherSinceDatesDidntChange(ISinceParameters old, ISinceParameters next, DateTimeOffset at)
                => next.Workspaces == old.Workspaces
                   && next.Clients == old.Clients
                   && next.Projects == old.Projects
                   && next.Tags == old.Tags
                   && next.Tasks == at
                   && next.TimeEntries == old.TimeEntries;

            protected override FetchObservables CreateObservables(
                ISinceParameters since = null,
                List<ITask> tasks = null)
            => new FetchObservables(
                since ?? new SinceParameters(null),
                Observable.Return(new List<IWorkspace>()),
                Observable.Return(new List<IWorkspaceFeatureCollection>()),
                Observable.Return(Substitute.For<IUser>()),
                Observable.Return(new List<IClient>()),
                Observable.Return(new List<IProject>()),
                Observable.Return(new List<ITimeEntry>()),
                Observable.Return(new List<ITag>()),
                Observable.Return(tasks),
                Observable.Return(Substitute.For<IPreferences>()));

            protected override bool IsDeletedOnServer(ITask entity) => false;

            protected override IDatabaseTask Clean(ITask entity) => Models.Task.Clean(entity);

            protected override List<ITask> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset at)
                => new List<ITask>
                {
                    new Task { At = at.AddDays(-1), Name = Guid.NewGuid().ToString() },
                    new Task { At = at.AddDays(-3), Name = Guid.NewGuid().ToString() },
                    new Task { At = at, Name = Guid.NewGuid().ToString() },
                    new Task { At = at.AddDays(-2), Name = Guid.NewGuid().ToString() }
                };

            protected override Func<IDatabaseTask, bool> ArePersistedAndClean(List<ITask> tasks)
                => persisted => persisted.SyncStatus == SyncStatus.InSync && tasks.Any(w => w.Name == persisted.Name);
        }
    }
}
