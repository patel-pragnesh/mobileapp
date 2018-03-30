using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class OnboardingViewModelTests
    {
        public abstract class OnboardingViewModelTest : BaseViewModelTests<OnboardingViewModel>
        {
            protected override OnboardingViewModel CreateViewModel()
                => new OnboardingViewModel(NavigationService, OnboardingStorage, AnalyticsService);
        }

        public sealed class TheConstructor : OnboardingViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(ThreeParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useNavigationService, bool useOnboardingStorage, bool useAnalyticsService)
            {
                var analyticsService = useAnalyticsService ? AnalyticsService : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new OnboardingViewModel(navigationService, onboardingStorage, analyticsService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheCurrentPageProperty : OnboardingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void DoesNotAllowUsersToSetItsValueToAValueGreaterThanTheNumberOfPagesMinusOne()
            {
                ViewModel.CurrentPage = OnboardingViewModel.TrackPage;

                ViewModel.CurrentPage = ViewModel.NumberOfPages;
                ViewModel.CurrentPage = ViewModel.NumberOfPages + 1;

                ViewModel.CurrentPage.Should().Be(OnboardingViewModel.TrackPage);
            }

            [Fact, LogIfTooSlow]
            public void DoesNotAllowUsersToSetItsValueToANegativeNumber()
            {
                ViewModel.CurrentPage = OnboardingViewModel.TrackPage;

                ViewModel.CurrentPage = -1;

                ViewModel.CurrentPage.Should().Be(OnboardingViewModel.TrackPage);
            }
        }

        public sealed class TheIsFirstPageProperty : OnboardingViewModelTest
        {
            [Theory, LogIfTooSlow]
            [InlineData(OnboardingViewModel.TrackPage)]
            [InlineData(OnboardingViewModel.MostUsedPage)]
            [InlineData(OnboardingViewModel.ReportsPage)]
            [InlineData(OnboardingViewModel.LoginPage)]
            public void OnlyReturnsTrueForTheFirstPage(int page)
            {
                ViewModel.CurrentPage = page;

                var expected = page == 0;
                ViewModel.IsFirstPage.Should().Be(expected);
            }
        }

        public sealed class TheIsLastPageProperty : OnboardingViewModelTest
        {
            [Theory, LogIfTooSlow]
            [InlineData(OnboardingViewModel.TrackPage)]
            [InlineData(OnboardingViewModel.MostUsedPage)]
            [InlineData(OnboardingViewModel.ReportsPage)]
            [InlineData(OnboardingViewModel.LoginPage)]
            public void OnlyReturnsTrueInThePageWhoseIndexEqualsToTheNumberOfPagesMinusOne(int page)
            {
                ViewModel.CurrentPage = page;

                var expected = page == ViewModel.NumberOfPages - 1;
                ViewModel.IsLastPage.Should().Be(expected);
            }
        }

        public sealed class TheSkipCommand : OnboardingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void GoesStraightToTheLastPage()
            {
                ViewModel.SkipCommand.Execute();

                ViewModel.CurrentPage.Should().Be(OnboardingViewModel.LoginPage);
            }

            [Theory, LogIfTooSlow]
            [InlineData(OnboardingViewModel.TrackPage, nameof(OnboardingViewModel.TrackPage))]
            [InlineData(OnboardingViewModel.MostUsedPage, nameof(OnboardingViewModel.MostUsedPage))]
            [InlineData(OnboardingViewModel.ReportsPage, nameof(OnboardingViewModel.ReportsPage))]
            public void CallsTheAnalyticsServiceIndicatingTheCurrentPage(int page, string expectedPageName)
            {
                ViewModel.CurrentPage = page;

                ViewModel.SkipCommand.Execute();

                AnalyticsService.Received().TrackOnboardingSkipEvent(expectedPageName);
            }
        }

        public sealed class TheNextCommand : OnboardingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void AdvancesToTheNextPage()
            {
                ViewModel.CurrentPage = OnboardingViewModel.TrackPage;

                ViewModel.NextCommand.Execute();

                ViewModel.CurrentPage.Should().Be(OnboardingViewModel.MostUsedPage);
            }

            [Fact, LogIfTooSlow]
            public void CannotBeExecutedWhenOnTheLastPage()
            {
                ViewModel.CurrentPage = OnboardingViewModel.LoginPage;

                ViewModel.NextCommand.CanExecute().Should().BeFalse();
            }
        }

        public sealed class ThePreviousCommand : OnboardingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void ReturnsToThePreviousPage()
            {
                ViewModel.CurrentPage = OnboardingViewModel.MostUsedPage;

                ViewModel.PreviousCommand.Execute();

                ViewModel.CurrentPage.Should().Be(OnboardingViewModel.TrackPage);
            }

            [Fact, LogIfTooSlow]
            public void CannotBeExecutedWhenOnTheFirstPage()
            {
                ViewModel.CurrentPage = OnboardingViewModel.TrackPage;

                ViewModel.PreviousCommand.CanExecute().Should().BeFalse();
            }
        }

        public sealed class TheLoginCommand : OnboardingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task RequestsTheLoginViewModelFromTheNavigationService()
            {
                await ViewModel.LoginCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Navigate<LoginViewModel, LoginType>(Arg.Is(LoginType.Login));
            }

            [Fact, LogIfTooSlow]
            public void SetsTheCompletedOnboardingFlagWhenUserViewsAllOnboardingPages()
            {
                ViewModel.Prepare();
                while (ViewModel.NextCommand.CanExecute())
                    ViewModel.NextCommand.Execute();

                ViewModel.LoginCommand.Execute();

                OnboardingStorage.Received().SetCompletedOnboarding();
            }

            [Theory, LogIfTooSlow]
            [InlineData(1)]
            [InlineData(2)]
            public void DoesNotSetTheCompletedOnboardingFlagWhenUserSkipsAtLeastOnePage(int pagesViewed)
            {
                ViewModel.Prepare();
                for (int i = 1; i < pagesViewed; i++)
                    ViewModel.NextCommand.Execute();
                ViewModel.SkipCommand.Execute();

                ViewModel.LoginCommand.Execute();

                OnboardingStorage.DidNotReceive().SetCompletedOnboarding();
            }
        }

        public sealed class TheSignUpCommand : OnboardingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task RequestsTheLoginViewModelFromTheNavigationService()
            {
                await ViewModel.SignUpCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Navigate<LoginViewModel, LoginType>(Arg.Is(LoginType.SignUp));
            }

            [Fact, LogIfTooSlow]
            public void SetsTheCompletedOnboardingFlagWhenUserViewsAllOnboardingPages()
            {
                ViewModel.Prepare();
                while (ViewModel.NextCommand.CanExecute())
                    ViewModel.NextCommand.Execute();

                ViewModel.SignUpCommand.Execute();

                OnboardingStorage.Received().SetCompletedOnboarding();
            }

            [Theory, LogIfTooSlow]
            [InlineData(1)]
            [InlineData(2)]
            public void DoesNotSetTheCompletedOnboardingFlagWhenUserSkipsAtLeastOnePage(int pagesViewed)
            {
                ViewModel.Prepare();
                for (int i = 1; i < pagesViewed; i++)
                    ViewModel.NextCommand.Execute();
                ViewModel.SkipCommand.Execute();

                ViewModel.SignUpCommand.Execute();

                OnboardingStorage.DidNotReceive().SetCompletedOnboarding();
            }
        }

        public sealed class ThePrepareMethod : OnboardingViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void InitializesCurrentPageToTrackPageIfUserHasNotCompletedOnboarding()
            {
                ViewModel.Prepare();

                ViewModel.CurrentPage.Should().Be(OnboardingViewModel.TrackPage);
            }

            [Fact, LogIfTooSlow]
            public void InitializesCurrentPageToTrackPageIfUserHasCompletedOnboarding()
            {
                OnboardingStorage.CompletedOnboarding().Returns(true);

                ViewModel.Prepare();

                ViewModel.CurrentPage.Should().Be(OnboardingViewModel.LoginPage);
            }
        }
    }
}
