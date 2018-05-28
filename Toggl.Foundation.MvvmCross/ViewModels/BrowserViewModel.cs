using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class BrowserViewModel : MvxViewModel<BrowserParameters>
    {
        private readonly IMvxNavigationService navigationService;

        public string Url { get; private set; }

        public string Title { get; private set; }

        public IMvxAsyncCommand BackCommand { get; private set; }

        public BrowserViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;

            BackCommand = new MvxAsyncCommand(back);
        }

        public override void Prepare(BrowserParameters parameter)
        {
            Url = parameter.Url;
            Title = parameter.Title;
        }

        private Task back() => navigationService.Close(this);
    }
}
