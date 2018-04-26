using System;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class CreateTimeEntryState : BaseCreateEntityState<IDatabaseTimeEntry>
    {
        public CreateTimeEntryState(ITogglApi api, IRepository<IDatabaseTimeEntry> repository) : base(api, repository)
        {
        }

        protected override IObservable<IDatabaseTimeEntry> Create(IDatabaseTimeEntry entity)
            => Api.TimeEntries.Create(entity).Select(TimeEntry.Clean);

        protected override IDatabaseTimeEntry CopyFrom(IDatabaseTimeEntry entity)
            => TimeEntry.From(entity);
    }
}
