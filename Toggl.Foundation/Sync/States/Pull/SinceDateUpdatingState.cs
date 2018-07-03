using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Extensions;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Pull
{
    public sealed class SinceDateUpdatingState<TInterface, TDatabaseInterface> : ISyncState<IFetchObservables>
        where TInterface : ILastChangedDatable
        where TDatabaseInterface : TInterface, IDatabaseSyncable
    {
        private readonly ISinceParameterRepository sinceParameterRepository;

        public StateResult<IFetchObservables> Finished { get; } = new StateResult<IFetchObservables>();

        public SinceDateUpdatingState(ISinceParameterRepository sinceParameterRepository)
        {
            Ensure.Argument.IsNotNull(sinceParameterRepository, nameof(sinceParameterRepository));

            this.sinceParameterRepository = sinceParameterRepository;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => fetch.GetList<TInterface>()
                .Do(maybeUpdateSinceDates)
                .Select(Finished.Transition(fetch));

        private void maybeUpdateSinceDates(List<TInterface> entities)
        {
            var since = entities.Max(entity => entity?.At);
            if (since.HasValue)
            {
                sinceParameterRepository.Set<TDatabaseInterface>(since);
            }
        }
    }
}
