using UIKit;

namespace Toggl.Daneel.Extensions
{
    public static class LayoutConstraintExtensions
    {
        public static void AdaptForIos10(this NSLayoutConstraint self, UINavigationBar navigationBar)
        {
            if (self == null) return;

            //on iOS 11 and later this happens automatically
            if (!UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                var statusbarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;
                var navigationBarHeight = navigationBar == null ? 0 : navigationBar.Frame.Height;
                self.Constant += statusbarHeight + navigationBarHeight;
            }
        }
    }
}
