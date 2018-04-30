using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    internal sealed class WorkspaceSyncSelector : ISyncSelector<IDatabaseWorkspace>
    {
        public DateTimeOffset LastModified(IDatabaseWorkspace model)
            => model.At ?? DateTimeOffset.Now;

        public bool IsInSync(IDatabaseWorkspace model)
            => model.SyncStatus == SyncStatus.InSync;
    }
}
