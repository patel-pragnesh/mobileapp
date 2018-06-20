using System;
using System.Reactive.Concurrency;
using Android.Content;
using MvvmCross;
using MvvmCross.Binding;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android.Presenters;
using MvvmCross.Plugin;
using MvvmCross.ViewModels;
using Toggl.Foundation;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Suggestions;
using Toggl.Giskard.Presenters;
using Toggl.Giskard.Services;
using Toggl.PrimeRadiant.Realm;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;

namespace Toggl.Giskard
{
    public sealed partial class Setup : MvxAppCompatSetup
    {
        private const int maxNumberOfSuggestions = 5;

        private IAnalyticsService analyticsService;
        private IMvxNavigationService navigationService;

#if USE_PRODUCTION_API
        private const ApiEnvironment environment = ApiEnvironment.Production;
#else
        private const ApiEnvironment environment = ApiEnvironment.Staging;
#endif

        protected override IMvxApplication CreateApp() => new App<LoginViewModel>();

        protected override MvxBindingBuilder CreateBindingBuilder() => new TogglBindingBuilder();

        protected override IMvxNavigationService InitializeNavigationService(IMvxViewModelLocatorCollection collection)
        {
            analyticsService = new AnalyticsService();

            var loader = CreateViewModelLoader(collection);
            Mvx.RegisterSingleton(loader);

            navigationService = new TrackingNavigationService(null, loader, analyticsService);

            Mvx.RegisterSingleton(navigationService);
            return navigationService;
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter() 
            => new TogglPresenter(AndroidViewAssemblies);

        protected override void InitializeApp(IMvxPluginManager pluginManager, IMvxApplication app)
        {
            const string clientName = "Giskard";
            var packageInfo = ApplicationContext.PackageManager.GetPackageInfo(ApplicationContext.PackageName, 0);
            var version = packageInfo.VersionName;
            var sharedPreferences = ApplicationContext.GetSharedPreferences(clientName, FileCreationMode.Private);
            var database = new Database();
            var scheduler = Scheduler.Default;
            var timeService = new TimeService(scheduler);
            var suggestionProviderContainer = new SuggestionProviderContainer(
                new MostUsedTimeEntrySuggestionProvider(database, timeService, maxNumberOfSuggestions)
            );

            var keyValueStorage = new SharedPreferencesStorage(sharedPreferences);
            var settingsStorage = new SettingsStorage(Version.Parse(version), keyValueStorage);

            var foundation = 
                TogglFoundation
                    .ForClient(clientName, version)
                    .WithDatabase(database)
                    .WithScheduler(scheduler)
                    .WithTimeService(timeService)
                    .WithApiEnvironment(environment)
                    .WithGoogleService<GoogleService>()
                    .WithLicenseProvider<LicenseProvider>()
                    .WithAnalyticsService(analyticsService)
                    .WithPlatformConstants<PlatformConstants>()
                    .WithMailService(new MailService(ApplicationContext))
                    .WithSuggestionProviderContainer(suggestionProviderContainer)
                    .WithApplicationShortcutCreator(new ApplicationShortcutCreator(ApplicationContext))

                    .StartRegisteringPlatformServices()
                    .WithDialogService<DialogService>()
                    .WithBrowserService<BrowserService>()
                    .WithKeyValueStorage(keyValueStorage)
                    .WithOnboardingStorage(settingsStorage)
                    .WithAccessRestrictionStorage(settingsStorage)
                    .WithUserPreferences(settingsStorage)
                    .WithNavigationService(navigationService)
                    .WithPasswordManagerService<OnePasswordService>()
                    .Build();

            foundation.Initialize();
            base.InitializeApp(pluginManager, app);
        }
    }
}
