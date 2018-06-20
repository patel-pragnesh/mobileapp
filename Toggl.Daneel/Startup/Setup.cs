using System;
using System.Reactive.Concurrency;
using Foundation;
using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Ios.Core;
using MvvmCross.Platforms.Ios.Presenters;
using MvvmCross.Plugin;
using MvvmCross.ViewModels;
using Toggl.Daneel.Presentation;
using Toggl.Daneel.Services;
using Toggl.Foundation;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Foundation.Suggestions;
using Toggl.PrimeRadiant.Realm;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Daneel
{
    public partial class Setup : MvxIosSetup
    {
        private const int maxNumberOfSuggestions = 3;

        private IAnalyticsService analyticsService;
        private IMvxNavigationService navigationService;

#if USE_PRODUCTION_API
        private const ApiEnvironment environment = ApiEnvironment.Production;
#else
        private const ApiEnvironment environment = ApiEnvironment.Staging;
#endif

        protected override IMvxIosViewPresenter CreateViewPresenter() => new TogglPresenter(ApplicationDelegate, Window);

        protected override IMvxApplication CreateApp() => new App<OnboardingViewModel>();

        protected override IMvxNavigationService InitializeNavigationService(IMvxViewModelLocatorCollection collection)
        {
            analyticsService = new AnalyticsService();

            var loader = CreateViewModelLoader(collection);
            Mvx.RegisterSingleton(loader);

            navigationService = new TrackingNavigationService(null, loader, analyticsService);

            Mvx.RegisterSingleton(navigationService);
            return navigationService;
        }

        protected override void RegisterViewTypeFinder()
        {
            var typeFinder = new MvxViewModelViewTypeFinder(CreateViewModelByNameLookup(), CreateViewToViewModelNaming());
            Mvx.RegisterSingleton<IMvxViewModelTypeFinder>(typeFinder);
        }

        protected override void InitializeApp(IMvxPluginManager pluginManager, IMvxApplication app)
        {
#if !USE_PRODUCTION_API
            System.Net.ServicePointManager.ServerCertificateValidationCallback
                  += (sender, certificate, chain, sslPolicyErrors) => true;
#endif

            const string clientName = "Daneel";
            var version = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            var database = new Database();
            var scheduler = Scheduler.Default;
            var timeService = new TimeService(scheduler);
            var suggestionProviderContainer = new SuggestionProviderContainer(
                new MostUsedTimeEntrySuggestionProvider(database, timeService, maxNumberOfSuggestions)
            );

            var appVersion = Version.Parse(version);
            var userAgent = new UserAgent(clientName, version);
            var keyValueStorage = new UserDefaultsStorage();
            var settingsStorage = new SettingsStorage(Version.Parse(version), keyValueStorage);

            var foundation =
                TogglFoundation
                   .ForClient(userAgent, appVersion)
                   .WithDatabase(database)
                   .WithScheduler(scheduler)
                   .WithTimeService(timeService)
                   .WithApiEnvironment(environment)
                   .WithGoogleService<GoogleService>()
                   .WithLicenseProvider<LicenseProvider>()
                   .WithAnalyticsService(analyticsService)
                   .WithPlatformConstants<PlatformConstants>()
                   .WithApiFactory(new ApiFactory(environment, userAgent))
                   .WithBackgroundService(new BackgroundService(timeService))
                   .WithApplicationShortcutCreator<ApplicationShortcutCreator>()
                   .WithSuggestionProviderContainer(suggestionProviderContainer)
                   .WithMailService(new MailService((ITopViewControllerProvider)Presenter))

                   .StartRegisteringPlatformServices()
                   .WithBrowserService<BrowserService>()
                   .WithKeyValueStorage(keyValueStorage)
                   .WithUserPreferences(settingsStorage)
                   .WithOnboardingStorage(settingsStorage)
                   .WithNavigationService(navigationService)
                   .WithAccessRestrictionStorage(settingsStorage)
                   .WithPasswordManagerService<OnePasswordService>()
                   .WithDialogService(new DialogService((ITopViewControllerProvider)Presenter))
                   .WithErrorHandlingService(new ErrorHandlingService(navigationService, settingsStorage))
                   .Build();

            foundation.RevokeNewUserIfNeeded().Initialize();

            foundation.RevokeNewUserIfNeeded().Initialize();

            base.InitializeApp(pluginManager, app);
        }
    }
}
