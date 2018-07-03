using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views.Attributes;
using MvvmCross.Platform.WeakSubscription;
using Toggl.Foundation.MvvmCross.Onboarding.MainView;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Helper;
using Toggl.Multivac.Extensions;
using static Toggl.Foundation.Sync.SyncProgress;
using static Toggl.Giskard.Extensions.CircularRevealAnimation.AnimationType;
using FoundationResources = Toggl.Foundation.Resources;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed class MainActivity : MvxAppCompatActivity<MainViewModel>
    {
        private const int snackbarDuration = 5000;

        private CompositeDisposable disposeBag;
        private View runningEntryCardFrame;
        private FloatingActionButton playButton;
        private FloatingActionButton stopButton;
        private CoordinatorLayout coordinatorLayout;
        private PopupWindow playButtonTooltipPopupWindow;

        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(Color.ParseColor("#2C2C2C"));

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainActivity);
            OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);

            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.Toolbar));
            SupportActionBar.SetDisplayShowHomeEnabled(false);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            runningEntryCardFrame = FindViewById(Resource.Id.MainRunningTimeEntryFrame);
            runningEntryCardFrame.Visibility = ViewStates.Invisible;

            playButton = FindViewById<FloatingActionButton>(Resource.Id.MainPlayButton);
            stopButton = FindViewById<FloatingActionButton>(Resource.Id.MainStopButton);
            coordinatorLayout = FindViewById<CoordinatorLayout>(Resource.Id.MainCoordinatorLayout);

            disposeBag = new CompositeDisposable();

            disposeBag.Add(ViewModel.IsTimeEntryRunning.Subscribe(onTimeEntryCardVisibilityChanged));
            disposeBag.Add(ViewModel.WeakSubscribe<PropertyChangedEventArgs>(nameof(ViewModel.SyncingProgress), onSyncChanged));

            setupStartTimeEntryOnboardingStep();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            disposeBag?.Dispose();
            disposeBag = null;
        }

        private void onSyncChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (ViewModel.SyncingProgress)
            {
                case Failed:
                case Unknown:
                case OfflineModeDetected:

                    var errorMessage = ViewModel.SyncingProgress == OfflineModeDetected
                                     ? FoundationResources.Offline
                                     : FoundationResources.SyncFailed;

                    var snackbar = Snackbar.Make(coordinatorLayout, errorMessage, Snackbar.LengthLong)
                        .SetAction(FoundationResources.TapToRetry, onRetryTapped);
                    snackbar.SetDuration(snackbarDuration);
                    snackbar.Show();
                    break;
            }

            void onRetryTapped(View view)
            {
                ViewModel.RefreshCommand.Execute();
            }
        }

        private async void onTimeEntryCardVisibilityChanged(bool visible)
        {
            if (runningEntryCardFrame == null) return;

            var isCardVisible = runningEntryCardFrame.Visibility == ViewStates.Visible;
            if (isCardVisible == visible) return;

            var fabListener = new FabAsyncHideListener();
            var radialAnimation =
                runningEntryCardFrame
                    .AnimateWithCircularReveal()
                    .SetDuration(TimeSpan.FromSeconds(0.5))
                    .SetBehaviour((x, y, w, h) => (x, y + h, 0, w))
                    .SetType(() => visible ? Appear : Disappear);

            if (visible)
            {
                playButton.Hide(fabListener);
                await fabListener.HideAsync;

                radialAnimation
                    .OnAnimationEnd(_ => stopButton.Show())
                    .Start();
            }
            else
            {
                stopButton.Hide(fabListener);
                await fabListener.HideAsync;

                radialAnimation
                    .OnAnimationEnd(_ => playButton.Show())
                    .Start();
            }
        }

        private void setupStartTimeEntryOnboardingStep()
        {
            if (playButtonTooltipPopupWindow == null)
            {
                playButtonTooltipPopupWindow = PopupWindowFactory.PopupWindowWithText(
                    this,
                    Resource.Layout.TooltipWithRightArrow,
                    Resource.Id.TooltipText,
                    Resource.String.OnboardingTapToStartTimer);
            }

            var storage = ViewModel.OnboardingStorage;

            new StartTimeEntryOnboardingStep(storage)
                .ManageDismissableTooltip(
                    playButtonTooltipPopupWindow,
                    playButton,
                    (popup, anchor) => popup.LeftVerticallyCenteredOffsetsTo(anchor, dpExtraRightMargin: 8),
                    storage)
                .DisposedBy(disposeBag);
        }

        private sealed class FabAsyncHideListener : FloatingActionButton.OnVisibilityChangedListener
        {
            private readonly TaskCompletionSource<object> hideTaskCompletionSource = new TaskCompletionSource<object>();

            public Task HideAsync => hideTaskCompletionSource.Task;

            public override void OnHidden(FloatingActionButton fab)
            {
                base.OnHidden(fab);
                hideTaskCompletionSource.SetResult(null);
            }
        }
    }
}
