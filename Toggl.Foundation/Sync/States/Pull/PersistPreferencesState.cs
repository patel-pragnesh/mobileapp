using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistPreferencesState : BasePersistState<IPreferences, IDatabasePreferences>
    {
        public PersistPreferencesState(IRepository<IDatabasePreferences> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, sinceParameterRepository, Resolver.ForPreferences())
        {
        }

        protected override IObservable<IEnumerable<IPreferences>> FetchObservable(FetchObservables fetch)
            => fetch.Preferences.Select(preferences
                => preferences == null
                    ? new IPreferences[0]
                    : new[] { preferences });

        protected override IDatabasePreferences ConvertToDatabaseEntity(IPreferences entity)
            => Preferences.Clean(entity);

        protected override ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated)
            => old;
    }
}
