using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class UnsyncableUserState : BaseUnsyncableEntityState<IDatabaseUser>
    {
        public UnsyncableUserState(IRepository<IDatabaseUser> repository) : base(repository)
        {
        }

        protected override bool HasChanged(IDatabaseUser original, IDatabaseUser current)
            => original.At < current.At;

        protected override IDatabaseUser CreateUnsyncableFrom(IDatabaseUser entity, string reason)
            => User.Unsyncable(entity, reason);

        protected override IDatabaseUser CopyFrom(IDatabaseUser entity)
            => User.From(entity);
    }
}
