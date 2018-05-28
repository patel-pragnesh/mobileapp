using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectWorkspaceViewModel : MvxViewModel<WorkspaceParameters, long>
    {
        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;

        private long defaultWorkspaceId;
        private IEnumerable<SelectableWorkspaceViewModel> allWorkspaces;

        public string Title { get; private set; } = "";

        public bool AllowQuerying { get; private set; }

        public string Text { get; set; } = "";

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand<SelectableWorkspaceViewModel> SelectWorkspaceCommand { get; }

        public MvxObservableCollection<SelectableWorkspaceViewModel> Suggestions { get; }
            = new MvxObservableCollection<SelectableWorkspaceViewModel>();

        public SelectWorkspaceViewModel(IInteractorFactory interactorFactory, IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;

            CloseCommand = new MvxAsyncCommand(close);
            SelectWorkspaceCommand = new MvxAsyncCommand<SelectableWorkspaceViewModel>(selectWorkspace);
        }

        public override void Prepare(WorkspaceParameters parameter)
        {
            Title = parameter.Title;
            AllowQuerying = parameter.AllowQuerying;
            defaultWorkspaceId = parameter.CurrentWorkspaceId;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            var workspaces = await interactorFactory.GetAllWorkspaces().Execute();

            allWorkspaces = workspaces.Select(w => new SelectableWorkspaceViewModel(w, w.Id == defaultWorkspaceId));
            Suggestions.AddRange(allWorkspaces);
        }

        private void OnTextChanged()
        {
            Suggestions.Clear();
            Suggestions.AddRange(
                allWorkspaces.Where(w => w.WorkspaceName.ContainsIgnoringCase(Text))
            );
        }

        private Task close()
            => navigationService.Close(this, defaultWorkspaceId);

        private Task selectWorkspace(SelectableWorkspaceViewModel workspace)
            => navigationService.Close(this, workspace.WorkspaceId);
    }
}
