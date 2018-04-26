using System;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PushUsersState : BasePushState<IDatabaseUser>
    {
        public PushUsersState(IRepository<IDatabaseUser> repository)
            : base(repository)
        {
        }

        protected override DateTimeOffset LastChange(IDatabaseUser entity)
            => entity.At;

        protected override IDatabaseUser CopyFrom(IDatabaseUser entity)
            => User.From(entity);
    }
}
