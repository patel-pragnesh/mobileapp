using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    internal sealed class UserSyncSelector : ISyncSelector<IDatabaseUser>
    {
        public bool IsInSync(IDatabaseUser model)
            => model.SyncStatus == SyncStatus.InSync;

        public DateTimeOffset LastModified(IDatabaseUser model)
            => model.At;
    }
}
