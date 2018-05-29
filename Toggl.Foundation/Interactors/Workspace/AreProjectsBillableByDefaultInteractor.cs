using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;

namespace Toggl.Foundation.Interactors
{
    internal sealed class AreProjectsBillableByDefaultInteractor : WorkspaceHasFeatureInteractor<bool?>
    {
        private readonly long workspaceId;

        public AreProjectsBillableByDefaultInteractor(ITogglDataSource dataSource, long workspaceId)
            : base(dataSource)
        {
            this.workspaceId = workspaceId;
        }

        public override IObservable<bool?> Execute()
            => CheckIfFeatureIsEnabled(workspaceId, WorkspaceFeatureId.Pro)
                .SelectMany(isPro =>
                {
                    if (!isPro)
                        return Observable.Return<bool?>(null);

                    return DataSource.Workspaces
                        .GetById(workspaceId)
                        .Select<IThreadSafeWorkspace, bool?>(workspace => workspace.ProjectsBillableByDefault);
                });

    }
}
