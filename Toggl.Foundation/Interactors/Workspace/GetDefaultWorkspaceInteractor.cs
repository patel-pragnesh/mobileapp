﻿using System;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetDefaultWorkspaceInteractor : IInteractor<IObservable<IThreadSafeWorkspace>>
    {
        private readonly ITogglDataSource dataSource;

        public GetDefaultWorkspaceInteractor(ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.dataSource = dataSource;
        }

        public IObservable<IThreadSafeWorkspace> Execute()
            => dataSource.User
                .Get()
                .SelectMany(user => user.DefaultWorkspaceId.HasValue
                    ? dataSource.Workspaces.GetById(user.DefaultWorkspaceId.Value)
                    : chooseWorkspace())
                .Catch((InvalidOperationException exception) => chooseWorkspace())
                .Select(Workspace.From);

        private IObservable<IThreadSafeWorkspace> chooseWorkspace()
            => dataSource.Workspaces.GetAll(workspace => !workspace.IsDeleted)
                .Select(workspaces => workspaces.OrderBy(workspace => workspace.Id).First());
    }
}
