using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Views;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class ReportsActivity : MvxAppCompatActivity<ReportsViewModel>
    {
        private ReportsRecyclerView reportsRecyclerView;
        private ReportsLinearLayout reportsMainContainer;

        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(Color.ParseColor("#2C2C2C"));

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ReportsActivity);

            reportsRecyclerView = FindViewById<ReportsRecyclerView>(Resource.Id.ReportsActivityRecyclerView);
            reportsMainContainer = FindViewById<ReportsLinearLayout>(Resource.Id.ReportsActivityMainContainer);
            reportsMainContainer.CalendarContainer = FindViewById(Resource.Id.ReportsCalendarContainer);
        }

        internal void ToggleCalendarState(bool forceHide)
        {
            reportsMainContainer.ToggleCalendar(forceHide);
        }
    }
}
