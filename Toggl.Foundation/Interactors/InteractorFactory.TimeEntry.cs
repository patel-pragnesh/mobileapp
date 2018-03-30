using System;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Models;
using Toggl.Foundation.Suggestions;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<IDatabaseTimeEntry>> CreateTimeEntry(ITimeEntryPrototype prototype)
            => new CreateTimeEntryInteractor(
                idProvider,
                timeService,
                dataSource,
                analyticsService,
                prototype,
                prototype.StartTime,
                prototype.Duration);

        public IInteractor<IObservable<IDatabaseTimeEntry>> ContinueTimeEntry(ITimeEntryPrototype prototype)
            => new CreateTimeEntryInteractor(
                idProvider,
                timeService,
                dataSource,
                analyticsService,
                prototype,
                timeService.CurrentDateTime,
                null,
                TimeEntryStartOrigin.Continue);

        public IInteractor<IObservable<IDatabaseTimeEntry>> StartSuggestion(Suggestion suggestion)
            => new CreateTimeEntryInteractor(
                idProvider,
                timeService,
                dataSource,
                analyticsService,
                suggestion,
                timeService.CurrentDateTime,
                null,
            TimeEntryStartOrigin.Suggestion);

        public IInteractor<IObservable<IDatabaseTimeEntry>> ContinueMostRecentTimeEntry()
            => new ContinueMostRecentTimeEntryInteractor(
                idProvider,
                timeService,
                dataSource,
                analyticsService);
    }
}
