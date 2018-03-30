using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Interactors
{
    internal sealed class ProjectDefaultsToBillableInteractor : IInteractor<IObservable<bool>>
    {
        private readonly long projectId;
        private readonly ITogglDatabase database;

        public ProjectDefaultsToBillableInteractor(ITogglDatabase database, long projectId)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.projectId = projectId;
            this.database = database;
        }

        public IObservable<bool> Execute()
            => database.Projects
                .GetById(projectId)
                .Select(project => project.Billable ?? false);
    }
}
