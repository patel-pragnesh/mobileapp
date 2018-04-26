using System;
namespace Toggl.Foundation.Sync
{
    public interface ISyncProgressManager
    {
        IObservable<SyncProgress> Progress { get; }

        IObservable<bool> IsRunningSync { get; }

        void ReportStart();

        void ReportFailure(Exception exception);

        void ReportOfflineMode();

        void ReportFinishing();
    }
}
