using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    internal sealed class TaskSyncSelector : ISyncSelector<IDatabaseTask>
    {
        public bool IsInSync(IDatabaseTask model)
            => model.SyncStatus == SyncStatus.InSync;
    }
}
