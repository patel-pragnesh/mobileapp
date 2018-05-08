using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.UI;
using PropertyChanged;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class OnboardingViewModel : MvxViewModel
    {
        public const int TrackPage = 0;
        public const int MostUsedPage = 1;
        public const int ReportsPage = 2;
        public const int LoginPage = 3;

        private static readonly string[] pageNames =
        {
            nameof(TrackPage),
            nameof(MostUsedPage),
            nameof(ReportsPage),
            nameof(LoginPage)
        };

        private static readonly (MvxColor BackgroundColor, MvxColor BorderColor)[] PageInfo =
        {
            (Color.Onboarding.TrackPageBackgroundColor, Color.Onboarding.TrackPageBorderColor),
            (Color.Onboarding.LogPageBackgroundColor, Color.Onboarding.LogPageBorderColor),
            (Color.Onboarding.ReportsPageBackgroundColor, Color.Onboarding.ReportsPageBorderColor),
            (Color.Onboarding.LoginPageBackgroundColor, MvxColors.Transparent)
        };

        private readonly IAnalyticsService analyticsService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMvxNavigationService navigationService;

        private readonly bool[] pagesVisited = new bool[PageInfo.Length];

        private int currentPage;
        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (currentPage == value) return;
                if (value < 0 || value >= NumberOfPages) return;

                currentPage = value;
                RaisePropertyChanged();
                NextCommand.RaiseCanExecuteChanged();
                PreviousCommand.RaiseCanExecuteChanged();
            }
        }

        [DependsOn(nameof(CurrentPage))]
        public bool IsTrackPage => CurrentPage == TrackPage;

        [DependsOn(nameof(CurrentPage))]
        public bool IsMostUsedPage => CurrentPage == MostUsedPage;

        [DependsOn(nameof(CurrentPage))]
        public bool IsSummaryPage => CurrentPage == ReportsPage;

        [DependsOn(nameof(CurrentPage))]
        public bool IsFirstPage => CurrentPage == 0;

        [DependsOn(nameof(CurrentPage))]
        public bool IsLastPage => CurrentPage == NumberOfPages - 1;

        [DependsOn(nameof(CurrentPage))]
        public MvxColor BorderColor => PageInfo[CurrentPage].BorderColor;

        [DependsOn(nameof(CurrentPage))]
        public MvxColor BackgroundColor => PageInfo[CurrentPage].BackgroundColor;

        public IMvxCommand SkipCommand { get; }

        public IMvxCommand NextCommand { get; }

        public IMvxCommand PreviousCommand { get; }

        public IMvxAsyncCommand LoginCommand { get; }

        public IMvxAsyncCommand SignUpCommand { get; }

        public int NumberOfPages => PageInfo.Length;

        public OnboardingViewModel(
            IMvxNavigationService navigationService,
            IOnboardingStorage onboardingStorage,
            IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));

            this.analyticsService = analyticsService;
            this.navigationService = navigationService;
            this.onboardingStorage = onboardingStorage;

            SkipCommand = new MvxCommand(skip);
            NextCommand = new MvxCommand(next, nextCanExecute);
            LoginCommand = new MvxAsyncCommand(login);
            SignUpCommand = new MvxAsyncCommand(signup);
            PreviousCommand = new MvxCommand(previous, previousCanExecute);
        }

        public override void Prepare()
        {
            base.Prepare();

            CurrentPage = onboardingStorage.CompletedOnboarding()
                ? LoginPage
                : TrackPage;

            OnCurrentPageChanged();
        }

        private async Task login()
        {
            setOnboardingCompletedIfNeeded();

            //TEMP: this is ugly and needs to be removed asap
            try
            {
                await navigationService.Navigate<LoginViewModel, LoginType>(LoginType.Login);
            }
            catch (KeyNotFoundException)
            {
                await navigationService.Navigate<NewLoginViewModel>();                
            }
        }

        private Task signup()
        {
            setOnboardingCompletedIfNeeded();
            return navigationService.Navigate<LoginViewModel, LoginType>(LoginType.SignUp);
        }

        private void skip()
        {
            analyticsService.TrackOnboardingSkipEvent(pageNames[CurrentPage]);
            CurrentPage = PageInfo.Length - 1;
        }

        private bool nextCanExecute() => !IsLastPage;

        private bool previousCanExecute() => !IsFirstPage;

        private void next() => CurrentPage++;

        private void previous() => CurrentPage--;

        private void OnCurrentPageChanged() 
            => pagesVisited[CurrentPage] = true;

        private void setOnboardingCompletedIfNeeded()
        {
            if (pagesVisited.All(visited => visited))
                onboardingStorage.SetCompletedOnboarding();
        }
    }
}
