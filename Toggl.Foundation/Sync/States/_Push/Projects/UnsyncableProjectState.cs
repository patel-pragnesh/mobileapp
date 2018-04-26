using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class UnsyncableProjectState : BaseUnsyncableEntityState<IDatabaseProject>
    {
        public UnsyncableProjectState(IRepository<IDatabaseProject> repository) : base(repository)
        {
        }

        protected override bool HasChanged(IDatabaseProject original, IDatabaseProject current)
            => original.At < current.At;

        protected override IDatabaseProject CreateUnsyncableFrom(IDatabaseProject entity, string reason)
            => Project.Unsyncable(entity, reason);

        protected override IDatabaseProject CopyFrom(IDatabaseProject entity)
            => Project.From(entity);
    }
}
