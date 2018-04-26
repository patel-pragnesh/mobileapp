using System;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PushTimeEntriesState : BasePushState<IDatabaseTimeEntry>
    {
        public PushTimeEntriesState(IRepository<IDatabaseTimeEntry> repository)
            : base(repository)
        {
        }

        protected override DateTimeOffset LastChange(IDatabaseTimeEntry entity)
            => entity.At;

        protected override IDatabaseTimeEntry CopyFrom(IDatabaseTimeEntry entity)
            => TimeEntry.From(entity);
    }
}
