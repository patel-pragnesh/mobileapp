using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Views;
using static Android.Support.V7.Widget.Toolbar;

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

            setupToolbar();
        }

        private void setupToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.Toolbar);
            toolbar.Title = "";
            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            toolbar.NavigationClick += onNavigateBack;
        }

        private void onNavigateBack(object sender, Toolbar.NavigationClickEventArgs e)
        {
            Finish();
        }

        internal void ToggleCalendarState(bool forceHide)
        {
            reportsMainContainer.ToggleCalendar(forceHide);
        }

        internal void RecalculateCalendarHeight()
        {
            reportsMainContainer.RecalculateCalendarHeight();
        }
    }
}
