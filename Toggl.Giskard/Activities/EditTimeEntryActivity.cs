using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Fragments;
using Toggl.Giskard.Views;
using static Android.Support.V7.Widget.Toolbar;
using TextView = Android.Widget.TextView;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme")]
    public sealed class EditTimeEntryActivity : MvxAppCompatActivity<EditTimeEntryViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.blueStatusBarBackground)));
            
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.EditTimeEntryActivity);

            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                ViewModel.CloseCommand.ExecuteAsync();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}
