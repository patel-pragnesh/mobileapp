using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Sync.EntryPoints;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync
{
    public static class TogglSyncManagerFactory
    {
        private const int maximumNumberOfRetries = 3;

        public static ISyncManager CreateSyncManager(
            ITogglDatabase database,
            ITogglApi api,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            TimeSpan delayLimit,
            IScheduler scheduler)
        {
            var random = new Random();
            var apiDelay = new RetryDelayService(random, delayLimit);
            var statusDelay = new RetryDelayService(random);
            var delayCancellation = new Subject<Unit>();
            var delayCancellationObservable = delayCancellation.AsObservable().Replay();
            var stateMachine = new StateMachine(
                apiDelay,
                new PullSyncEntryPointFactory(database, api, timeService),
                null,
                new RetryLoopEntryPointFactory(api, scheduler, apiDelay, statusDelay, delayCancellationObservable), 
                maximumNumberOfRetries);
            var queue = new SyncStateQueue();
            var syncProgressManager = new SyncProgressManager();
            return new SyncManager(queue, analyticsService, syncProgressManager, stateMachine);
        }
    }
}
