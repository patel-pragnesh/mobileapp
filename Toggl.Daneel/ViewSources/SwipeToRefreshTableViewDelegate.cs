using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using MvvmCross.ViewModels;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Daneel.Views;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using UIKit;
using static Toggl.Daneel.Extensions.TextExtensions;
using MvvmCross.Commands;

namespace Toggl.Daneel.ViewSources
{
    public sealed class SwipeToRefreshTableViewDelegate : UITableViewDelegate
    {
        private const float syncBarHeight = 26;
        private const float activityIndicatorSize = 14;
        private const float syncLabelFontSize = 12;

        private static readonly float scrollThreshold = 3 * syncBarHeight;

        private readonly UITableView tableView;
        private readonly UIColor pullToRefreshColor = Color.Main.PullToRefresh.ToNativeColor();
        private readonly UIColor syncingColor = Color.Main.Syncing.ToNativeColor();
        private readonly UIColor syncFailedColor = Color.Main.SyncFailed.ToNativeColor();
        private readonly UIColor offlineColor = Color.Main.Offline.ToNativeColor();
        private readonly UIColor syncCompletedColor = Color.Main.SyncCompleted.ToNativeColor();

        private SyncProgress syncProgress;
        private bool wasReleased;
        private bool needsRefresh;
        private bool shouldCalculateOnDeceleration;
        private bool isSyncing => syncProgress == SyncProgress.Syncing;
        private int syncIndicatorLastShown;
        private bool shouldRefreshOnTap;

        public SyncProgress SyncProgress
        {
            get => syncProgress;
            set
            {
                if (value == syncProgress) return;
                syncProgress = value;
                OnSyncProgressChanged();
            }
        }

        private readonly UIView syncStateView = new UIView();
        private readonly UILabel syncStateLabel = new UILabel();
        private readonly UIButton dismissSyncBarButton = new UIButton();
        private readonly ActivityIndicatorView activityIndicatorView = new ActivityIndicatorView();

        public IMvxCommand RefreshCommand { get; set; }

        public SwipeToRefreshTableViewDelegate(UITableView tableView)
        {
            Ensure.Argument.IsNotNull(tableView, nameof(tableView));

            this.tableView = tableView;
        }

        public void Initialize()
        {
            syncStateView.AddSubview(syncStateLabel);
            syncStateView.AddSubview(activityIndicatorView);
            syncStateView.AddSubview(dismissSyncBarButton);
            tableView.AddSubview(syncStateView);

            prepareSyncStateView();
            prepareSyncStateLabel();
            prepareActivityIndicatorView();
            prepareDismissSyncBarImageView();
        }

        private void prepareSyncStateView()
        {
            syncStateView.BackgroundColor = pullToRefreshColor;
            syncStateView.TranslatesAutoresizingMaskIntoConstraints = false;
            syncStateView.BottomAnchor.ConstraintEqualTo(tableView.TopAnchor).Active = true;
            syncStateView.WidthAnchor.ConstraintEqualTo(tableView.WidthAnchor).Active = true;
            syncStateView.CenterXAnchor.ConstraintEqualTo(tableView.CenterXAnchor).Active = true;
            syncStateView.TopAnchor.ConstraintEqualTo(tableView.Superview.TopAnchor).Active = true;
        }

        private void prepareSyncStateLabel()
        {
            syncStateLabel.TextColor = UIColor.White;
            syncStateLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            syncStateLabel.Font = syncStateLabel.Font.WithSize(syncLabelFontSize);
            syncStateLabel.CenterXAnchor.ConstraintEqualTo(syncStateView.CenterXAnchor).Active = true;
            syncStateLabel.BottomAnchor.ConstraintEqualTo(syncStateView.BottomAnchor, -6).Active = true;

            var tapGestureRecognizer = new UITapGestureRecognizer(onStatusLabelTap);
            syncStateLabel.UserInteractionEnabled = true;
            syncStateLabel.AddGestureRecognizer(tapGestureRecognizer);
        }

        private void prepareActivityIndicatorView()
        {
            activityIndicatorView.TranslatesAutoresizingMaskIntoConstraints = false;
            activityIndicatorView.WidthAnchor.ConstraintEqualTo(activityIndicatorSize).Active = true;
            activityIndicatorView.HeightAnchor.ConstraintEqualTo(activityIndicatorSize).Active = true;
            activityIndicatorView.CenterYAnchor.ConstraintEqualTo(syncStateLabel.CenterYAnchor).Active = true;
            activityIndicatorView.LeadingAnchor.ConstraintEqualTo(syncStateLabel.TrailingAnchor, 6).Active = true;
            activityIndicatorView.StartAnimation();
        }

