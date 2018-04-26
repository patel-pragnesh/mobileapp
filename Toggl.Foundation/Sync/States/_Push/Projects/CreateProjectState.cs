using System;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class CreateProjectState : BaseCreateEntityState<IDatabaseProject>
    {
        public CreateProjectState(ITogglApi api, IRepository<IDatabaseProject> repository) : base(api, repository)
        {
        }

        protected override IObservable<IDatabaseProject> Create(IDatabaseProject entity)
            => Api.Projects.Create(entity).Select(Project.Clean);

        protected override IDatabaseProject CopyFrom(IDatabaseProject entity)
            => Project.From(entity);
    }
}
