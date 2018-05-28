using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class OnboardingActivity : MvxAppCompatActivity<OnboardingViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            var statusBarColor = new Color(ContextCompat.GetColor(this, Resource.Color.onboardingStatusBarColor));
            this.ChangeStatusBarColor(statusBarColor);

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.OnboardingActivity);
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
        }
    }
}
