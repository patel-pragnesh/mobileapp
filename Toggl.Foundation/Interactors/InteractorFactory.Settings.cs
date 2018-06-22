using System;
using System.Reactive;
using Toggl.Foundation.Interactors.Settings;

namespace Toggl.Foundation.Interactors
{
    public partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<Unit>> SendFeedback(string message)
            => new SendFeedbackInteractor(
                null, // todo: Inject the API as soon as its PR is merged.
                dataSource.User,
                dataSource.Workspaces,
                dataSource.TimeEntries,
                platformConstants,
                userPreferences,
                lastTimeUsageStorage,
                timeService,
                userAgent,
                message);
    }
}
