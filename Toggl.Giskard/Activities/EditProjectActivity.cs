using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Fragments;
using static Android.Support.V7.Widget.Toolbar;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              WindowSoftInputMode = SoftInput.AdjustResize,
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class EditProjectActivity : MvxAppCompatActivity<EditProjectViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.blueStatusBarBackground)));

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EditProjectActivity);

            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
        }

        protected override void OnStart()
        {
            base.OnStart();

            setupToolbar();
        }

        private void setupToolbar()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.Toolbar);

            toolbar.Title = ViewModel.Title;

            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            toolbar.NavigationClick += onNavigateBack;
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                var fragment = SupportFragmentManager.Fragments.FirstOrDefault();
                if (fragment is SelectWorkspaceFragment selectWorkspaceFragment)
                {
                    selectWorkspaceFragment.ViewModel.CloseCommand.Execute();
                    return true;
                }

                ViewModel.CloseCommand.Execute();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        private void onNavigateBack(object sender, NavigationClickEventArgs e)
        {
            ViewModel.CloseCommand.Execute();
        }
    }
}
