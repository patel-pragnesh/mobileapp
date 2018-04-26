using System;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using static Toggl.Multivac.Extensions.EnumerableExtensions;

namespace Toggl.Foundation.Interactors
{
    public class ContinueMostRecentTimeEntryInteractor : IInteractor<IObservable<IDatabaseTimeEntry>>
    {
        private readonly IIdProvider idProvider;
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IAnalyticsService analyticsService;

        public ContinueMostRecentTimeEntryInteractor(
            IIdProvider idProvider,
            ITimeService timeService,
            ITogglDataSource dataSource,
            IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.idProvider = idProvider;
            this.dataSource = dataSource;
            this.timeService = timeService;
            this.analyticsService = analyticsService;
        }

        public IObservable<IDatabaseTimeEntry> Execute()
            => dataSource
                .TimeEntries
                .GetAll()
                .Select(timeEntries => timeEntries.MaxBy(te => te.Start))
                .Select(newTimeEntry)
                .SelectMany(dataSource.TimeEntries.Create)
                .Do(_ => dataSource.SyncManager.StartPushSync())
                .Do(_ => analyticsService.TrackStartedTimeEntry(TimeEntryStartOrigin.ContinueMostRecent));

        private IDatabaseTimeEntry newTimeEntry(IDatabaseTimeEntry timeEntry)
            => TimeEntry.Builder
                        .Create(idProvider.GetNextIdentifier())
                        .SetTagIds(timeEntry.TagIds)
                        .SetUserId(timeEntry.UserId)
                        .SetTaskId(timeEntry.TaskId)
                        .SetBillable(timeEntry.Billable)
                        .SetProjectId(timeEntry.ProjectId)
                        .SetAt(timeService.CurrentDateTime)
                        .SetSyncStatus(SyncStatus.SyncNeeded)
                        .SetDescription(timeEntry.Description)
                        .SetStart(timeService.CurrentDateTime)
                        .SetWorkspaceId(timeEntry.WorkspaceId)
                        .Build();
    }
}
