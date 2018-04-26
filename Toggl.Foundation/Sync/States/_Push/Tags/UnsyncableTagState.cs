using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class UnsyncableTagState : BaseUnsyncableEntityState<IDatabaseTag>
    {
        public UnsyncableTagState(IRepository<IDatabaseTag> repository) : base(repository)
        {
        }

        protected override bool HasChanged(IDatabaseTag original, IDatabaseTag current)
            => original.At < current.At;

        protected override IDatabaseTag CreateUnsyncableFrom(IDatabaseTag entity, string reason)
            => Tag.Unsyncable(entity, reason);

        protected override IDatabaseTag CopyFrom(IDatabaseTag entity)
            => Tag.From(entity);
    }
}
