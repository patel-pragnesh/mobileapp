using System;
using System.Reactive.Linq;
using Toggl.Foundation.Analytics;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.States.Pull
{
    internal sealed class NoDefaultWorkspaceTrackingState : ISyncState<IFetchObservables>
    {
        private readonly IAnalyticsService analyticsService;

        public StateResult<IFetchObservables> Continue { get; } = new StateResult<IFetchObservables>();

        public NoDefaultWorkspaceTrackingState(IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.analyticsService = analyticsService;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => fetch.GetSingle<IUser>()
                .Do(trackIfNoDefaultWorkspace)
                .Select(Continue.Transition(fetch));

        private void trackIfNoDefaultWorkspace(IUser user)
        {
            if (!user.DefaultWorkspaceId.HasValue)
            {
                analyticsService.NoDefaultWorkspace.Track();
            }
        }
    }
}
