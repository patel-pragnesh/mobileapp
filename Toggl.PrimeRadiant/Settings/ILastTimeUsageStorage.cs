using System;

namespace Toggl.PrimeRadiant.Settings
{
    public interface ILastTimeUsageStorage
    {
        DateTimeOffset? LastSyncAttempt { get; }

        DateTimeOffset? LastSuccessfulSync { get; }

        DateTimeOffset? LastLogin { get; }

        void SetFullSyncAttempt(DateTimeOffset now);

        void SetSuccessfulFullSync(DateTimeOffset now);

        void SetLogin(DateTimeOffset now);
    }
}
