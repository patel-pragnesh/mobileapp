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

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistPreferencesStateTests : PersistStateTests
    {
        public PersistPreferencesStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod
            : TheStartMethod<PersistPreferencesState, IPreferences, IDatabasePreferences>
        {
            public override void SelectsTheLatestAtValue() { } // skip this test

            public override void UpdatesSinceParametersOfTheFetchedEntity() { } // skip this test

            protected override PersistPreferencesState CreateState(IRepository<IDatabasePreferences> repository, ISinceParameterRepository sinceParameterRepository)
                => new PersistPreferencesState(repository, sinceParameterRepository);

            protected override List<IPreferences> CreateListWithOneItem(DateTimeOffset? at = null)
                => new List<IPreferences> { Substitute.For<IPreferences>() };

            protected override FetchObservables CreateObservablesWhichFetchesTwice()
                => CreateFetchObservables(
                    null, new SinceParameters(null),
                    preferences: Observable.Create<IPreferences>(observer =>
                    {
                        observer.OnNext(Substitute.For<IPreferences>());
                        observer.OnNext(Substitute.For<IPreferences>());
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
                List<IPreferences> entity = null)
            => new FetchObservables(
                since ?? new SinceParameters(null),
                Observable.Return(new List<IWorkspace>()),
                Observable.Return(new List<IWorkspaceFeatureCollection>()),
                Observable.Return(Substitute.For<IUser>()),
                Observable.Return(new List<IClient>()),
                Observable.Return(new List<IProject>()),
                Observable.Return(new List<ITimeEntry>()),
                Observable.Return(new List<ITag>()),
                Observable.Return(new List<ITask>()),
                Observable.Return(entity?.First()));

            protected override List<IPreferences> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset at)
                => new List<IPreferences>
                {
                    Substitute.For<IPreferences>()
                };

            protected override bool IsDeletedOnServer(IPreferences entity) => false;

            protected override IDatabasePreferences Clean(IPreferences entity) => Preferences.Clean(entity);

            protected override Func<IDatabasePreferences, bool> ArePersistedAndClean(List<IPreferences> entities)
                => persisted => persisted.SyncStatus == SyncStatus.InSync;
        }
    }
}