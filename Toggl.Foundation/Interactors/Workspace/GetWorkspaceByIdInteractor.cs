using System;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetWorkspaceByIdInteractor : IInteractor<IObservable<IDatabaseWorkspace>>
    {
        private readonly long workspaceId;
        private readonly ITogglDatabase database;

        public GetWorkspaceByIdInteractor(ITogglDatabase database, long workspaceId)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.database = database;
            this.workspaceId = workspaceId;
        }

        public IObservable<IDatabaseWorkspace> Execute()
            => database.Workspaces.GetById(workspaceId);
    }
}
