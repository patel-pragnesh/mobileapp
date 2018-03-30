using System;
using System.Linq;
using System.ComponentModel;
using Android.App;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using MvvmCross.Platform.WeakSubscription;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using static Android.Support.V7.Widget.Toolbar;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              WindowSoftInputMode = SoftInput.AdjustResize,
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class LoginActivity : MvxAppCompatActivity<LoginViewModel>
    {
        private IDisposable disposable;
        private Toolbar toolbar;

        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(Color.White, true);

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LoginActivity);

            setupGoogleText();

            setupToolbar();
        }

        private void setupToolbar()
        {
            toolbar = FindViewById<Toolbar>(Resource.Id.LoginToolbar);
            toolbar.Title = ViewModel.Title;

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            toolbar.NavigationClick += navigationClick;
            disposable = ViewModel.WeakSubscribe<PropertyChangedEventArgs>(nameof(ViewModel.Title), onTitleChanged);
        }

        private void setupGoogleText()
        {
            var text = Resources.GetString(Resource.String.common_signin_button_text_long);

            FindViewById<SignInButton>(Resource.Id.LoginGoogleLogin)
                .GetChildren<TextView>()
                .First()
                .Text = text;
        }

        private void onTitleChanged(object sender, PropertyChangedEventArgs args)
        {
            toolbar.Title = ViewModel.Title;
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                executeBackCommand();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        private void navigationClick(object sender, NavigationClickEventArgs args)
        {
            executeBackCommand();
        }

        private void executeBackCommand()
        {
            if (ViewModel.IsLoading) return;

            ViewModel.BackCommand.Execute();
        }
    }
}
