using System;
using System.Reactive.Concurrency;
using FluentAssertions;
using MvvmCross.Navigation;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross
{
    public class FoundationExtensionsTests
    {
        public class TheRegisterServicesMethod : BaseMvvmCrossTests
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(EightParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheParametersIsNull(
                bool useFoundation,
                bool useDialogService,
                bool useBrowserService,
                bool useKeyValueStorage,
                bool useNavigationService,
                bool useAccessRestrictionStorage,
                bool useUserPreferences,
                bool useOnboardingStorage)
            {
                var foundation = useFoundation ? new Foundation() : null;
                var dialogService = useDialogService ? Substitute.For<IDialogService>() : null;
                var browserService = useBrowserService ? Substitute.For<IBrowserService>() : null;
                var keyValueStorage = useKeyValueStorage ? Substitute.For<IKeyValueStorage>() : null;
                var navigationService = useNavigationService ? Substitute.For<IMvxNavigationService>() : null;
                var accessRestrictionStorage = useAccessRestrictionStorage ? Substitute.For<IAccessRestrictionStorage>() : null;
                var userPreferences = useUserPreferences ? Substitute.For<IUserPreferences>() : null;
                var onboardingStorage = useOnboardingStorage ? Substitute.For<IOnboardingStorage>() : null;

                Action tryingToConstructWithEmptyParameters =
                    () => foundation.RegisterServices(
                            dialogService,
                            browserService,
                            keyValueStorage,
                            accessRestrictionStorage,
                            userPreferences,
                            onboardingStorage,
                            navigationService
                        );

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public class TheRevokeNewUserIfNeededMethod : BaseMvvmCrossTests
        {
            [Fact, LogIfTooSlow]
            public void MarksTheUserAsNotNewWhenUsingTheAppForTheFirstTimeAfterSixtyDays()
            {
                var version = "1.0";
                var now = DateTimeOffset.Now;
                var scheduler = Substitute.For<IScheduler>();
                var apiFactory = Substitute.For<IApiFactory>();
                var database = Substitute.For<ITogglDatabase>();
                var timeService = Substitute.For<ITimeService>();
                var analyticsService = Substitute.For<IAnalyticsService>();
                var googleService = Substitute.For<IGoogleService>();
                var backgroundService = Substitute.For<IBackgroundService>();
                var keyValueStorage = Substitute.For<IKeyValueStorage>();
                var apiErrorHandlingService = Substitute.For<IApiErrorHandlingService>();
                var applicationShortcutCreator = Substitute.For<IApplicationShortcutCreator>();
                var onboardingStorage = Substitute.For<IOnboardingStorage>();
                var accessRestrictionStorage = Substitute.For<IAccessRestrictionStorage>();
                var foundationMvvmCross = new FoundationMvvmCross(
                    apiFactory,
                    database,
                    timeService,
                    scheduler,
                    analyticsService,
                    googleService,
                    applicationShortcutCreator,
                    backgroundService,
                    onboardingStorage,
                    accessRestrictionStorage,
                    NavigationService,
                    apiErrorHandlingService);
                timeService.CurrentDateTime.Returns(now);
                onboardingStorage.GetLastOpened().Returns(now.AddDays(-60).ToString());

                foundationMvvmCross.RevokeNewUserIfNeeded();

                onboardingStorage.Received().SetLastOpened(now);
                onboardingStorage.Received().SetIsNewUser(false);
            }
        }
    }
}
