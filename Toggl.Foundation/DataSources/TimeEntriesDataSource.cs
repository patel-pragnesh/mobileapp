using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.Models;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Sync.ConflictResolution;

namespace Toggl.Foundation.DataSources
{
    internal sealed class TimeEntriesDataSource : ITimeEntriesSource
    {
        private long? currentlyRunningTimeEntryId;

        private readonly IIdProvider idProvider;
        private readonly ITimeService timeService;
        private readonly TimeEntryRivalsResolver rivalsResolver;
        private readonly IRepository<IDatabaseTimeEntry> repository;
        private readonly Subject<long> timeEntryDeletedSubject = new Subject<long>();
        private readonly Subject<IDatabaseTimeEntry> timeEntryCreatedSubject = new Subject<IDatabaseTimeEntry>();
        private readonly Subject<(long Id, IDatabaseTimeEntry Entity)> timeEntryUpdatedSubject = new Subject<(long, IDatabaseTimeEntry)>();
        private readonly Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode> alwaysCreate = (a, b) => ConflictResolutionMode.Create;

        public IObservable<bool> IsEmpty { get; }

        public IObservable<IDatabaseTimeEntry> CurrentlyRunningTimeEntry { get; }

        public IObservable<IDatabaseTimeEntry> TimeEntryCreated { get; }

        public IObservable<(long Id, IDatabaseTimeEntry Entity)> TimeEntryUpdated { get; }

        public IObservable<long> TimeEntryDeleted { get; }

        public TimeEntriesDataSource(
            IIdProvider idProvider,
            IRepository<IDatabaseTimeEntry> repository,
            ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(repository, nameof(repository));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.repository = repository;
            this.idProvider = idProvider;
            this.timeService = timeService;

            TimeEntryCreated = timeEntryCreatedSubject.AsObservable();
            TimeEntryUpdated = timeEntryUpdatedSubject.AsObservable();
            TimeEntryDeleted = timeEntryDeletedSubject.AsObservable();
            CurrentlyRunningTimeEntry =
                repository.GetAll(te => te.Duration == null)
                    .Select(tes => tes.SingleOrDefault())
                    .StartWith()
                    .Merge(TimeEntryCreated.Where(te => te.IsRunning()))
                    .Merge(TimeEntryUpdated.Where(tuple => tuple.Entity.Id == currentlyRunningTimeEntryId).Select(tuple => tuple.Entity))
                    .Merge(TimeEntryDeleted.Where(id => id == currentlyRunningTimeEntryId).Select(_ => null as IDatabaseTimeEntry))
                    .Select(runningTimeEntry)
                    .Do(setRunningTimeEntryId)
                    .ConnectedReplay();

            IsEmpty =
                Observable.Return(default(IDatabaseTimeEntry))
                    .StartWith()
                    .Merge(TimeEntryUpdated.Select(tuple => tuple.Entity))
                    .Merge(TimeEntryCreated)
                    .SelectMany(_ => GetAll())
                    .Select(timeEntries => !timeEntries.Any());

            rivalsResolver = new TimeEntryRivalsResolver(timeService);
        }

        public IObservable<IEnumerable<IDatabaseTimeEntry>> GetAll()
            => repository.GetAll(te => !te.IsDeleted);

        public IObservable<IEnumerable<IDatabaseTimeEntry>> GetAll(Func<IDatabaseTimeEntry, bool> predicate)
            => repository.GetAll(predicate).Select(tes => tes.Where(te => !te.IsDeleted));

        public IObservable<IDatabaseTimeEntry> GetById(long id)
            => repository.GetById(id);

        public IObservable<Unit> Delete(long id)
            => repository.GetById(id)
                         .Select(TimeEntry.DirtyDeleted)
                         .SelectMany(repository.Update)
                         .Select(TimeEntry.DirtyDeleted)
                         .Do(te => timeEntryDeletedSubject.OnNext(te.Id))
                         .IgnoreElements()
                         .Cast<Unit>()
                         .Concat(Observable.Return(Unit.Default));

        public IObservable<IDatabaseTimeEntry> Stop(DateTimeOffset stopTime)
            => repository
                    .GetAll(te => te.Duration == null)
                    .Select(timeEntries => timeEntries.SingleOrDefault() ?? throw new NoRunningTimeEntryException())
                    .SelectMany(timeEntry => timeEntry
                        .With((long)(stopTime - timeEntry.Start).TotalSeconds)
                        .Apply(this.Update));

        public IObservable<IDatabaseTimeEntry> Update(EditTimeEntryDto dto)
            => repository.GetById(dto.Id)
                         .Select(te => createUpdatedTimeEntry(te, dto))
                         .SelectMany(this.Update);

        public IObservable<IDatabaseTimeEntry> Create(IDatabaseTimeEntry entity)
            => repository
                .UpdateWithConflictResolution(entity.Id, entity, alwaysCreate, rivalsResolver)
                .Select(result => ((CreateResult<IDatabaseTimeEntry>)result).Entity)
                .Select(TimeEntry.From)
                .Do(timeEntryCreatedSubject.OnNext);

        public IObservable<IDatabaseTimeEntry> Update(long id, IDatabaseTimeEntry entity)
            => repository.Update(id, entity)
                .Do(updatedEntity => maybeUpdateCurrentlyRunningTimeEntryId(id, updatedEntity))
                .Do(updatedEntity => timeEntryUpdatedSubject.OnNext((id, TimeEntry.From(updatedEntity))));

        public IObservable<IEnumerable<IConflictResolutionResult<IDatabaseTimeEntry>>> BatchUpdate(
            IEnumerable<(long Id, IDatabaseTimeEntry Entity)> entities,
            Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<IDatabaseTimeEntry> rivalsResolver = null)
            => repository
                .BatchUpdate(entities, conflictResolution, rivalsResolver)
                .Do(updatedEntities => updatedEntities
                    .Where(result => !(result is IgnoreResult<IDatabaseTimeEntry>))
                    .ForEach(handleBatchUpdateResult));

        private void handleBatchUpdateResult(IConflictResolutionResult<IDatabaseTimeEntry> result)
        {
            switch (result)
            {
                case DeleteResult<IDatabaseTimeEntry> d:
                    timeEntryDeletedSubject.OnNext(d.Id);
                    return;

                case CreateResult<IDatabaseTimeEntry> c:
                    timeEntryCreatedSubject.OnNext(TimeEntry.From(c.Entity));
                    return;

                case UpdateResult<IDatabaseTimeEntry> u:
                    timeEntryUpdatedSubject.OnNext((u.OriginalId, TimeEntry.From(u.Entity)));
                    return;
            }
        }

        private TimeEntry createUpdatedTimeEntry(IDatabaseTimeEntry timeEntry, EditTimeEntryDto dto)
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

        private IDatabaseTimeEntry runningTimeEntry(IDatabaseTimeEntry timeEntry)
        {
            if (timeEntry == null || !timeEntry.IsRunning())
                return null;
           
            return TimeEntry.From(timeEntry);
        }

        private void setRunningTimeEntryId(IDatabaseTimeEntry timeEntry)
        {
            currentlyRunningTimeEntryId = timeEntry?.Id;
        }

        private void maybeUpdateCurrentlyRunningTimeEntryId(long id, IDatabaseTimeEntry entity)
        {
            if (id == currentlyRunningTimeEntryId)
                currentlyRunningTimeEntryId = entity.Id;
        }
    }
}
