using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    internal sealed class TagSyncSelector : ISyncSelector<IDatabaseTag>
    {
        public bool IsInSync(IDatabaseTag model)
            => model.SyncStatus == SyncStatus.InSync;
    }
}
