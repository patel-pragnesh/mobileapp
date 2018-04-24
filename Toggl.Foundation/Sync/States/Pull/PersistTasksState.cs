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
    internal sealed class PersistTasksState : BasePersistState<ITask, IDatabaseTask>
    {
        public PersistTasksState(IRepository<IDatabaseTask> repository, ISinceParameterRepository sinceParameterRepository)
            : base(repository, sinceParameterRepository, Resolver.ForTasks())
        {
        }

        protected override IObservable<IEnumerable<ITask>> FetchObservable(FetchObservables fetch)
            => fetch.Tasks;

        protected override IDatabaseTask ConvertToDatabaseEntity(ITask entity)
            => Task.Clean(entity);

        protected override ISinceParameters UpdateSinceParameters(ISinceParameters old, DateTimeOffset? lastUpdated)
            => new SinceParameters(old, tasks: lastUpdated);
    }
}
