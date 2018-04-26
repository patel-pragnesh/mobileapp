using System;

namespace Toggl.Foundation.Sync
{
    public interface ISyncManager
    {
        IObservable<SyncProgress> ProgressObservable { get; }
        IObservable<bool> IsRunningSyncObservable { get; }

        void StartPushSync();
        void StartFullSync();
        void Freeze();
    }
}
