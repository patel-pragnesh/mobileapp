using System;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Exceptions;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.States.Pull
{
    internal sealed class NoWorkspaceDetectingState : ISyncState<IFetchObservables>
    {
        public StateResult<IFetchObservables> Continue { get; } = new StateResult<IFetchObservables>();

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => fetch.GetList<IWorkspace>()
                .Select(workspaces => workspaces.Any()
                    ? Continue.Transition(fetch)
                    : throw new NoWorkspaceException());
    }
}
