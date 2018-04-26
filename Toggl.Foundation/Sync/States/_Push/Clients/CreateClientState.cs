using System;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class CreateClientState : BaseCreateEntityState<IDatabaseClient>
    {
        public CreateClientState(ITogglApi api, IRepository<IDatabaseClient> repository) : base(api, repository)
        {
        }

        protected override IObservable<IDatabaseClient> Create(IDatabaseClient entity)
            => Api.Clients.Create(entity).Select(Client.Clean);

        protected override IDatabaseClient CopyFrom(IDatabaseClient entity)
            => Client.From(entity);
    }
}
