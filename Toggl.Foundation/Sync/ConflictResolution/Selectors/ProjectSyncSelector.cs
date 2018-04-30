using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    internal sealed class ProjectSyncSelector : ISyncSelector<IDatabaseProject>
    {
        public bool IsInSync(IDatabaseProject model)
            => model.SyncStatus == SyncStatus.InSync;
    }
}
