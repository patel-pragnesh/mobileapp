using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    public class NoWorkspaceViewModel : MvxViewModel
    {
        private IMvxNavigationService navigationService;
        private ITogglDataSource dataSource;

        public bool IsLoading { get; private set; }

        public IMvxAsyncCommand TryAgainCommand { get; }

        public IMvxAsyncCommand CreateWorkspaceCommand { get; }

        public NoWorkspaceViewModel(
            IMvxNavigationService navigationService,
            ITogglDataSource dataSource
        )
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.navigationService = navigationService;
            this.dataSource = dataSource;

            TryAgainCommand = new MvxAsyncCommand(tryAgain);
            CreateWorkspaceCommand = new MvxAsyncCommand(createWorkspace);
        }

        private async Task tryAgain()
        {
            IsLoading = true;

            var workspaces = await dataSource
                .SyncManager
                .ForceFullSync()
                .SelectMany(dataSource.Workspaces.GetAll());

            IsLoading = false;

            if (workspaces.Any())
            {
                close();
            }
        }

        private async Task createWorkspace()
        {
            IsLoading = true;

            var workspaces = await dataSource.Workspaces.GetAll();
            if (!workspaces.Any())
            {
                var user = await dataSource.User.Current;
                await dataSource.Workspaces.Create($"{user.Fullname}'s Workspace");
            }

            IsLoading = false;
            close();
        }

        private void close()
        {
            navigationService.Close(this);
        }
    }
}
