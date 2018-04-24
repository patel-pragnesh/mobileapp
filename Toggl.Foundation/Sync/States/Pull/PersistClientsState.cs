using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistClientsState : BasePersistState<IClient, IDatabaseClient>
    {
        public PersistClientsState(IRepository<IDatabaseClient> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, Client.Clean, sinceParameterRepository, Resolver.ForClients())
        {
        }

        protected override IObservable<IEnumerable<IClient>> FetchObservable(FetchObservables fetch)
            => fetch.Clients;

        protected override ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated)
            => new SinceParameters(old, clients: lastUpdated);
    }
}
