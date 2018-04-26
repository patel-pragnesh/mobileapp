using System;
using System.Reactive;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    public sealed class DeleteLocalTimeEntryState : BaseDeleteLocalEntityState<IDatabaseTimeEntry>
    {
        public DeleteLocalTimeEntryState(IRepository<IDatabaseTimeEntry> repository)
            : base(repository)
        {
        }

        protected override IObservable<Unit> Delete(IDatabaseTimeEntry entity)
            => Repository.Delete(entity.Id);
    }
}
