using System;
using System.Collections.Generic;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistWorkspacesState : BasePersistState<IWorkspace, IDatabaseWorkspace>
    {
        public PersistWorkspacesState(IRepository<IDatabaseWorkspace> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, Workspace.Clean, sinceParameterRepository, Resolver.ForWorkspaces())
        {
        }

        protected override IObservable<IEnumerable<IWorkspace>> FetchObservable(FetchObservables fetch)
            => fetch.Workspaces;

        protected override ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated)
            => new SinceParameters(old, workspaces: lastUpdated);
    }
}
