using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    internal sealed class TaskSyncSelector : ISyncSelector<IDatabaseTask>
    {
        public DateTimeOffset LastModified(IDatabaseTask model)
            => model.At;

        public bool IsInSync(IDatabaseTask model)
            => model.SyncStatus == SyncStatus.InSync;
    }
}
