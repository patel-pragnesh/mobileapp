﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using Toggl.Foundation.Models;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using System.Reactive.Subjects;
using Toggl.Foundation.Analytics;

namespace Toggl.Foundation.DataSources
{
    internal sealed class TimeEntriesDataSource
        : ObservableDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry>,
          ITimeEntriesSource
    {
        private long? currentlyRunningTimeEntryId;

        private readonly ITimeService timeService;
        private readonly Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode> alwaysCreate
            = (a, b) => ConflictResolutionMode.Create;

        private readonly Subject<IThreadSafeTimeEntry> timeEntryStartedSubject = new Subject<IThreadSafeTimeEntry>();
        private readonly Subject<IThreadSafeTimeEntry> timeEntryStoppedSubject = new Subject<IThreadSafeTimeEntry>();
        private readonly Subject<IThreadSafeTimeEntry> timeEntryContinuedSubject = new Subject<IThreadSafeTimeEntry>();
        private readonly Subject<IThreadSafeTimeEntry> suggestionStartedSubject = new Subject<IThreadSafeTimeEntry>();

        public IObservable<IThreadSafeTimeEntry> TimeEntryStarted { get; }
        public IObservable<IThreadSafeTimeEntry> TimeEntryStopped { get; }
        public IObservable<IThreadSafeTimeEntry> TimeEntryContinued { get; }
        public IObservable<IThreadSafeTimeEntry> SuggestionStarted { get; }

        public IObservable<bool> IsEmpty { get; }

        public IObservable<IThreadSafeTimeEntry> CurrentlyRunningTimeEntry { get; }

        protected override IRivalsResolver<IDatabaseTimeEntry> RivalsResolver { get; }

        public TimeEntriesDataSource(
            IRepository<IDatabaseTimeEntry> repository,
            ITimeService timeService)
            : base(repository)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.timeService = timeService;

            CurrentlyRunningTimeEntry = 
                GetAll(te => te.IsDeleted == false && te.Duration == null)
                    .Select(tes => tes.SingleOrDefault())
                    .StartWith()
                    .Merge(Created.Where(te => te.IsRunning()))
                    .Merge(Updated.Where(update => update.Id == currentlyRunningTimeEntryId).Select(update => update.Entity))
                    .Merge(Deleted.Where(id => id == currentlyRunningTimeEntryId).Select(_ => null as IThreadSafeTimeEntry))
                    .Select(runningTimeEntry)
                    .ConnectedReplay();

            IsEmpty =
                Observable.Return(default(IDatabaseTimeEntry))
                    .StartWith()
                    .Merge(Updated.Select(tuple => tuple.Entity))
                    .Merge(Created)
                    .SelectMany(_ => GetAll(te => te.IsDeleted == false))
                    .Select(timeEntries => timeEntries.None());

            RivalsResolver = new TimeEntryRivalsResolver(timeService);

            TimeEntryStarted = timeEntryStartedSubject.AsObservable();
            TimeEntryStopped = timeEntryStoppedSubject.AsObservable();
            SuggestionStarted = suggestionStartedSubject.AsObservable();
            TimeEntryContinued = timeEntryContinuedSubject.AsObservable();
        }

        public override IObservable<IThreadSafeTimeEntry> Create(IThreadSafeTimeEntry entity)
            => Repository.UpdateWithConflictResolution(entity.Id, entity, alwaysCreate, RivalsResolver)
                .OfType<CreateResult<IDatabaseTimeEntry>>()
                .Select(result => result.Entity)
                .Select(Convert)
                .Do(CreatedSubject.OnNext);

        public IObservable<IThreadSafeTimeEntry> Stop(DateTimeOffset stopTime)
            => GetAll(te => te.IsDeleted == false && te.Duration == null)
                .Select(timeEntries => timeEntries.SingleOrDefault() ?? throw new NoRunningTimeEntryException())
                .SelectMany(timeEntry => timeEntry
                    .With((long)(stopTime - timeEntry.Start).TotalSeconds)
                    .Apply(Update))
                .Do(timeEntryStoppedSubject.OnNext);

        public IObservable<Unit> SoftDelete(IThreadSafeTimeEntry timeEntry)
            => Observable.Return(timeEntry)
                .Select(TimeEntry.DirtyDeleted)
                .SelectMany(Repository.Update)
                .Do(entity => DeletedSubject.OnNext(entity.Id))
                .Select(_ => Unit.Default);

        public IObservable<IThreadSafeTimeEntry> Update(EditTimeEntryDto dto)
            => GetById(dto.Id)
                 .Select(te => createUpdatedTimeEntry(te, dto))
                 .SelectMany(Update);

        public void OnTimeEntryStarted(IThreadSafeTimeEntry timeEntry, TimeEntryStartOrigin origin)
        {
            switch (origin)
            {
                case TimeEntryStartOrigin.Continue:
                case TimeEntryStartOrigin.ContinueMostRecent:
                    timeEntryContinuedSubject.OnNext(timeEntry);
                    break;

                case TimeEntryStartOrigin.Manual:
                case TimeEntryStartOrigin.Timer:
                    timeEntryStartedSubject.OnNext(timeEntry);
                    break;

                case TimeEntryStartOrigin.Suggestion:
                    suggestionStartedSubject.OnNext(timeEntry);
                    break;
            }
        }

        protected override IThreadSafeTimeEntry Convert(IDatabaseTimeEntry entity)
            => TimeEntry.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseTimeEntry first, IDatabaseTimeEntry second)
            => Resolver.ForTimeEntries.Resolve(first, second);

        private TimeEntry createUpdatedTimeEntry(IThreadSafeTimeEntry timeEntry, EditTimeEntryDto dto)
            => TimeEntry.Builder.Create(dto.Id)
                        .SetDescription(dto.Description)
                        .SetDuration(dto.StopTime.HasValue ? (long?)(dto.StopTime.Value - dto.StartTime).TotalSeconds : null)
                        .SetTagIds(dto.TagIds)
                        .SetStart(dto.StartTime)
                        .SetTaskId(dto.TaskId)
                        .SetBillable(dto.Billable)
                        .SetProjectId(dto.ProjectId)
                        .SetWorkspaceId(dto.WorkspaceId)
                        .SetUserId(timeEntry.UserId)
                        .SetIsDeleted(timeEntry.IsDeleted)
                        .SetServerDeletedAt(timeEntry.ServerDeletedAt)
                        .SetAt(timeService.CurrentDateTime)
                        .SetSyncStatus(SyncStatus.SyncNeeded)
                        .Build();

        private IThreadSafeTimeEntry runningTimeEntry(IThreadSafeTimeEntry timeEntry)
        {
            timeEntry = timeEntry != null && timeEntry.IsRunning() ? timeEntry : null;
            currentlyRunningTimeEntryId = timeEntry?.Id;
            return timeEntry;
        }
    }
}
