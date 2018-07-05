using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
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
            OverridePendingTransition(Resource.Animation.abc_slide_in_right, Resource.Animation.abc_fade_out);

            reportsRecyclerView = FindViewById<ReportsRecyclerView>(Resource.Id.ReportsActivityRecyclerView);
            reportsMainContainer = FindViewById<ReportsLinearLayout>(Resource.Id.ReportsActivityMainContainer);
            reportsMainContainer.CalendarContainer = FindViewById(Resource.Id.ReportsCalendarContainer);

            setupToolbar();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_right);
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
