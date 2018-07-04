using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Pull
{
    public interface IPersistState : ISyncState<IFetchObservables>
    {
        StateResult<IFetchObservables> FinishedPersisting { get; }

        StateResult<ApiException> ErrorOccured { get; }
    }
}
