using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    internal sealed class TagSyncSelector : ISyncSelector<IDatabaseTag>
    {
        public DateTimeOffset LastModified(IDatabaseTag model)
            => model.At;

        public bool IsInSync(IDatabaseTag model)
            => model.SyncStatus == SyncStatus.InSync;

        public bool IsDeleted(IDatabaseTag model)
            => model.ServerDeletedAt.HasValue;
    }
}
