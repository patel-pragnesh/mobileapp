using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using static Toggl.Multivac.WorkspaceFeatureId;

namespace Toggl.Foundation.Interactors
{
    internal sealed class IsBillableAvailableForProjectInteractor : WorkspaceHasFeatureInteractor<bool>
    {
        private readonly long projectId;

        public IsBillableAvailableForProjectInteractor(ITogglDatabase database, long projectId)
            : base (database)
        {
            this.projectId = projectId;
        }

        public override IObservable<bool> Execute() 
            => Database.Projects.GetById(projectId)
                .SelectMany(project => CheckIfFeatureIsEnabled(project.WorkspaceId, Pro));
    }
}
