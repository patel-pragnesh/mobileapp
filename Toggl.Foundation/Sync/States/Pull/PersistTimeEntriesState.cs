using System;
using System.Collections.Generic;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistTimeEntriesState : BasePersistState<ITimeEntry, IDatabaseTimeEntry>
    {
        public PersistTimeEntriesState(IRepository<IDatabaseTimeEntry> repository, ISinceParameterRepository sinceParameterRepository, ITimeService timeService)
            : base(repository, TimeEntry.Clean, sinceParameterRepository, Resolver.ForTimeEntries(), new TimeEntryRivalsResolver(timeService))
        {
        }

        protected override IObservable<IEnumerable<ITimeEntry>> FetchObservable(FetchObservables fetch)
            => fetch.TimeEntries;

        protected override ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated)
            => new SinceParameters(old, timeEntries: lastUpdated);
    }
}
