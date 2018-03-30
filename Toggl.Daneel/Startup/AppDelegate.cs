using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Platform;
using MvvmCross.Platform;
using Foundation;
using Toggl.Foundation.Services;
using UIKit;
using MvvmCross.Core.Navigation;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Analytics;

namespace Toggl.Daneel
{
    [Register(nameof(AppDelegate))]
    public sealed class AppDelegate : MvxApplicationDelegate
    {
        private IAnalyticsService analyticsService;
        private IBackgroundService backgroundService;
        private IMvxNavigationService navigationService;

        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            var setup = new Setup(this, Window);
            setup.Initialize();

            analyticsService = Mvx.Resolve<IAnalyticsService>();
            backgroundService = Mvx.Resolve<IBackgroundService>();
            navigationService = Mvx.Resolve<IMvxNavigationService>();

            var startup = Mvx.Resolve<IMvxAppStart>();
            startup.Start();

            Window.MakeKeyAndVisible();

            #if ENABLE_TEST_CLOUD
            Xamarin.Calabash.Start();
            #endif
            #if USE_ANALYTICS
            Microsoft.AppCenter.AppCenter.Start(
                "{TOGGL_APP_CENTER_ID_IOS}", 
                typeof(Microsoft.AppCenter.Crashes.Crashes),
                typeof(Microsoft.AppCenter.Analytics.Analytics));
            Firebase.Core.App.Configure();
            Google.SignIn.SignIn.SharedInstance.ClientID =
                Firebase.Core.App.DefaultInstance.Options.ClientId;
            #endif

            return true;
        }

        #if USE_ANALYTICS
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var openUrlOptions = new UIApplicationOpenUrlOptions(options);
            return Google.SignIn.SignIn.SharedInstance.HandleUrl(url, openUrlOptions.SourceApplication, openUrlOptions.Annotation);
        }
        #endif

        public override void WillEnterForeground(UIApplication application)
        {
            base.WillEnterForeground(application);
            backgroundService.EnterForeground();
        }

        public override void DidEnterBackground(UIApplication application)
        {
            base.DidEnterBackground(application);
            backgroundService.EnterBackground();
        }

        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
        {
            analyticsService.TrackAppShortcut(shortcutItem.LocalizedTitle);
            var url = shortcutItem.UserInfo[nameof(ApplicationShortcut.Url)].ToString();
            navigationService.Navigate(url);
        }
    }
}
