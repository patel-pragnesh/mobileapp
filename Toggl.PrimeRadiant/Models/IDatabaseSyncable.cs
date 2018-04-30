namespace Toggl.PrimeRadiant
{
    public interface IDatabaseSyncable
    {
        SyncStatus SyncStatus { get; }

        string LastSyncErrorMessage { get; }

        bool IsDeleted { get; }
    }
}
