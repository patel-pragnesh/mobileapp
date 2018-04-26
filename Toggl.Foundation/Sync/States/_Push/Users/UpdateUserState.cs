using System;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class UpdateUserState : BaseUpdateEntityState<IDatabaseUser>
    {
        public UpdateUserState(ITogglApi api, IRepository<IDatabaseUser> repository) : base(api, repository)
        {
        }

        protected override bool HasChanged(IDatabaseUser original, IDatabaseUser current)
            => original.At < current.At;

        protected override IObservable<IDatabaseUser> Update(IDatabaseUser entity)
            => Api.User.Update(entity).Select(User.Clean);

        protected override IDatabaseUser CopyFrom(IDatabaseUser entity)
            => User.From(entity);
    }
}
