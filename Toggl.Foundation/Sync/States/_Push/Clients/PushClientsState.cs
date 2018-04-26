using System;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PushClientsState : BasePushState<IDatabaseClient>
    {
        public PushClientsState(IRepository<IDatabaseClient> repository)
            : base(repository)
        {
        }

        protected override DateTimeOffset LastChange(IDatabaseClient entity)
            => entity.At;

        protected override IDatabaseClient CopyFrom(IDatabaseClient entity)
            => Client.From(entity);
    }
}
