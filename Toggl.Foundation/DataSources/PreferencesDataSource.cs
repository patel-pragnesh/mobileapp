using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class PreferencesDataSource : IPreferencesSource
    {
        private readonly ISingleObjectStorage<IDatabasePreferences> storage;
        private readonly ISubject<IDatabasePreferences> currentPreferencesSubject;

        private IDisposable initializationDisposable;
    
        public IObservable<IDatabasePreferences> Current { get; }

        public PreferencesDataSource(ISingleObjectStorage<IDatabasePreferences> storage)
        {
            Ensure.Argument.IsNotNull(storage, nameof(storage));

            this.storage = storage;

            currentPreferencesSubject = new BehaviorSubject<IDatabasePreferences>(Preferences.DefaultPreferences);

            Current = currentPreferencesSubject.AsObservable();

            // right after login/signup the database does not contain the preferences and retreiving
            // it will fail we can ignore this error because it will be immediately fetched and until
            // then the default preferences will be used
            initializationDisposable = Get().Subscribe(currentPreferencesSubject.OnNext, (Exception _) => { });
        }

        public IObservable<IDatabasePreferences> Get()
            => storage.Single().Select(Preferences.From);

        public IObservable<IDatabasePreferences> Update(EditPreferencesDTO dto)
            => storage
                .Single()
                .Select(preferences => updatedPreferences(preferences, dto))
                .SelectMany(storage.Update)
                .Select(Preferences.From)
                .Do(currentPreferencesSubject.OnNext);

        public IObservable<IEnumerable<IConflictResolutionResult<IDatabasePreferences>>> BatchUpdate(
            IEnumerable<(long Id, IDatabasePreferences Entity)> entities,
            Func<IDatabasePreferences, IDatabasePreferences, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<IDatabasePreferences> rivalsResolver = null)
            => storage.BatchUpdate(entities, conflictResolution, rivalsResolver)
                .Do(processConflictResultionResult);

        public IObservable<IDatabasePreferences> GetById(long id)
            => storage.GetById(id);

        public IObservable<IDatabasePreferences> Create(IDatabasePreferences entity)
            => storage.Create(entity)
                .Select(Preferences.From)
                .Do(currentPreferencesSubject.OnNext);

        public IObservable<IDatabasePreferences> Update(long id, IDatabasePreferences entity)
            => storage.Update(id, entity)
                .Select(Preferences.From)
                .Do(currentPreferencesSubject.OnNext);

        public IObservable<Unit> Delete(long id)
        {
            throw new InvalidOperationException("Preferences cannot be deleted.");
        }

        public IObservable<IEnumerable<IDatabasePreferences>> GetAll()
            => storage.GetAll().Select(preferences => preferences.Select(Preferences.From));

        public IObservable<IEnumerable<IDatabasePreferences>> GetAll(Func<IDatabasePreferences, bool> predicate)
            => storage.GetAll(predicate).Select(preferences => preferences.Select(Preferences.From));

        private void processConflictResultionResult(IEnumerable<IConflictResolutionResult<IDatabasePreferences>> batchResult)
        {
            var preferences = batchResult.FirstOrDefault();

            if (preferences == null) return;

            switch (preferences)
            {
                case CreateResult<IDatabasePreferences> created:
                    var createdEntity = Preferences.From(created.Entity);
                    currentPreferencesSubject.OnNext(createdEntity);
                    break;

                case UpdateResult<IDatabasePreferences> updated:
                    var updatedEntity = Preferences.From(updated.Entity);
                    currentPreferencesSubject.OnNext(updatedEntity);
                    break;
            }
        }

        private IDatabasePreferences updatedPreferences(IDatabasePreferences existing, EditPreferencesDTO dto)
            => existing.With(
                dateFormat: dto.DateFormat,
                durationFormat: dto.DurationFormat,
                timeOfDayFormat: dto.TimeOfDayFormat,
                collapseTimeEntries: dto.CollapseTimeEntries,
                syncStatus: SyncStatus.SyncNeeded
                );
    }
}
