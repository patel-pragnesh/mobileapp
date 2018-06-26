﻿using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using MvvmCross.Platform.WeakSubscription;
using MvvmCross.Plugins.Color;
using MvvmCross.Plugins.Color.iOS;
using MvvmCross.Plugins.Visibility;
using Toggl.Daneel.Combiners;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Onboarding.MainView;
using Toggl.Daneel.Suggestions;
using Toggl.Daneel.Views;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Extensions;
using Toggl.PrimeRadiant.Onboarding;
using Toggl.PrimeRadiant.Settings;
using UIKit;
using static Toggl.Foundation.MvvmCross.Helper.Animation;

namespace Toggl.Daneel.ViewControllers
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    public partial class MainViewController : MvxViewController<MainViewModel>
    {
        private const float animationAngle = 0.1f;
        private const float showCardDelay = 0.1f;

        private const float spiderHingeCornerRadius = 0.8f;
        private const float spiderHingeWidth = 16;
        private const float spiderHingeHeight = 2;
        private const float spiderXOffset = -2;

        private const float tooltipOffset = 7;
        private const float emptyViewTopMargin = 32;

        private readonly UIView spiderContainerView = new UIView();
        private readonly SpiderOnARopeView spiderBroView = new SpiderOnARopeView();
        private readonly UIButton reportsButton = new UIButton(new CGRect(0, 0, 30, 40));
        private readonly UIButton settingsButton = new UIButton(new CGRect(0, 0, 30, 40));
        private readonly UIButton syncFailuresButton = new UIButton(new CGRect(0, 0, 30, 40));
        private readonly UIImageView titleImage = new UIImageView(UIImage.FromBundle("togglLogo"));
        private readonly TimeEntriesEmptyLogView emptyStateView = TimeEntriesEmptyLogView.Create();

        private MainTableViewSource source;
        private TimeEntriesLogViewCell firstTimeEntry;

        private bool viewInitialized;
        private bool? blockedTimeEntryCardVisibilityChange;

        private DismissableOnboardingStep tapToEditStep;
        private DismissableOnboardingStep swipeLeftStep;
        private DismissableOnboardingStep swipeRightStep;

        private UIGestureRecognizer swipeLeftGestureRecognizer;

        private IDisposable cardDisposable;

        private IDisposable tapToEditDisposable;
        private IDisposable firstTimeEntryDisposable;
        private IDisposable isEmptyDisposable;
        private IDisposable startButtonOnboardingDisposable;
        private IDisposable stopButtonOnboardingDisposable;
        private IDisposable scrollDisposable;
        private IDisposable timeEntriesCountDisposable;
        private IDisposable swipeLeftOnboardingDisposable;
        private IDisposable swipeRightOnboardingDisposable;
        private IDisposable swipeLeftAnimationDisposable;
        private IDisposable swipeRightAnimationDisposable;
        private IDisposable swipeToContinueWasUsedDisposable;
        private IDisposable swipeToDeleteWasUsedDisposable;

        private readonly ISubject<bool> isRunningSubject = new BehaviorSubject<bool>(false);
        private readonly ISubject<bool> isEmptySubject = new BehaviorSubject<bool>(false);
        private readonly ISubject<int> timeEntriesCountSubject = new BehaviorSubject<int>(0);

        private readonly UIView tableHeader = new UIView();
        private readonly UIView suggestionsContaier = new UIView { TranslatesAutoresizingMaskIntoConstraints = false };
        private readonly UIView ratingViewContainer = new UIView { TranslatesAutoresizingMaskIntoConstraints = false };
        private readonly SuggestionsView suggestionsView = new SuggestionsView{ TranslatesAutoresizingMaskIntoConstraints = false };

        public MainViewController()
            : base(nameof(MainViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            source = new MainTableViewSource(TimeEntriesLogTableView);

            prepareOnboarding();

            setupTableViewHeader();

            TimeEntriesLogTableView.Source = source;

            source.Initialize();

            cardDisposable = ViewModel.TimeEntryCardVisibility
                .ObserveOn(SynchronizationContext.Current).Subscribe(onTimeEntryCardVisibilityChanged);

            var timeEntriesLogFooter = new UIView(
                new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 64)
            );
            var colorConverter = new MvxNativeColorValueConverter();
            var visibilityConverter = new MvxVisibilityValueConverter();
            var parametricTimeSpanConverter = new ParametricTimeSpanToDurationValueConverter();
            var invertedVisibilityConverter = new MvxInvertedVisibilityValueConverter();
            var timeEntriesLogFooterConverter = new BoolToConstantValueConverter<UIView>(new UIView(), timeEntriesLogFooter);
            var projectTaskClientCombiner = new ProjectTaskClientValueCombiner(
                CurrentTimeEntryProjectTaskClientLabel.Font.CapHeight,
                Color.Main.CurrentTimeEntryClientColor.ToNativeColor(),
                true
            );
            var startTimeEntryButtonManualModeIconConverter = new BoolToConstantValueConverter<UIImage>(
                UIImage.FromBundle("manualIcon"),
                UIImage.FromBundle("playIcon")
            );

            var bindingSet = this.CreateBindingSet<MainViewController, MainViewModel>();

            //Table view
            bindingSet.Bind(source)
                      .For(v => v.ObservableCollection)
                      .To(vm => vm.TimeEntriesLogViewModel.TimeEntries);

            bindingSet.Bind(source)
                      .For(v => v.SyncProgress)
                      .To(vm => vm.SyncingProgress);

            bindingSet.Bind(TimeEntriesLogTableView)
                      .For(v => v.TableFooterView)
                      .To(vm => vm.TimeEntriesLogViewModel.IsEmpty)
                      .WithConversion(timeEntriesLogFooterConverter);

            //Commands
            bindingSet.Bind(reportsButton).To(vm => vm.OpenReportsCommand);
            bindingSet.Bind(settingsButton).To(vm => vm.OpenSettingsCommand);
            bindingSet.Bind(StopTimeEntryButton).To(vm => vm.StopTimeEntryCommand);
            bindingSet.Bind(StartTimeEntryButton).To(vm => vm.StartTimeEntryCommand);
            bindingSet.Bind(EditTimeEntryButton).To(vm => vm.EditTimeEntryCommand);
            bindingSet.Bind(syncFailuresButton).To(vm => vm.OpenSyncFailuresCommand);

            bindingSet.Bind(CurrentTimeEntryCard)
                      .For(v => v.BindTap())
                      .To(vm => vm.EditTimeEntryCommand);

            bindingSet.Bind(source)
                      .For(v => v.SelectionChangedCommand)
                      .To(vm => vm.TimeEntriesLogViewModel.EditCommand);

            bindingSet.Bind(source)
                      .For(v => v.ContinueTimeEntryCommand)
                      .To(vm => vm.TimeEntriesLogViewModel.ContinueTimeEntryCommand);

            bindingSet.Bind(source)
                      .For(v => v.RefreshCommand)
                      .To(vm => vm.RefreshCommand);

            bindingSet.Bind(source)
                      .For(v => v.DeleteTimeEntryCommand)
                      .To(vm => vm.TimeEntriesLogViewModel.DeleteCommand);

            bindingSet.Bind(suggestionsView)
                      .For(v => v.SuggestionTappedCommad)
                      .To(vm => vm.SuggestionsViewModel.StartTimeEntryCommand);

            bindingSet.Bind(StartTimeEntryButton)
                      .For(v => v.BindLongPress())
                      .To(vm => vm.AlternativeStartTimeEntryCommand);

            //Visibility
            bindingSet.Bind(WelcomeBackView)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.ShouldShowWelcomeBack)
                      .WithConversion(visibilityConverter);

            bindingSet.Bind(spiderContainerView)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.ShouldShowWelcomeBack)
                      .WithConversion(visibilityConverter);

            bindingSet.Bind(spiderBroView)
                      .For(v => v.BindSpiderVisibility())
                      .To(vm => vm.ShouldShowWelcomeBack);

            bindingSet.Bind(emptyStateView)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.ShouldShowEmptyState)
                      .WithConversion(visibilityConverter);

            //Text
            bindingSet.Bind(CurrentTimeEntryDescriptionLabel).To(vm => vm.CurrentTimeEntryDescription);
            bindingSet.Bind(CurrentTimeEntryElapsedTimeLabel)
                      .To(vm => vm.CurrentTimeEntryElapsedTime)
                      .WithConversion(parametricTimeSpanConverter, DurationFormat.Improved);

            bindingSet.Bind(CurrentTimeEntryProjectTaskClientLabel)
                      .For(v => v.AttributedText)
                      .ByCombining(projectTaskClientCombiner,
                                   v => v.CurrentTimeEntryProject,
                                   v => v.CurrentTimeEntryTask,
                                   v => v.CurrentTimeEntryClient,
                                   v => v.CurrentTimeEntryProjectColor);

            //The start button
            bindingSet.Bind(StartTimeEntryButton)
                      .For(v => v.BindImage())
                      .To(vm => vm.IsInManualMode)
                      .WithConversion(startTimeEntryButtonManualModeIconConverter);

            //The sync failures button
            bindingSet.Bind(syncFailuresButton)
                .For(v => v.BindVisibility())
                .To(vm => vm.NumberOfSyncFailures)
                .WithConversion(visibilityConverter);

            bindingSet.Apply();

            View.SetNeedsLayout();
            View.LayoutIfNeeded();
        }

        private void setupTableViewHeader()
        {
            TimeEntriesLogTableView.TableHeaderView = tableHeader;

            tableHeader.TranslatesAutoresizingMaskIntoConstraints = false;
            tableHeader.WidthAnchor.ConstraintEqualTo(TimeEntriesLogTableView.WidthAnchor).Active = true;

            tableHeader.AddSubview(suggestionsContaier);
            tableHeader.AddSubview(ratingViewContainer);

            suggestionsContaier.ConstrainToViewSides(tableHeader);
            ratingViewContainer.ConstrainToViewSides(tableHeader);

            suggestionsContaier.TopAnchor.ConstraintEqualTo(tableHeader.TopAnchor).Active = true;
            suggestionsContaier.BottomAnchor.ConstraintEqualTo(ratingViewContainer.TopAnchor).Active = true;
            ratingViewContainer.BottomAnchor.ConstraintEqualTo(tableHeader.BottomAnchor).Active = true;

            suggestionsContaier.AddSubview(suggestionsView);
            suggestionsView.ConstrainInView(suggestionsContaier);
            suggestionsView.DataContext = ViewModel.SuggestionsViewModel;
        }

        private void onTimeEntryCardVisibilityChanged(bool visible)
        {
            if (!viewInitialized)
            {
                blockedTimeEntryCardVisibilityChange = visible;
                return;
            }

            if (visible)
            {
                showTimeEntryCard();
            }
            else
            {
                hideTimeEntryCard();
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationItem.TitleView = titleImage;
            NavigationItem.RightBarButtonItems = new[]
            {
                new UIBarButtonItem(settingsButton),
                new UIBarButtonItem(reportsButton)
            };

#if DEBUG
            NavigationItem.LeftBarButtonItems = new[]
            {
                new UIBarButtonItem(syncFailuresButton)
            };
#endif
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            spiderBroView.Dispose();
            tapToEditDisposable.Dispose();
            firstTimeEntryDisposable.Dispose();
            isEmptyDisposable.Dispose();

            scrollDisposable.Dispose();
            swipeLeftOnboardingDisposable.Dispose();
            swipeRightOnboardingDisposable.Dispose();
            source.OnScrolled -= onScrolled;
            ViewModel.NavigationService.AfterNavigate -= onNavigate;
            timeEntriesCountDisposable.Dispose();

            startButtonOnboardingDisposable.Dispose();
            stopButtonOnboardingDisposable.Dispose();

            swipeToContinueWasUsedDisposable?.Dispose();
            swipeToContinueWasUsedDisposable = null;

            swipeToDeleteWasUsedDisposable?.Dispose();
            swipeToDeleteWasUsedDisposable = null;

            cardDisposable?.Dispose();
            cardDisposable = null;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (viewInitialized) return;

            viewInitialized = true;

            if (blockedTimeEntryCardVisibilityChange.HasValue)
            {
                onTimeEntryCardVisibilityChanged(blockedTimeEntryCardVisibilityChange.Value);
                blockedTimeEntryCardVisibilityChange = null;
            }
        }

        RatingView ratingView;
        public void ShowRatingView()
        {
            ratingView = RatingView.Create();
            ratingView.TranslatesAutoresizingMaskIntoConstraints = false;
            ratingView.DataContext = ViewModel.RatingViewModel;
            ratingViewContainer.AddSubview(ratingView);
            ratingView.ConstrainInView(ratingViewContainer);
        }

        public void HideRatingView()
        {
            ratingView.RemoveFromSuperview();
            ratingView.Dispose();
            ratingView = null;

            //We have to scroll a little to update the header size.
            //Using LayoutSubviews(), SetNeedsLayout(), LayoutIfNeeded() etc. does not work.
            var offset = TimeEntriesLogTableView.ContentOffset;
            var rect = new CGRect
            {
                X = offset.X,
                Y = offset.Y == 0
                    ? TimeEntriesLogTableView.Frame.Height + 1
                    : offset.Y - 1,
                Width = 1,
                Height = 1
            };
            TimeEntriesLogTableView.ScrollRectToVisible(rect, true);
        }

        private void prepareViews()
        {
            //Prevent bounces in UIScrollView
            AutomaticallyAdjustsScrollViewInsets = false;

            //Card border
            CurrentTimeEntryCard.Layer.CornerRadius = 8;
            CurrentTimeEntryCard.Layer.ShadowColor = UIColor.Black.CGColor;
            CurrentTimeEntryCard.Layer.ShadowOffset = new CGSize(0, -2);
            CurrentTimeEntryCard.Layer.ShadowOpacity = 0.1f;
            CurrentTimeEntryElapsedTimeLabel.Font = CurrentTimeEntryElapsedTimeLabel.Font.GetMonospacedDigitFont();

            // Card animations
            StopTimeEntryButton.Hidden = true;
            CurrentTimeEntryCard.Hidden = true;

            //Hide play button for later animating it
            StartTimeEntryButton.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f);

            //Prepare Navigation bar images
            reportsButton.SetImage(UIImage.FromBundle("icReports"), UIControlState.Normal);
            settingsButton.SetImage(UIImage.FromBundle("icSettings"), UIControlState.Normal);
            syncFailuresButton.SetImage(UIImage.FromBundle("icWarning"), UIControlState.Normal);

            RunningEntryDescriptionFadeView.FadeLeft = true;
            RunningEntryDescriptionFadeView.FadeRight = true;

            prepareSpiderViews();
            prepareEmptyStateView();

            View.BackgroundColor = Color.Main.BackgroundColor.ToNativeColor();
        }

        private void showTimeEntryCard()
        {
            isRunningSubject.OnNext(true);

            StopTimeEntryButton.Hidden = false;
            CurrentTimeEntryCard.Hidden = false;

            AnimationExtensions.Animate(Timings.EnterTiming, showCardDelay, Curves.EaseOut,
                () => StartTimeEntryButton.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f),
                () =>
                {
                    AnimationExtensions.Animate(Timings.LeaveTimingFaster, Curves.EaseIn,
                        () => StopTimeEntryButton.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f));

                    AnimationExtensions.Animate(Timings.LeaveTiming, Curves.CardOutCurve,
                        () => CurrentTimeEntryCard.Transform = CGAffineTransform.MakeTranslation(0, 0));
                });
        }

        private void hideTimeEntryCard()
        {
            isRunningSubject.OnNext(false);

            AnimationExtensions.Animate(Timings.LeaveTimingFaster, Curves.EaseIn,
                () => StopTimeEntryButton.Transform = CGAffineTransform.MakeScale(0.01f, 0.01f),
                () => StopTimeEntryButton.Hidden = true);

            AnimationExtensions.Animate(Timings.LeaveTiming, Curves.CardOutCurve,
                () => CurrentTimeEntryCard.Transform = CGAffineTransform.MakeTranslation(0, CurrentTimeEntryCard.Frame.Height),
                () =>
                {
                    CurrentTimeEntryCard.Hidden = true;

                    AnimationExtensions.Animate(Timings.EnterTiming, Curves.EaseOut,
                        () => StartTimeEntryButton.Transform = CGAffineTransform.MakeScale(1f, 1f)
                    );
                });
        }

        //Spider is added in code, because IB doesn't allow adding subviews
        //to a UITableView and the spider needs to be a subview of the table
        //view so it reacts to pulling down to refresh
        private void prepareSpiderViews()
        {
            var spiderHinge = new UIView();

            spiderHinge.Layer.CornerRadius = spiderHingeCornerRadius;
            spiderHinge.TranslatesAutoresizingMaskIntoConstraints = false;
            spiderHinge.BackgroundColor = Color.Main.SpiderHinge.ToNativeColor();
            spiderContainerView.TranslatesAutoresizingMaskIntoConstraints = false;
            spiderBroView.TranslatesAutoresizingMaskIntoConstraints = false;
            spiderContainerView.BackgroundColor = UIColor.Clear;

            spiderContainerView.AddSubview(spiderHinge);
            spiderContainerView.AddSubview(spiderBroView);
            TimeEntriesLogTableView.AddSubview(spiderContainerView);

            //Container constraints
            spiderContainerView.WidthAnchor.ConstraintEqualTo(TimeEntriesLogTableView.WidthAnchor).Active = true;
            spiderContainerView.HeightAnchor.ConstraintEqualTo(TimeEntriesLogTableView.HeightAnchor).Active = true;
            spiderContainerView.CenterYAnchor.ConstraintEqualTo(TimeEntriesLogTableView.CenterYAnchor).Active = true;
            spiderContainerView.CenterXAnchor.ConstraintEqualTo(TimeEntriesLogTableView.CenterXAnchor).Active = true;

            //Hinge constraints
            spiderHinge.WidthAnchor.ConstraintEqualTo(spiderHingeWidth).Active = true;
            spiderHinge.HeightAnchor.ConstraintEqualTo(spiderHingeHeight).Active = true;
            spiderHinge.TopAnchor.ConstraintEqualTo(spiderContainerView.TopAnchor).Active = true;
            spiderHinge.CenterXAnchor.ConstraintEqualTo(spiderContainerView.CenterXAnchor).Active = true;

            //Spider constraints
            spiderBroView.TopAnchor.ConstraintEqualTo(spiderContainerView.TopAnchor).Active = true;
            spiderBroView.WidthAnchor.ConstraintEqualTo(spiderContainerView.WidthAnchor).Active = true;
            spiderBroView.BottomAnchor.ConstraintEqualTo(spiderContainerView.BottomAnchor).Active = true;
            spiderBroView.CenterXAnchor.ConstraintEqualTo(spiderContainerView.CenterXAnchor).Active = true;
        }

        private void prepareEmptyStateView()
        {
            emptyStateView.BackgroundColor = UIColor.Clear;
            emptyStateView.TranslatesAutoresizingMaskIntoConstraints = false;

            TimeEntriesLogTableView.AddSubview(emptyStateView);

            emptyStateView.WidthAnchor.ConstraintEqualTo(TimeEntriesLogTableView.WidthAnchor).Active = true;
            emptyStateView.HeightAnchor.ConstraintEqualTo(TimeEntriesLogTableView.HeightAnchor).Active = true;
            emptyStateView.CenterYAnchor.ConstraintEqualTo(TimeEntriesLogTableView.CenterYAnchor).Active = true;
            emptyStateView.TopAnchor.ConstraintEqualTo(TimeEntriesLogTableView.TopAnchor, emptyViewTopMargin).Active = true;
        }

        private void prepareOnboarding()
        {
            var storage = ViewModel.OnboardingStorage;

            isRunningSubject.OnNext(ViewModel.CurrentTimeEntryId.HasValue);
            isEmptySubject.OnNext(ViewModel.IsLogEmpty);

            isEmptyDisposable = ViewModel.WeakSubscribe(() => ViewModel.IsLogEmpty, onEmptyChanged);

            startButtonOnboardingDisposable = new StartTimeEntryOnboardingStep(storage)
                .ManageDismissableTooltip(StartTimeEntryOnboardingBubbleView, storage);

            stopButtonOnboardingDisposable = new StopTimeEntryOnboardingStep(storage, isRunningSubject.AsObservable())
                .ManageDismissableTooltip(StopTimeEntryOnboardingBubbleView, storage);

            tapToEditStep = new EditTimeEntryOnboardingStep(storage, isEmptySubject.AsObservable())
                .ToDismissable(nameof(EditTimeEntryOnboardingStep), storage);

            tapToEditStep.DismissByTapping(TapToEditBubbleView);
            tapToEditDisposable = tapToEditStep.ManageVisibilityOf(TapToEditBubbleView);

            prepareSwipeGesturesOnboarding(storage, tapToEditStep.ShouldBeVisible);

            scrollDisposable = tapToEditStep.ShouldBeVisible
                .CombineLatest(
                    swipeLeftStep.ShouldBeVisible,
                    swipeRightStep.ShouldBeVisible,
                    (tapToEdit, swipeLeft, swipeRight) => tapToEdit || swipeLeft || swipeRight)
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(onScrollableTooltipsVisibilityChanged);

            firstTimeEntryDisposable = source.FirstTimeEntry
                .Where(nextFirstTimeEntry => nextFirstTimeEntry != firstTimeEntry)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(onFirstTimeEntryChanged);

            ViewModel.NavigationService.AfterNavigate += onNavigate;
        }

        private void prepareSwipeGesturesOnboarding(IOnboardingStorage storage, IObservable<bool> tapToEditStepIsVisible)
        {
            timeEntriesCountSubject.OnNext(ViewModel.TimeEntriesCount);

            timeEntriesCountDisposable = ViewModel.WeakSubscribe(() => ViewModel.TimeEntriesCount, onTimeEntriesCountChanged);

            var swipeRightCanBeShown = tapToEditStepIsVisible.Select(isVisible => !isVisible);
            swipeRightStep = new SwipeRightOnboardingStep(swipeRightCanBeShown, timeEntriesCountSubject.AsObservable())
                .ToDismissable(nameof(SwipeRightOnboardingStep), storage);

            var swipeLeftCanBeShown = Observable.CombineLatest(
                tapToEditStepIsVisible,
                swipeRightStep.ShouldBeVisible,
                (tapToEditIsVisible, swipeRightIsVisble) => !tapToEditIsVisible && !swipeRightIsVisble);
            swipeLeftStep = new SwipeLeftOnboardingStep(swipeLeftCanBeShown, timeEntriesCountSubject.AsObservable())
                .ToDismissable(nameof(SwipeLeftOnboardingStep), storage);

            swipeLeftStep.DismissByTapping(SwipeLeftBubbleView);
            swipeLeftOnboardingDisposable = swipeLeftStep.ManageVisibilityOf(SwipeLeftBubbleView);
            swipeLeftAnimationDisposable = swipeLeftStep.ManageSwipeActionAnimationOf(firstTimeEntry, Direction.Left);

            swipeRightStep.DismissByTapping(SwipeRightBubbleView);
            swipeRightOnboardingDisposable = swipeRightStep.ManageVisibilityOf(SwipeRightBubbleView);
            swipeRightAnimationDisposable = swipeRightStep.ManageSwipeActionAnimationOf(firstTimeEntry, Direction.Right);

            swipeToContinueWasUsedDisposable = Observable.FromEventPattern(source, nameof(MainTableViewSource.SwipeToContinueWasUsed))
                .Subscribe(_ =>
                {
                    swipeRightStep.Dismiss();
                    swipeToContinueWasUsedDisposable?.Dispose();
                    swipeToContinueWasUsedDisposable = null;
                });

            swipeToDeleteWasUsedDisposable = Observable.FromEventPattern(source, nameof(MainTableViewSource.SwipeToDeleteWasUsed))
                .Subscribe(_ =>
                {
                    swipeLeftStep.Dismiss();
                    swipeToDeleteWasUsedDisposable?.Dispose();
                    swipeToDeleteWasUsedDisposable = null;
                });

            updateSwipeDismissGestures(firstTimeEntry);
        }

        private void onScrollableTooltipsVisibilityChanged(bool visible)
        {
            if (visible)
                source.OnScrolled += onScrolled;
            else
                source.OnScrolled -= onScrolled;
        }

        private void onFirstTimeEntryChanged(TimeEntriesLogViewCell nextFirstTimeEntry)
        {
            updateSwipeDismissGestures(nextFirstTimeEntry);
            firstTimeEntry = nextFirstTimeEntry;
            updateTooltipPositions();
        }

        private void onEmptyChanged(object sender, PropertyChangedEventArgs args)
        {
            isEmptySubject.OnNext(ViewModel.IsLogEmpty);
        }

        private void onTimeEntriesCountChanged(object sender, PropertyChangedEventArgs e)
        {
            timeEntriesCountSubject.OnNext(ViewModel.TimeEntriesCount);
        }

        private void onScrolled(object sender, EventArgs e)
        {
            updateTooltipPositions();
        }

        private void onNavigate(object sender, EventArgs e)
        {
            bool isHidden = false;
            InvokeOnMainThread(() => isHidden = TapToEditBubbleView.Hidden);

            if (isHidden == false)
            {
                tapToEditStep.Dismiss();
                ViewModel.NavigationService.AfterNavigate -= onNavigate;
            }
        }

        private void updateTooltipPositions()
        {
            if (firstTimeEntry == null) return;

            var position = TimeEntriesLogTableView.ConvertRectToView(
                firstTimeEntry.Frame, TimeEntriesLogTableView.Superview);

            TapToEditBubbleViewTopConstraint.Constant = position.Bottom + tooltipOffset;
            SwipeLeftTopConstraint.Constant = position.Y - SwipeLeftBubbleView.Frame.Height - tooltipOffset;
            SwipeRightTopConstraint.Constant = position.Y - SwipeRightBubbleView.Frame.Height - tooltipOffset;
        }

        private void updateSwipeDismissGestures(TimeEntriesLogViewCell nextFirstTimeEntry)
        {
            if (swipeLeftGestureRecognizer != null)
            {
                firstTimeEntry?.RemoveGestureRecognizer(swipeLeftGestureRecognizer);
            }

            swipeLeftAnimationDisposable?.Dispose();
            swipeRightAnimationDisposable?.Dispose();

            if (nextFirstTimeEntry == null) return;

            swipeLeftAnimationDisposable = swipeLeftStep.ManageSwipeActionAnimationOf(nextFirstTimeEntry, Direction.Left);
            swipeRightAnimationDisposable = swipeRightStep.ManageSwipeActionAnimationOf(nextFirstTimeEntry, Direction.Right);

            swipeLeftGestureRecognizer = swipeLeftStep.DismissBySwiping(nextFirstTimeEntry, Direction.Left);
        }

        internal void Reload()
        {
            var range = new NSRange(0, TimeEntriesLogTableView.NumberOfSections());
            var indexSet = NSIndexSet.FromNSRange(range);
            TimeEntriesLogTableView.ReloadSections(indexSet, UITableViewRowAnimation.None);
        }
    }
}
