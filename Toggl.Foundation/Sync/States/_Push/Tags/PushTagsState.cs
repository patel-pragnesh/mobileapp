using System;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PushTagsState : BasePushState<IDatabaseTag>
    {
        public PushTagsState(IRepository<IDatabaseTag> repository)
            : base(repository)
        {
        }

        protected override DateTimeOffset LastChange(IDatabaseTag entity)
            => entity.At;

        protected override IDatabaseTag CopyFrom(IDatabaseTag entity)
            => Tag.From(entity);
    }
}
