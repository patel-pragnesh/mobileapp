using System;
using System.Collections.Generic;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistTagsState : BasePersistState<ITag, IDatabaseTag>
    {
        public PersistTagsState(IRepository<IDatabaseTag> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, Tag.Clean, sinceParameterRepository, Resolver.ForTags())
        {
        }

        protected override IObservable<IEnumerable<ITag>> FetchObservable(FetchObservables fetch)
            => fetch.Tags;

        protected override ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated)
            => new SinceParameters(old, tags: lastUpdated);
    }
}

