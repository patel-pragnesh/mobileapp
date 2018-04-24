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
using Tag = Toggl.Foundation.Tests.Mocks.MockTag;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistTagsStateTests : PersistStateTests
    {
        public PersistTagsStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod
            : TheStartMethod<PersistTagsState, ITag, IDatabaseTag>
        {
            protected override PersistTagsState CreateState(IRepository<IDatabaseTag> repository, ISinceParameterRepository sinceParameterRepository)
                => new PersistTagsState(repository, sinceParameterRepository);

            protected override List<ITag> CreateListWithOneItem(DateTimeOffset? at = null)
                => new List<ITag> { new Tag { At = at ?? Now, Name = Guid.NewGuid().ToString() } };

            protected override FetchObservables CreateObservablesWhichFetchesTwice()
                => CreateFetchObservables(
                    null, new SinceParameters(null),
                    tags: Observable.Create<List<ITag>>(observer =>
                    {
                        observer.OnNext(new List<ITag>());
                        observer.OnNext(new List<ITag>());
                        return () => { };
                    }));

            protected override bool OtherSinceDatesDidntChange(ISinceParameters old, ISinceParameters next, DateTimeOffset at)
                => next.Workspaces == old.Workspaces
                   && next.Projects == old.Projects
                   && next.Clients == old.Clients
                   && next.Tags == at
                   && next.Tasks == old.Tasks
                   && next.TimeEntries == old.TimeEntries;

            protected override FetchObservables CreateObservables(
                ISinceParameters since = null,
                List<ITag> tags = null)
            => new FetchObservables(
                    since ?? new SinceParameters(null),
                    Observable.Return(new List<IWorkspace>()),
                    Observable.Return(new List<IWorkspaceFeatureCollection>()),
                    Observable.Return(Substitute.For<IUser>()),
                    Observable.Return(new List<IClient>()),
                    Observable.Return(new List<IProject>()),
                    Observable.Return(new List<ITimeEntry>()),
                    Observable.Return(tags),
                    Observable.Return(new List<ITask>()),
                    Observable.Return(Substitute.For<IPreferences>()));

            protected override bool IsDeletedOnServer(ITag entity) => entity.ServerDeletedAt.HasValue;

            protected override IDatabaseTag Clean(ITag entity) => Models.Tag.Clean(entity);

            protected override List<ITag> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset at)
                => new List<ITag>
                {
                    new Tag { At = at.AddDays(-1), Name = Guid.NewGuid().ToString() },
                    new Tag { At = at.AddDays(-3), Name = Guid.NewGuid().ToString() },
                    new Tag { At = at, Name = Guid.NewGuid().ToString(), ServerDeletedAt = at.AddDays(-1) },
                    new Tag { At = at.AddDays(-2), Name = Guid.NewGuid().ToString() }
                };

            protected override Func<IDatabaseTag, bool> ArePersistedAndClean(List<ITag> entities)
                => persisted => persisted.SyncStatus == SyncStatus.InSync && entities.Any(te => te.Name == persisted.Name);
        }
    }
}
