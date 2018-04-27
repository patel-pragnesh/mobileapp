﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave.Exceptions;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Sync.States
{
    internal abstract class BasePersistState<TInterface, TDatabaseInterface>
        where TInterface : IIdentifiable
        where TDatabaseInterface : TInterface
    {
        private readonly IRepository<TDatabaseInterface> repository;

        private readonly Func<TInterface, TDatabaseInterface> convertToDatabaseEntity;

        private readonly ISinceParameterRepository sinceParameterRepository;

        private readonly IConflictResolver<TDatabaseInterface> conflictResolver;

        private readonly IRivalsResolver<TDatabaseInterface> rivalsResolver;

        public StateResult<FetchObservables> FinishedPersisting { get; } = new StateResult<FetchObservables>();

        public StateResult<Exception> Failed { get; } = new StateResult<Exception>();

        protected BasePersistState(
            IRepository<TDatabaseInterface> repository,
            Func<TInterface, TDatabaseInterface> convertToDatabaseEntity,
            ISinceParameterRepository sinceParameterRepository,
            IConflictResolver<TDatabaseInterface> conflictResolver,
            IRivalsResolver<TDatabaseInterface> rivalsResolver = null)
        {
            this.repository = repository;
            this.convertToDatabaseEntity = convertToDatabaseEntity;
            this.sinceParameterRepository = sinceParameterRepository;
            this.conflictResolver = conflictResolver;
            this.rivalsResolver = rivalsResolver;
        }

        public IObservable<ITransition> Start(FetchObservables fetch)
            => FetchObservable(fetch)
                .SingleAsync()
                .Select(listOfDatabaseEntities)
                .SelectMany(batchUpdate)
                .Select(updateFetchObservable(fetch))
                .Select(FinishedPersisting.Transition)
                .Catch((Exception exception) => processError(exception));

        private IList<TDatabaseInterface> listOfDatabaseEntities(IEnumerable<TInterface> entities)
            => entities?.Where(entity => entity != null).Select(convertToDatabaseEntity).ToList()
                ?? new List<TDatabaseInterface>();

        private IObservable<IList<ILastChangeDatable>> batchUpdate(IEnumerable<TDatabaseInterface> databaseEntities)
            => repository.BatchUpdate(databaseEntities.Select(entity => (entity.Id, entity)), conflictResolver.Resolve, rivalsResolver)
                .IgnoreElements()
                .OfType<IList<ILastChangeDatable>>()
                .Concat(Observable.Return(databaseEntities.OfType<ILastChangeDatable>().ToList()));

        private Func<IList<ILastChangeDatable>, FetchObservables> updateFetchObservable(FetchObservables fetch)
            => (IList<ILastChangeDatable> listOfSyncable) =>
            {
                if (listOfSyncable.Count == 0)
                    return fetch;

                var lastUpdatedDate = listOfSyncable.Select(entity => entity.At).Max();
                var sinceParameters = UpdateSinceParameters(fetch.SinceParameters, lastUpdatedDate);
                sinceParameterRepository.Set(sinceParameters);
                return new FetchObservables(fetch, sinceParameters);
            };

        private IObservable<ITransition> processError(Exception exception)
            => shouldRethrow(exception)
                ? Observable.Throw<ITransition>(exception)
                : Observable.Return(Failed.Transition(exception));

        private bool shouldRethrow(Exception e)
            => e is ApiException == false || e is ApiDeprecatedException || e is ClientDeprecatedException || e is UnauthorizedException;

        protected abstract IObservable<IEnumerable<TInterface>> FetchObservable(FetchObservables fetch);

        protected abstract ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated);
    }
}
