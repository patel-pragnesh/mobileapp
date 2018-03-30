using System;
using System.Collections.Generic;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetAllWorkspacesInteractor : IInteractor<IObservable<IEnumerable<IDatabaseWorkspace>>>
    {
        private readonly ITogglDatabase database;

        public GetAllWorkspacesInteractor(ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.database = database;
        }

        public IObservable<IEnumerable<IDatabaseWorkspace>> Execute()
            => database.Workspaces.GetAll();
    }
}
