using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static Toggl.Foundation.Sync.SyncProgress;

namespace Toggl.Foundation.Sync
{
    public sealed class SyncProgressManager : ISyncProgressManager
    {
        private readonly object progressLock = new object();

        private readonly ISubject<SyncProgress> progressSubject;
        private readonly ISubject<bool> isRunningSyncSubject;

        public IObservable<SyncProgress> Progress { get; }

        public IObservable<bool> IsRunningSync { get; }

        public SyncProgressManager()
        {
            progressSubject = new BehaviorSubject<SyncProgress>(Unknown);
            Progress = progressSubject.AsObservable();

            isRunningSyncSubject = new BehaviorSubject<bool>(false);
            IsRunningSync = isRunningSyncSubject.AsObservable();
        }

        public void ReportStart()
        {
            lock (progressLock)
            {
                isRunningSyncSubject.OnNext(true);
                progressSubject.OnNext(Syncing);
            }
        }

        public void ReportFailure(Exception exception)
        {
            lock (progressLock)
            {
                isRunningSyncSubject.OnNext(false);
                progressSubject.OnNext(Failed);
            }
        }

        public void ReportOfflineMode()
        {
            lock (progressLock)
            {
                isRunningSyncSubject.OnNext(false);
                progressSubject.OnNext(OfflineModeDetected);
            }
        }

        public void ReportFinishing()
        {
            lock (progressLock)
            {
                isRunningSyncSubject.OnNext(false);
                progressSubject.OnNext(Synced);
            }
        }
    }
}
