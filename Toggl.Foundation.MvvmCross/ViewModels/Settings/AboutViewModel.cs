using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Multivac;
using Toggl.Foundation.MvvmCross.Parameters;

namespace Toggl.Foundation.MvvmCross.ViewModels.Settings
{
    [Preserve(AllMembers = true)]
    public sealed class AboutViewModel : MvxViewModel
    {
        public const string PrivacyPolicyUrl = "https://toggl.com/legal/privacy";
        public const string TermsOfServiceUrl = "https://toggl.com/legal/terms";

        private readonly IMvxNavigationService navigationService;
      
        public IMvxAsyncCommand PrivacyPolicyCommand { get; }
        public IMvxAsyncCommand TermsOfServiceCommand { get; }
        public IMvxAsyncCommand LicensesCommand { get; }

        public AboutViewModel(IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.navigationService = navigationService;

            PrivacyPolicyCommand = new MvxAsyncCommand(openPrivacyPolicyView);
            TermsOfServiceCommand = new MvxAsyncCommand(openTermsOfServiceView);
            LicensesCommand = new MvxAsyncCommand(openLicensesView);
        }


        private Task openPrivacyPolicyView() => 
            navigationService.Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(PrivacyPolicyUrl, Resources.PrivacyPolicy)
            );

        private Task openTermsOfServiceView() => 
            navigationService.Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(TermsOfServiceUrl, Resources.TermsOfService)
            );

        private Task openLicensesView()
            => navigationService.Navigate<LicensesViewModel>();
    }
}
