using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    public class NoWorksaceViewModel : MvxViewModel
    {
        private IMvxNavigationService navigationService;
        private ITogglDataSource dataSource;

        private CompositeDisposable disposeBag = new CompositeDisposable();
        private Subject<Unit> syncSubject = new Subject<Unit>();
        private Subject<Unit> getWorkspacesSubject = new Subject<Unit>();

        public bool IsLoading { get; private set; }

        public IMvxCommand TryAgainCommand { get; }

        public IMvxAsyncCommand CreateWorkspaceCommand { get; }

        public NoWorksaceViewModel(
            IMvxNavigationService navigationService, 
            ITogglDataSource dataSource
        )
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.navigationService = navigationService;
            this.dataSource = dataSource;

            disposeBag.Add(
                getWorkspacesSubject
                    .AsObservable()
                    .Do(_ => IsLoading = true)
                    .SelectMany(_ => dataSource.Workspaces.GetAll())
                    .Subscribe(onWorkspaces)
            );

            disposeBag.Add(
                syncSubject
                    .AsObservable()
                    .SelectMany(dataSource.SyncManager.ForceFullSync())
                    .SelectUnit()
                    .Subscribe(getWorkspacesSubject.OnNext)
            );

            TryAgainCommand = new MvxCommand(tryAgain);
            CreateWorkspaceCommand = new MvxAsyncCommand(createWorkspace);
        }

        private void tryAgain()
            => syncSubject.OnNext(Unit.Default);

        private async Task createWorkspace()
        {
            IsLoading = true;
            var workspaces = await dataSource.Workspaces.GetAll();
            if (workspaces.Any())
            {
                IsLoading = false;
                close();
                return;
            }

            var user = await dataSource.User.Current;
            var workspace = await dataSource.Workspaces.Create($"{user.Fullname}'s Workspace");
            syncSubject.OnNext(Unit.Default);
        }

        private void onWorkspaces(IEnumerable<IThreadSafeWorkspace> workspaces)
        {
            IsLoading = false;
            if (workspaces.Any())
            {
                close();
            }
        }

        private void close()
        {
            navigationService.Close(this);
        }
    }
}
