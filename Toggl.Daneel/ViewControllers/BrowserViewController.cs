using CoreGraphics;
using Foundation;
using MvvmCross.Platforms.Ios.Views;
using MvvmCross.Platforms.Ios.Presenters.Attributes;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using WebKit;

namespace Toggl.Daneel.ViewControllers
{
    [MvxChildPresentation]
    public sealed class BrowserViewController : MvxViewController<BrowserViewModel>, IWKNavigationDelegate
    {
        private WKWebView webView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = Resources.Loading;

            setNavBar();
            buildUI();

            webView.LoadRequest(new NSUrlRequest(new NSUrl(ViewModel.Url)));
        }

        private void buildUI()
        {
            // This is done via code because of a bug in XCode that does not allow WKWebViews in IB files.
            // This needs to be turned into an .xib file after the bug is fixed.
            var backgroundView = new UIView(new CGRect(0, 0, View.Frame.Width, 70))
            {
                BackgroundColor = Color.NavigationBar.BackgroundColor.ToNativeColor()
            };


            webView = new WKWebView(View.Frame, new WKWebViewConfiguration());
            webView.NavigationDelegate = this;

            View.Add(webView);
            View.Add(backgroundView);
        }

        private void setNavBar()
        {
            var tintColor = Color.NavigationBar.BackButton.ToNativeColor();
            NavigationController.NavigationBar.TintColor = tintColor;
            NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
            NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
            {
                ForegroundColor = tintColor
            };
        }

        [Export("webView:didFinishNavigation:")]
        public void OnNavigationFinished(WKWebView webView, WKNavigation navigation)
        {
            Title = ViewModel.Title;
        }
    }
}

