using System;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using static Toggl.Multivac.WorkspaceFeatureId;

namespace Toggl.Foundation.Interactors
{
    internal sealed class IsBillableAvailableForWorkspaceInteractor : WorkspaceHasFeatureInteractor<bool>
    {
        private readonly long workspaceId;

        public IsBillableAvailableForWorkspaceInteractor(ITogglDatabase database, long workspaceId)
            : base(database)
        {
            this.workspaceId = workspaceId;
        }

        public override IObservable<bool> Execute()
            => CheckIfFeatureIsEnabled(workspaceId, Pro);
    }
}
