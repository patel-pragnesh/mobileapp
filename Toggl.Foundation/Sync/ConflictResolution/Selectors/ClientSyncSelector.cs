using System;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution.Selectors
{
    internal sealed class ClientSyncSelector : ISyncSelector<IDatabaseClient>
    {
        public DateTimeOffset LastModified(IDatabaseClient model)
            => model.At;

        public bool IsInSync(IDatabaseClient model)
            => model.SyncStatus == SyncStatus.InSync;
    }
}
