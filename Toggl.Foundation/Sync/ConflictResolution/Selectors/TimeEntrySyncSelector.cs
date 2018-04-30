using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    internal sealed class TimeEntrySyncSelector : ISyncSelector<IDatabaseTimeEntry>
    {
        public bool IsInSync(IDatabaseTimeEntry model)
            => model.SyncStatus == SyncStatus.InSync;
    }
}
