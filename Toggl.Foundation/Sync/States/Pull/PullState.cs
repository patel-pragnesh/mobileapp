using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using Toggl.Foundation.Sync.States.Results;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Pull
{
    public sealed class PullState<TInterface, TDatabaseInterface> : IState, ISpeculativePreloadable
        where TInterface : IIdentifiable
        where TDatabaseInterface : TInterface
    {
        private readonly IRepository<TDatabaseInterface> repository;
        private readonly Func<TInterface, TDatabaseInterface> convertToDatabaseEntity;
        private readonly Func<IObservable<IEnumerable<TInterface>>> fetch;
        private readonly Func<TDatabaseInterface, TDatabaseInterface, ConflictResolutionMode> conflictResolution;
        private readonly IRivalsResolver<TDatabaseInterface> rivalsResolver;

        private IObservable<IEnumerable<TInterface>> fetchObservable;

        private readonly Proceed proceed;

        public IEnumerable<IResult> AllPossibleOutcomes
            => new IResult[] { proceed, new ApiServerFailure() };

        public PullState(
            Func<IObservable<IEnumerable<TInterface>>> fetch,
            IRepository<TDatabaseInterface> repository,
            Func<TInterface, TDatabaseInterface> convertToDatabaseEntity,
            Func<TDatabaseInterface, TDatabaseInterface, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<TDatabaseInterface> rivalsResolver,
            IState nextState = null)
        {
            Ensure.Argument.IsNotNull(fetch, nameof(fetch));
            Ensure.Argument.IsNotNull(repository, nameof(repository));
            Ensure.Argument.IsNotNull(convertToDatabaseEntity, nameof(convertToDatabaseEntity));
            Ensure.Argument.IsNotNull(conflictResolution, nameof(conflictResolution));
            Ensure.Argument.IsNotNull(rivalsResolver, nameof(rivalsResolver));

            this.fetch = fetch;
            this.repository = repository;
            this.convertToDatabaseEntity = convertToDatabaseEntity;
            this.conflictResolution = conflictResolution;
            this.rivalsResolver = rivalsResolver;

            proceed = new Proceed(nextState);
        }

        public void Preload()
        {
            fetchIfNeeded();

            if (proceed.NextState is ISpeculativePreloadable preloadable)
                preloadable.Preload();
        }

        public IObservable<IResult> Run()
        {
            return fetchIfNeeded()
                .Select(databaseEntities)
                .Do(entities => repository.BatchUpdate(entities, conflictResolution, rivalsResolver))
                .Select(_ => proceed)
                .Catch((Exception exception) => processError(exception));
        }

        private IObservable<IEnumerable<TInterface>> fetchIfNeeded()
            => fetchObservable ?? (fetchObservable = fetch());

        private IList<TDatabaseInterface> databaseEntities(IEnumerable<TInterface> entities)
            => entities?.Where(entity => entity != null).Select(convertToDatabaseEntity).ToList()
               ?? new List<TDatabaseInterface>();

        private IObservable<IResult> processError(Exception exception)
            => shouldRethrow(exception)
                ? Observable.Throw<IResult>(exception)
                : Observable.Return(new ApiServerFailure(exception as ApiException));

        private static bool shouldRethrow(Exception e)
            => e is ApiException == false || e is ApiDeprecatedException || e is ClientDeprecatedException || e is UnauthorizedException;
    }
}
