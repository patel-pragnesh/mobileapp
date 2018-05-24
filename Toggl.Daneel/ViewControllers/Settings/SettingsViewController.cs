using System;
using System.Reactive.Disposables;
using MvvmCross.Binding.BindingContext;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using MvvmCross.Plugins.Color.iOS;
using MvvmCross.Plugins.Visibility;
using Toggl.Daneel.Extensions;
using Toggl.Multivac.Extensions;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.Views;
using UIKit;
using System.Reactive;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Math = System.Math;

namespace Toggl.Daneel.ViewControllers
{
    [MvxChildPresentation]
    public partial class SettingsViewController : MvxViewController<SettingsViewModel>, ISettingsView
    {
        private CompositeDisposable disposeBag = new CompositeDisposable();

        private const int verticalSpacing = 24;

        public IObservable<Unit> EmailTappedObservable => EmailView.TappedObservable();

        public IObservable<Unit> AboutTappedObservable => AboutView.TappedObservable();

        public IObservable<Unit> FeedbackTappedObservable => FeedbackView.TappedObservable();

        public IObservable<Unit> WorkspaceTappedObservable => WorkspaceView.TappedObservable();

        public IObservable<Unit> ManualModeTappedObservable => ManualModeView.TappedObservable();

        public IObservable<Unit> DateFormatTappedObservable => DateFormatView.TappedObservable();

        public IObservable<Unit> LogoutButtonTappedObservable => LogoutButton.TappedObservable();

        public IObservable<Unit> DurationFormatTappedObservable => DurationFormatView.TappedObservable();

        public IObservable<Unit> BeginningOfWeekTappedObservable => BeginningOfWeekView.TappedObservable();

        public IObservable<Unit> TwentyFourHourClockTappedObservable => TwentyFourHourClockView.TappedObservable();

        public SettingsViewController()
            : base(nameof(SettingsViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            Title = ViewModel.Title;

            this.CreateBindings(ViewModel).DisposedBy(disposeBag);

            UIApplication.Notifications
                .ObserveWillEnterForeground((sender, e) => startAnimations())
                .DisposedBy(disposeBag);
        }

        public void OnEmailChanged(string email)
        {
            EmailLabel.Text = email;
        }

        public void OnWorkspaceNameChanged(string workspaceName)
        {
            WorkspaceLabel.Text = workspaceName;
        }

        public void OnDateFormatChanged(string dateFormat)
        {
            DateFormatLabel.Text = dateFormat;
        }

        public void OnUseTwentyFourHourFormatChanged(bool useTwentyFourHourFormat)
        {
            TwentyFourHourClockSwitch.SetState(useTwentyFourHourFormat, true);
        }

        public void OnDurationChanged(string duration)
        {
            DurationFormatLabel.Text = duration;
        }

        public void OnBeginningOfWeekChanged(string beginningOfWeek)
        {
            BeginningOfWeekLabel.Text = beginningOfWeek;
        }

        public void OnManualModeChanged(bool isOnManualMode)
        {
            ManualModeSwitch.SetState(isOnManualMode, true);
        }

        public void LoggingOut()
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing == false) return;

            disposeBag?.Dispose();
            disposeBag = null;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            startAnimations();
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            tryAlignLogoutButtonWithBottomEdge();
        }

        private void prepareViews()
        {
            // Syncing indicator colors
            setIndicatorSyncColor(SyncedIcon);
            setIndicatorSyncColor(SyncingIndicator);
            setIndicatorSyncColor(LoggingOutIndicator);

            // Resize Switches
            TwentyFourHourClockSwitch.Resize();
            ManualModeSwitch.Resize();

            TopConstraint.AdaptForIos10(NavigationController.NavigationBar);
        }

        private void setIndicatorSyncColor(UIImageView imageView)
        {
            imageView.Image = imageView.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            imageView.TintColor = Color.Settings.SyncStatusText.ToNativeColor();
        }

        private void startAnimations()
        {
            SyncingActivityIndicatorView.StartAnimation();
            LoggingOutActivityIndicatorView.StartAnimation();
        }

        private void tryAlignLogoutButtonWithBottomEdge()
        {
            var contentHeight = LogoutContainerView.Frame.Top - LogoutVerticalOffsetConstraint.Constant + LogoutContainerView.Frame.Height;
            var bottomOffset = verticalSpacing;
            var idealDistance = ScrollView.Frame.Height - contentHeight - bottomOffset;
            var distance = Math.Max(idealDistance, verticalSpacing);
            LogoutVerticalOffsetConstraint.Constant = (nfloat)distance;
        }
    }
}
