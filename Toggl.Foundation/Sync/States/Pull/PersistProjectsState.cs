using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistProjectsState : BasePersistState<IProject, IDatabaseProject>
    {
        public PersistProjectsState(IRepository<IDatabaseProject> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, sinceParameterRepository, Resolver.ForProjects())
        {
        }

        protected override IObservable<IEnumerable<IProject>> FetchObservable(FetchObservables fetch)
            => fetch.Projects;

        protected override IDatabaseProject ConvertToDatabaseEntity(IProject entity)
            => Project.Clean(entity);

        protected override ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated)
            => new SinceParameters(old, projects: lastUpdated);
    }
}
