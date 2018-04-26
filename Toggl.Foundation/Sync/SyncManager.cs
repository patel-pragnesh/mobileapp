using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Sync.States;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;
using static Toggl.Foundation.Sync.SyncState;

namespace Toggl.Foundation.Sync
{
    public sealed class SyncManager : ISyncManager
    {
        private readonly object freezingLock = new object();

        private readonly ISyncStateQueue queue;
        private readonly IAnalyticsService analyticsService;
        private readonly ISyncProgressManager syncProgressManager;
        private readonly IState pullSyncEntryPoint;
        private readonly IState pushSyncEntryPoint;

        private readonly ISubject<bool> isFrozenSubject;

        private readonly IObservable<Unit> abortObservable;

        public IObservable<SyncProgress> ProgressObservable => syncProgressManager.Progress;

        public IObservable<bool> IsRunningSyncObservable => syncProgressManager.IsRunningSync;

        public SyncManager(
            ISyncStateQueue queue,
            IAnalyticsService analyticsService,
            ISyncProgressManager syncProgressManager,
            IState pullSyncEntryPoint,
            IState pushSyncEntryPoint)
        {
            Ensure.Argument.IsNotNull(queue, nameof(queue));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(syncProgressManager, nameof(syncProgressManager));
            Ensure.Argument.IsNotNull(pullSyncEntryPoint, nameof(pullSyncEntryPoint));
            Ensure.Argument.IsNotNull(pushSyncEntryPoint, nameof(pushSyncEntryPoint));

            this.queue = queue;
            this.analyticsService = analyticsService;
            this.syncProgressManager = syncProgressManager;
            this.pullSyncEntryPoint = pullSyncEntryPoint;
            this.pushSyncEntryPoint = pushSyncEntryPoint;

            isFrozenSubject = new BehaviorSubject<bool>(false);
            abortObservable = isFrozenSubject.AsObservable().Select(_ => Unit.Default);
        }

        public void StartPushSync()
        {
            queue.QueuePushSync();
            syncIfNeeded().ConfigureAwait(false);
        }

        public void StartFullSync()
        {
            queue.QueuePullSync();
            syncIfNeeded().ConfigureAwait(false);
        }

        public void Freeze()
        {
            isFrozenSubject.OnNext(true);
        }

        private async Task syncIfNeeded()
        {
            var isFrozen = await isFrozenSubject.FirstAsync();
            var isRunningSync = await IsRunningSyncObservable.FirstAsync();

            if (isRunningSync || isFrozen) return;

            await sync();
        }

        private async Task sync()
        {
            syncProgressManager.ReportStart();

            var entryPoint = chooseNextEntryPoint();
            while (entryPoint != null && await isFrozenSubject.FirstAsync() == false)
            {
                try
                {
                    await entryPoint.RunUntilReachingDeadEnd(abortObservable);
                }
                catch (Exception exception)
                {
                    processError(exception);
                    return;
                }

                entryPoint = chooseNextEntryPoint();
            }

            syncProgressManager.ReportFinishing();
        }

        private IState chooseNextEntryPoint()
        {
            var state = queue.Dequeue();

            switch (state)
            {
                case Pull:
                    return pullSyncEntryPoint;

                case Push:
                    return pushSyncEntryPoint;

                default:
                    return null;
            }
        }

        private void processError(Exception error)
        {
            queue.Clear();

            if (error is OfflineException)
            {
                syncProgressManager.ReportOfflineMode();
            }
            else
            {
                syncProgressManager.ReportFailure(error);
                analyticsService.TrackSyncError(error);
            }

            if (error is ClientDeprecatedException
                || error is ApiDeprecatedException
                || error is UnauthorizedException)
            {
                Freeze();
            }
        }
    }
}
