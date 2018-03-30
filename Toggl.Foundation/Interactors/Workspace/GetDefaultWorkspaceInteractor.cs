using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetDefaultWorkspaceInteractor : IInteractor<IObservable<IDatabaseWorkspace>>
    {
        private readonly ITogglDatabase database;

        public GetDefaultWorkspaceInteractor(ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            this.database = database;
        }

        public IObservable<IDatabaseWorkspace> Execute()
            => database.User
                .Single()
                .SelectMany(user =>
                    database.Workspaces.GetById(user.DefaultWorkspaceId));
    }
}
