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
using Client = Toggl.Foundation.Tests.Mocks.MockClient;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistClientsStateTests : PersistStateTests
    {
        public PersistClientsStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod
            : TheStartMethod<PersistClientsState, IClient, IDatabaseClient>
        {
            protected override PersistClientsState CreateState(IRepository<IDatabaseClient> repository, ISinceParameterRepository sinceParameterRepository)
                => new PersistClientsState(repository, sinceParameterRepository);

            protected override List<IClient> CreateListWithOneItem(DateTimeOffset? at = null)
                => new List<IClient> { new Client { At = at ?? Now, Name = Guid.NewGuid().ToString() } };

            protected override FetchObservables CreateObservablesWhichFetchesTwice()
                => CreateFetchObservables(
                    null, new SinceParameters(null),
                    clients: Observable.Create<List<IClient>>(observer =>
                    {
                        observer.OnNext(new List<IClient>());
                        observer.OnNext(new List<IClient>());
                        return () => { };
                    }));

            protected override bool OtherSinceDatesDidntChange(ISinceParameters old, ISinceParameters next, DateTimeOffset at)
                => next.Workspaces == old.Workspaces
                   && next.Projects == old.Projects
                   && next.Clients == at
                   && next.Tags == old.Tags
                   && next.Tasks == old.Tasks
                   && next.TimeEntries == old.TimeEntries;

            protected override FetchObservables CreateObservables(
                ISinceParameters since = null,
                List<IClient> clients = null)
            => new FetchObservables(
                since ?? new SinceParameters(null),
                Observable.Return(new List<IWorkspace>()),
                Observable.Return(new List<IWorkspaceFeatureCollection>()),
                Observable.Return(Substitute.For<IUser>()),
                Observable.Return(clients),
                Observable.Return(new List<IProject>()),
                Observable.Return(new List<ITimeEntry>()),
                Observable.Return(new List<ITag>()),
                Observable.Return(new List<ITask>()),
                Observable.Return(Substitute.For<IPreferences>()));

            protected override bool IsDeletedOnServer(IClient entity) => entity.ServerDeletedAt.HasValue;

            protected override IDatabaseClient Clean(IClient entity) => Models.Client.Clean(entity);

            protected override List<IClient> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset at)
                => new List<IClient>
                {
                    new Client { At = at.AddDays(-1), Name = Guid.NewGuid().ToString() },
                    new Client { At = at.AddDays(-3), Name = Guid.NewGuid().ToString() },
                    new Client { At = at, ServerDeletedAt = at, Name = Guid.NewGuid().ToString() },
                    new Client { At = at.AddDays(-2), Name = Guid.NewGuid().ToString() }
                };

            protected override Func<IDatabaseClient, bool> ArePersistedAndClean(List<IClient> entities)
                => persisted => persisted.SyncStatus == SyncStatus.InSync && entities.Any(w => w.Name == persisted.Name);
        }
    }
}
