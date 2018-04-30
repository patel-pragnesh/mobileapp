using MvvmCross.Core.ViewModels;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Foundation.Suggestions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public abstract class BaseViewModelTests<TViewModel> : BaseMvvmCrossTests
        where TViewModel : MvxViewModel
    {
        protected ITogglApi Api { get; } = Substitute.For<ITogglApi>();
        protected UserAgent UserAgent { get; } = new UserAgent("Foundation.Tests", "1.0");
        protected IMailService MailService { get; } = Substitute.For<IMailService>();
        protected ITogglDatabase Database { get; } = Substitute.For<ITogglDatabase>();
        protected IDialogService DialogService { get; } = Substitute.For<IDialogService>();
        protected ILicenseProvider LicenseProvider { get; } = Substitute.For<ILicenseProvider>();
        protected IPlatformConstants PlatformConstants { get; } = Substitute.For<IPlatformConstants>();
        protected IOnboardingStorage OnboardingStorage { get; } = Substitute.For<IOnboardingStorage>();
        protected ISuggestionProviderContainer SuggestionProviderContainer { get; } = Substitute.For<ISuggestionProviderContainer>();

        protected TViewModel ViewModel { get; private set; }

        protected BaseViewModelTests()
        {
            Setup();
        }

        protected abstract TViewModel CreateViewModel();

        private void Setup()
        {
            AdditionalSetup();

            ViewModel = CreateViewModel();
        }

        protected virtual void AdditionalSetup()
        {
        }
    }
}