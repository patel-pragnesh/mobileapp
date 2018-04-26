using System;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PushProjectsState : BasePushState<IDatabaseProject>
    {
        public PushProjectsState(IRepository<IDatabaseProject> repository)
            : base(repository)
        {
        }

        protected override DateTimeOffset LastChange(IDatabaseProject entity)
            => entity.At;

        protected override IDatabaseProject CopyFrom(IDatabaseProject entity)
            => Project.From(entity);
    }
}
