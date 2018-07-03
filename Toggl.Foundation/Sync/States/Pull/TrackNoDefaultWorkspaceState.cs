namespace Toggl.Foundation.Sync.States.Pull
{
    using System;
    using System.Reactive.Linq;
    using Toggl.Foundation.Analytics;
    using Toggl.Multivac;
    using Toggl.Multivac.Extensions;
    using Toggl.Multivac.Models;

    internal sealed class TrackNoDefaultWorkspaceState : IPersistState
    {
        private readonly IPersistState internalState;

        private readonly IAnalyticsService analyticsService;

        public StateResult<IFetchObservables> FinishedPersisting { get; } = new StateResult<IFetchObservables>();

        public TrackNoDefaultWorkspaceState(IPersistState internalState, IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(internalState, nameof(internalState));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.internalState = internalState;
            this.analyticsService = analyticsService;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => fetch.GetSingle<IUser>()
                .SelectMany(user => trackIfNoDefaultWorkspace(user, fetch));

        private IObservable<ITransition> trackIfNoDefaultWorkspace(IUser user, IFetchObservables fetch)
        {
            if (!user.DefaultWorkspaceId.HasValue)
            {
                analyticsService.NoDefaultWorkspace.Track();
            }
            return internalState.Start(fetch)
                .Select(FinishedPersisting.Transition(fetch));
        }
    }
}
