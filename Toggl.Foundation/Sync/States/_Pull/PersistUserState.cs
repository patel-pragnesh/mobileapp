using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistUserState : BasePersistState<IUser, IDatabaseUser>
    {
        public PersistUserState(IRepository<IDatabaseUser> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, sinceParameterRepository, Resolver.ForUser())
        {
        }

        protected override IDatabaseUser ConvertToDatabaseEntity(IUser entity)
            => User.Clean(entity);

        protected override IObservable<IEnumerable<IUser>> FetchObservable(FetchObservables fetch)
            => fetch.User.Select(user
                => user == null
                    ? new User[0]
                    : new[] { user });

        protected override long GetId(IDatabaseUser entity)
            => entity.Id;

        protected override DateTimeOffset? LastUpdated(ISinceParameters old, IEnumerable<IDatabaseUser> entities)
            => null;

        protected override ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated)
            => old;
    }
}
