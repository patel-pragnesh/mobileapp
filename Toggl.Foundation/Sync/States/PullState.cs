using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Foundation.Sync.States.Results;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States
{
    public sealed class PullState<TInterface, TDatabaseInterface> : IState, ISpeculativePreloadable
        where TInterface : IBaseModel
        where TDatabaseInterface : IDatabaseSyncable
    {
        private readonly IRepository<TInterface> repository;

        private readonly ISinceParameterRepository sinceParameterRepository;

        private readonly IConflictResolver<TDatabaseInterface> conflictResolver;

        private readonly IRivalsResolver<TDatabaseInterface> rivalsResolver;

        private readonly Func<TInterface, TDatabaseInterface> convertToDatabaseEntity;

        private readonly Func<DateTimeOffset?> getSince;

        private readonly IState nextState;

        private IObservable<List<TInterface>> fetchObservable;

        public PullState(IState nextState)
        {
            Ensure.Argument.IsNotNull(nextState, nameof(nextState));

            this.nextState = nextState;
        }

        public IEnumerable<IState> AllPossibleOutcomes => throw new NotImplementedException();

        public IEnumerable<ISpeculativePreloadable> Preload()
        {
            throw new NotImplementedException();
        }

        public IObservable<IResult> Run(IObservable<Unit> abort)
            => fetchIfNeeded()
                .Do(_ => fetchObservable = null)
                .Select(entities => entities ?? new List<TInterface>())
                    .Select(entities => entities.Select(convertToDatabaseEntity).ToList())
                    .SelectMany(databaseEntities =>
                        repository.BatchUpdate(databaseEntities.Select(entity => (entity.Id, entity)), conflictResolver.Resolve, rivalsResolver)
                            .IgnoreElements()
                            .OfType<List<TDatabaseInterface>>()
                            .Concat(Observable.Return(databaseEntities)))
                    .Select(databaseEntities => lastUpdated(databaseEntities))
                    .Select(lastUpdated => updateSinceParameters(since, lastUpdated))
                    .Do(sinceParameterRepository.Set)
                    .Select(_ => new Success(nextState))
                    .Catch((Exception exception) => processError(exception));

        private IObservable<List<TInterface>> fetchIfNeeded()
        {
            if (fetchObservable == null)
            {
                fetchObservable = null; // TODO
            }

            return fetchObservable;
        }
        
        private IObservable<IResult> processError(Exception exception)
            => shouldRethrow(exception)
                ? Observable.Throw<IResult>(exception)
                : Observable.Return(new Success(exception));

        private bool shouldRethrow(Exception e)
            => e is ApiException == false || e is ApiDeprecatedException || e is ClientDeprecatedException || e is UnauthorizedException;

        private DateTimeOffset lastUpdated(IEnumerable<TDatabaseInterface> entities)
            => entities.Select(p => p?.At).Where(d => d.HasValue).DefaultIfEmpty(getSince()).Max();
    }
}
