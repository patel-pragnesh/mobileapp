using System;
using System.Reactive.Concurrency;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Login;
using Toggl.Foundation.Services;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Suggestions;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation
{
    public sealed class Foundation
    {
        public Version Version { get; internal set; }
        public UserAgent UserAgent { get; internal set; }
        public IApiFactory ApiFactory { get; internal set; }
        public ITogglDatabase Database { get; internal set; }
        public ITimeService TimeService { get; internal set; }
        public IScheduler Scheduler { get; internal set; }
        public IMailService MailService { get; internal set; }
        public IGoogleService GoogleService { get; internal set; }
        public ApiEnvironment ApiEnvironment { get; internal set; }
        public IAnalyticsService AnalyticsService { get; internal set; }
        public IApplicationShortcutCreator ShortcutCreator { get; internal set; }
        public IBackgroundService BackgroundService { get; internal set; }
        public IPlatformConstants PlatformConstants { get; internal set; }
        public ISuggestionProviderContainer SuggestionProviderContainer { get; internal set; }

        public static Foundation Create(
            string clientName,
            string version,
            ITogglDatabase database,
            ITimeService timeService,
            IScheduler scheduler,
            IMailService mailService,
            IGoogleService googleService,
            ApiEnvironment apiEnvironment,
            IAnalyticsService analyticsService,
            IPlatformConstants platformConstants,
            IApplicationShortcutCreator shortcutCreator,
            ISuggestionProviderContainer suggestionProviderContainer)
        {
            Ensure.Argument.IsNotNull(version, nameof(version));
            Ensure.Argument.IsNotNull(database, nameof(database));
            Ensure.Argument.IsNotNull(clientName, nameof(clientName));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(scheduler, nameof(scheduler));
            Ensure.Argument.IsNotNull(mailService, nameof(mailService));
            Ensure.Argument.IsNotNull(googleService, nameof(googleService));
            Ensure.Argument.IsNotNull(shortcutCreator, nameof(shortcutCreator));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(platformConstants, nameof(platformConstants));
            Ensure.Argument.IsNotNull(suggestionProviderContainer, nameof(suggestionProviderContainer));

            var userAgent = new UserAgent(clientName, version.ToString());

            var foundation = new Foundation
            {
                Database = database,
                UserAgent = userAgent,
                TimeService = timeService,
                Scheduler = scheduler,
                MailService = mailService,
                GoogleService = googleService,
                ApiEnvironment = apiEnvironment,
                Version = Version.Parse(version),
                ShortcutCreator = shortcutCreator,
                AnalyticsService = analyticsService,
                PlatformConstants = platformConstants,
                BackgroundService = new BackgroundService(timeService),
                ApiFactory = new ApiFactory(apiEnvironment, userAgent),
                SuggestionProviderContainer = suggestionProviderContainer
            };

            return foundation;
        }

        internal Foundation() { }
    }
}
