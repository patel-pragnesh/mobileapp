using System;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Login;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Services;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Xunit;
using Toggl.Foundation.Suggestions;
using Toggl.Foundation.Analytics;
using System.Reactive.Concurrency;

namespace Toggl.Foundation.Tests
{
    public sealed class FoundationTests
    {
        public class TheCreateMethod
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(ElevenParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useClientName,
                bool useVersion,
                bool useDatabase,
                bool useScheduler,
                bool useTimeService,
                bool useMailService,
                bool useGoogleService,
                bool useAnalyticsService,
                bool usePlatformConstants,
                bool useApplicationShortcutCreator,
                bool useSuggestionProviderContainer)
            {
                var version = useVersion ? "1.0" : null;
                var clientName = useClientName ? "Some Client" : null;
                var scheduler = useScheduler ? Substitute.For<IScheduler>() : null;
                var database = useDatabase ? Substitute.For<ITogglDatabase>() : null;
                var timeService = useTimeService ? Substitute.For<ITimeService>() : null;
                var mailService = useMailService ? Substitute.For<IMailService>() : null;
                var googleService = useGoogleService ? Substitute.For<IGoogleService>() : null;
                var analyticsService = useAnalyticsService ? Substitute.For<IAnalyticsService>() : null;
                var platformConstants = usePlatformConstants ? Substitute.For<IPlatformConstants>() : null;
                var applicationShortcutCreator = useApplicationShortcutCreator ? Substitute.For<IApplicationShortcutCreator>() : null;
                var suggestionProviderContainer = useSuggestionProviderContainer ? Substitute.For<ISuggestionProviderContainer>() : null;

                Action tryingToConstructWithEmptyParameters =
                    () => Foundation.Create(
                        clientName,
                        version,
                        database,
                        timeService,
                        scheduler,
                        mailService,
                        googleService,
                        ApiEnvironment.Staging,
                        analyticsService,
                        platformConstants,
                        applicationShortcutCreator,
                        suggestionProviderContainer
                    );

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }
    }
}