        private void prepareDismissSyncBarImageView()
        {
            dismissSyncBarButton.Hidden = true;
            dismissSyncBarButton.SetImage(UIImage.FromBundle("icClose").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            dismissSyncBarButton.TintColor = UIColor.White;
            dismissSyncBarButton.TranslatesAutoresizingMaskIntoConstraints = false;
            dismissSyncBarButton.CenterYAnchor.ConstraintEqualTo(syncStateLabel.CenterYAnchor).Active = true;
            dismissSyncBarButton.TrailingAnchor.ConstraintEqualTo(syncStateView.TrailingAnchor, -16).Active = true;
            dismissSyncBarButton.TouchUpInside += onDismissSyncBarButtonTap;
        }

        private async void OnSyncProgressChanged()
        {
            bool hideIndicator = false;
            shouldRefreshOnTap = false;

            switch (SyncProgress)
            {
                case SyncProgress.Unknown:
                    return;

                case SyncProgress.Syncing:
                    setSyncIndicatorTextAndBackground(
                        new NSAttributedString(Resources.Syncing),
                        syncingColor);
                    setActivityIndicatorVisible(true);
                    break;

                case SyncProgress.OfflineModeDetected:
                    setSyncIndicatorTextAndBackground(
                        Resources.Offline.EndingWithRefreshIcon(syncStateLabel.Font.CapHeight),
                        offlineColor);
                    dismissSyncBarButton.Hidden = false;
                    setActivityIndicatorVisible(false);
                    shouldRefreshOnTap = true;
                    break;

                case SyncProgress.Synced:
                    setSyncIndicatorTextAndBackground(
                        Resources.SyncCompleted.EndingWithTick(syncStateLabel.Font.CapHeight),
                        syncCompletedColor);
                    hideIndicator = true;
                    setActivityIndicatorVisible(false);
                    break;

                case SyncProgress.Failed:
                    setSyncIndicatorTextAndBackground(
                        Resources.SyncFailed.EndingWithRefreshIcon(syncStateLabel.Font.CapHeight),
                        syncFailedColor);
                    dismissSyncBarButton.Hidden = false;
                    setActivityIndicatorVisible(false);
                    break;

                default:
                    throw new ArgumentException(nameof(SyncProgress));
            }

            int syncIndicatorShown = showSyncBar();

            if (!hideIndicator) return;

            await hideSyncBar(syncIndicatorShown);
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            if (!scrollView.Dragging || wasReleased) return;

            var offset = scrollView.ContentOffset.Y;
            if (offset >= 0) return;

            var needsMorePulling = System.Math.Abs(offset) < scrollThreshold;
            needsRefresh = !needsMorePulling;

            if (isSyncing) return;

            dismissSyncBarButton.Hidden = true;
            setSyncIndicatorTextAndBackground(
                new NSAttributedString(needsMorePulling ? Resources.PullDownToRefresh : Resources.ReleaseToRefresh),
                pullToRefreshColor);
        }

        public override void DraggingStarted(UIScrollView scrollView)
        {
            wasReleased = false;
        }

        public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
        {
            var offset = scrollView.ContentOffset.Y;
            if (offset >= 0) return;

            shouldCalculateOnDeceleration = willDecelerate;
            wasReleased = true;

            if (shouldCalculateOnDeceleration) return;
            refreshIfNeeded();
        }

        public override void DecelerationEnded(UIScrollView scrollView)
        {
            if (!shouldCalculateOnDeceleration) return;
            refreshIfNeeded();
        }

        private void refreshIfNeeded()
        {
            if (!needsRefresh) return;

            needsRefresh = false;
            shouldCalculateOnDeceleration = false;

            if (isSyncing)
            {
                showSyncBar();
                return;
            }

            RefreshCommand?.Execute();
        }

        private void setSyncIndicatorTextAndBackground(NSAttributedString text, UIColor backgroundColor)
        {
            UIView.Animate(Animation.Timings.EnterTiming, () =>
            {
                syncStateLabel.AttributedText = text;
                syncStateView.BackgroundColor = backgroundColor;
            });
        }

        private int showSyncBar()
        {
            if (tableView.Dragging) return syncIndicatorLastShown;
            if (tableView.ContentOffset.Y > 0) return syncIndicatorLastShown;

            tableView.SetContentOffset(new CGPoint(0, -syncBarHeight), true);
            return ++syncIndicatorLastShown;
        }

        private async Task hideSyncBar(int syncIndicatorShown)
        {
            await Task.Delay(Animation.Timings.HideSyncStateViewDelay);

            if (syncIndicatorShown != syncIndicatorLastShown) return;
            if (tableView.ContentOffset.Y > 0) return;

            tableView.SetContentOffset(CGPoint.Empty, true);
        }

        private void setActivityIndicatorVisible(bool visible)
        {
            activityIndicatorView.Hidden = !visible;

            if (visible)
                activityIndicatorView.StartAnimation();
            else
                activityIndicatorView.StopAnimation();
        }

        private void onStatusLabelTap()
        {
            if (shouldRefreshOnTap == false) return;

            RefreshCommand?.Execute();
        }

        private void onDismissSyncBarButtonTap(object sender, EventArgs e)
        {
            tableView.SetContentOffset(CGPoint.Empty, true);
        }
    }
}
