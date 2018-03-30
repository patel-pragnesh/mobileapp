using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Support.V4.Content;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class StartTimeEntryActivity : MvxAppCompatActivity<StartTimeEntryViewModel>
    {
        private int durationCardYOffset;
        private View durationCard;
        private View[] durationCardAnimatedViews;
        private CardState currentCardState = CardState.Collapsed;

        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.blueStatusBarBackground)));

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.StartTimeEntryActivity);

            OverridePendingTransition(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_fade_out);

            durationCardYOffset = (int)144.DpToPixels(this);
            durationCard = FindViewById(Resource.Id.StartTimeEntryDurationCard);
            durationCardAnimatedViews = new[] { FindViewById(Resource.Id.StartTimeEntryDoneButton), durationCard };

            durationCard.Click += onDurationCardClick;
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                ViewModel.BackCommand.ExecuteAsync();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        protected override void OnResume()
        {
            base.OnResume();
            FindViewById<EditText>(Resource.Id.StartTimeEntryDescriptionTextField).RequestFocus();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_slide_out_bottom);

            durationCard.Click -= onDurationCardClick;
        }

        private async void onDurationCardClick(object sender, EventArgs _)
        {
            if (currentCardState == CardState.Animating) return;

            var cardIsCurrentlyCollapsed = currentCardState == CardState.Collapsed;
            var nextState = cardIsCurrentlyCollapsed ? CardState.FullyVisible : CardState.Collapsed;
            var offset = cardIsCurrentlyCollapsed ? -durationCardYOffset : durationCardYOffset;

            currentCardState = CardState.Animating;

            await durationCardAnimatedViews
                .Select(animationTask)
                .Apply(Task.WhenAll);

            currentCardState = nextState;

            Task animationTask(View view)
            {
                var tcs = new TaskCompletionSource<object>();
                view.Animate().YBy(offset).WithEndAction(new Runnable(() => tcs.SetResult(null)));
                return tcs.Task;
            }
        }

        private enum CardState
        {
            Collapsed,
            Animating,
            FullyVisible
        }
    }
}
