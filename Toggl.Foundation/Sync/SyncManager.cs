using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Foundation.Analytics;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync
{
    public sealed class SyncManager : ISyncManager
    {
        private readonly ISyncStateQueue queue;
        private readonly IAnalyticsService analyticsService;
        private readonly ISyncProgressManager syncProgressManager;
        private readonly IStateMachine stateMachine;

        private readonly ISubject<bool> isFrozenSubject;

        private readonly IObservable<bool> abortObservable;

        public IObservable<SyncProgress> ProgressObservable => syncProgressManager.Progress;

        public IObservable<bool> IsRunningSyncObservable => syncProgressManager.IsRunningSync;

        public SyncManager(
            ISyncStateQueue queue,
            IAnalyticsService analyticsService,
            ISyncProgressManager syncProgressManager,
            IStateMachine stateMachine)
        {
            Ensure.Argument.IsNotNull(queue, nameof(queue));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(syncProgressManager, nameof(syncProgressManager));
            Ensure.Argument.IsNotNull(stateMachine, nameof(stateMachine));

            this.queue = queue;
            this.analyticsService = analyticsService;
            this.syncProgressManager = syncProgressManager;
            this.stateMachine = stateMachine;

            isFrozenSubject = new BehaviorSubject<bool>(false);
            abortObservable = isFrozenSubject.AsObservable().Select(isFrozen => !isFrozen);
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

            syncProgressManager.ReportStart();

            try
            {
                await stateMachine.Run(queue, abortObservable);
            }
            catch (Exception exception)
            {
                processError(exception);
                return;
            }

            syncProgressManager.ReportFinishing();
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
