using System;
using System.Collections.Generic;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistTasksState : BasePersistState<ITask, IDatabaseTask>
    {
        public PersistTasksState(IRepository<IDatabaseTask> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, Task.Clean, sinceParameterRepository, Resolver.ForTasks())
        {
        }

        protected override IObservable<IEnumerable<ITask>> FetchObservable(FetchObservables fetch)
            => fetch.Tasks;

        protected override ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated)
            => new SinceParameters(old, tasks: lastUpdated);
    }
}
