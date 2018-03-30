using System;
using System.Collections.Generic;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<IDatabaseWorkspace>> GetDefaultWorkspace()
            => new GetDefaultWorkspaceInteractor(database);

        public IInteractor<IObservable<bool>> AreCustomColorsEnabledForWorkspace(long workspaceId)
            => new AreCustomColorsEnabledForWorkspaceInteractor(database, workspaceId);

        public IInteractor<IObservable<bool?>> AreProjectsBillableByDefault(long workspaceId)
            => new AreProjectsBillableByDefaultInteractor(database, workspaceId);

        public IInteractor<IObservable<IDatabaseWorkspace>> GetWorkspaceById(long workspaceId)
            => new GetWorkspaceByIdInteractor(database, workspaceId);

        public IInteractor<IObservable<IEnumerable<IDatabaseWorkspace>>> GetAllWorkspaces()
            => new GetAllWorkspacesInteractor(database);

        public IInteractor<IObservable<bool>> WorkspaceAllowsBillableRates(long workspaceId)
            => new WorkspaceAllowsBillableRatesInteractor(database, workspaceId);

        public IInteractor<IObservable<bool>> IsBillableAvailableForWorkspace(long workspaceId)
            => new IsBillableAvailableForWorkspaceInteractor(database, workspaceId);
    }
}
