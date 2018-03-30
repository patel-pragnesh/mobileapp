using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    internal sealed class AreProjectsBillableByDefaultInteractor : WorkspaceHasFeatureInteractor<bool?>
    {
        private readonly long workspaceId;

        public AreProjectsBillableByDefaultInteractor(ITogglDatabase database, long workspaceId)
            : base(database)
        {
            this.workspaceId = workspaceId;
        }

        public override IObservable<bool?> Execute()
            => CheckIfFeatureIsEnabled(workspaceId, WorkspaceFeatureId.Pro)
                .SelectMany(isPro =>
                {
                    if (!isPro)
                        return Observable.Return<bool?>(null);

                    return Database.Workspaces
                        .GetById(workspaceId)
                        .Select<IDatabaseWorkspace, bool?>(workspace => workspace.ProjectsBillableByDefault);
                });

    }
}
