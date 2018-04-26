using System;
using System.Reactive;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.States
{
    public class DeleteTimeEntryState : BaseDeleteEntityState<IDatabaseTimeEntry>
    {
        public DeleteTimeEntryState(ITogglApi api, IRepository<IDatabaseTimeEntry> repository) : base(api, repository)
        {
        }

        protected override IObservable<Unit> Delete(IDatabaseTimeEntry entity)
            => Api.TimeEntries.Delete(entity);

        protected override IDatabaseTimeEntry CopyFrom(IDatabaseTimeEntry entity)
            => TimeEntry.From(entity);
    }
}
