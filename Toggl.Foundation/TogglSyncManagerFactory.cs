using System;
using Toggl.Foundation.Sync;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using System.Reactive.Concurrency;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;
using Toggl.Foundation.DataSources;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using Toggl.Foundation.Analytics;

namespace Toggl.Foundation
{
    public static class TogglSyncManager
    {
        public static ISyncManager CreateSyncManager(
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            TimeSpan? retryLimit,
            IScheduler scheduler)
        {
            var queue = new SyncStateQueue();
            return new SyncManager(queue, analyticsService);
        }
    }
}
