using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class UnsyncableClientState : BaseUnsyncableEntityState<IDatabaseClient>
    {
        public UnsyncableClientState(IRepository<IDatabaseClient> repository) : base(repository)
        {
        }

        protected override bool HasChanged(IDatabaseClient original, IDatabaseClient current)
            => original.At < current.At;

        protected override IDatabaseClient CreateUnsyncableFrom(IDatabaseClient entity, string reason)
            => Client.Unsyncable(entity, reason);

        protected override IDatabaseClient CopyFrom(IDatabaseClient entity)
            => Client.From(entity);
    }
}
