﻿using System;
using System.Reactive.Linq;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.DTOs;

namespace Toggl.Foundation.Interactors
{
    internal sealed class CreateTimeEntryInteractor : IInteractor<IObservable<IThreadSafeTimeEntry>>
    {
        private readonly TimeSpan? duration;
        private readonly IIdProvider idProvider;
        private readonly DateTimeOffset startTime;
        private readonly ITimeService timeService;
        private readonly TimeEntryStartOrigin origin;
        private readonly ITogglDataSource dataSource;
        private readonly ITimeEntryPrototype prototype;
        private readonly IAnalyticsService analyticsService;

        public CreateTimeEntryInteractor(
            IIdProvider idProvider,
            ITimeService timeService,
            ITogglDataSource dataSource,
            IAnalyticsService analyticsService,
            ITimeEntryPrototype prototype,
            DateTimeOffset startTime,
            TimeSpan? duration)
            : this(idProvider, timeService, dataSource, analyticsService, prototype, startTime, duration,
                prototype.Duration.HasValue ? TimeEntryStartOrigin.Manual : TimeEntryStartOrigin.Timer) { }

        public CreateTimeEntryInteractor(
            IIdProvider idProvider,
            ITimeService timeService,
            ITogglDataSource dataSource,
            IAnalyticsService analyticsService,
            ITimeEntryPrototype prototype,
            DateTimeOffset startTime,
            TimeSpan? duration,
            TimeEntryStartOrigin origin)
        {
            Ensure.Argument.IsNotNull(origin, nameof(origin));
            Ensure.Argument.IsNotNull(prototype, nameof(prototype));
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.origin = origin;
            this.duration = duration;
            this.prototype = prototype;
            this.startTime = startTime;
            this.idProvider = idProvider;
            this.dataSource = dataSource;
            this.timeService = timeService;
            this.analyticsService = analyticsService;
        }

        public IObservable<IThreadSafeTimeEntry> Execute()
            => dataSource.User.Current
                .FirstAsync()
                .Select(user => new TimeEntryDto(
                    id: idProvider.GetNextIdentifier(),
                    at: timeService.CurrentDateTime,
                    workspaceId: prototype.WorkspaceId,
                    projectId: prototype.ProjectId,
                    taskId: prototype.TaskId,
                    billable: prototype.IsBillable,
                    start: startTime,
                    duration: (long?)duration?.TotalSeconds,
                    description: prototype.Description,
                    tagIds: prototype.TagIds,
                    userId: user.Id))
                .SelectMany(dataSource.TimeEntries.Create)
                .Do(notifyOfNewTimeEntryIfPossible)
                .Do(_ => dataSource.SyncManager.PushSync())
                .Track(StartTimeEntryEvent.With(origin), analyticsService);

        private void notifyOfNewTimeEntryIfPossible(IThreadSafeTimeEntry timeEntry)
        {
            if (dataSource.TimeEntries is TimeEntriesDataSource timeEntriesDataSource)
                timeEntriesDataSource.OnTimeEntryStarted(timeEntry, origin);
        }
    }
}
