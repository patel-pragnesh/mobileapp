﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Experiments;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Foundation.Services;
using Toggl.Foundation.Suggestions;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;

[assembly: MvxNavigation(typeof(MainViewModel), ApplicationUrls.Main.Regex)]
namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class MainViewModel : MvxViewModel
    {
        private bool isStopButtonEnabled = false;
        private string urlNavigationAction;

        private CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly IScheduler scheduler;
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IUserPreferences userPreferences;
        private readonly IAnalyticsService analyticsService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;
        private readonly TimeSpan currentTimeEntryDueTime = TimeSpan.FromMilliseconds(50);

        private RatingViewExperiment ratingViewExperiment;

        private readonly TimeSpan ratingViewTimeout = TimeSpan.FromMinutes(5);

        public TimeSpan CurrentTimeEntryElapsedTime { get; private set; } = TimeSpan.Zero;

        private DateTimeOffset? currentTimeEntryStart;

        public long? CurrentTimeEntryId { get; private set; }

        public string CurrentTimeEntryDescription { get; private set; }

        public string CurrentTimeEntryProject { get; private set; }

        public string CurrentTimeEntryProjectColor { get; private set; }

        public string CurrentTimeEntryTask { get; private set; }

        public string CurrentTimeEntryClient { get; private set; }

        public SyncProgress SyncingProgress { get; private set; }

        public int NumberOfSyncFailures { get; private set; }

        public IObservable<bool> TimeEntryCardVisibility { get; private set; }

        [DependsOn(nameof(SyncingProgress))]
        public bool ShowSyncIndicator => SyncingProgress == SyncProgress.Syncing;

        public bool IsTimeEntryRunning => CurrentTimeEntryId.HasValue;

        public bool IsAddDescriptionLabelVisible =>
            string.IsNullOrEmpty(CurrentTimeEntryDescription)
            && string.IsNullOrEmpty(CurrentTimeEntryProject);

        public bool IsLogEmpty
            => TimeEntriesLogViewModel.IsEmpty;

        public bool ShouldShowTimeEntriesLog
            => !TimeEntriesLogViewModel.IsEmpty;

        public bool ShouldShowEmptyState
            => SuggestionsViewModel.IsEmpty
            && TimeEntriesLogViewModel.IsEmpty
            && IsWelcome;

        public bool ShouldShowWelcomeBack
            => SuggestionsViewModel.IsEmpty
            && TimeEntriesLogViewModel.IsEmpty
            && !IsWelcome;

        public int TimeEntriesCount => TimeEntriesLogViewModel.TimeEntries?.Select(section => section.Count).Sum() ?? 0;

        public bool IsWelcome => TimeEntriesLogViewModel.IsWelcome;

        public bool IsInManualMode { get; set; } = false;

        public TimeEntriesLogViewModel TimeEntriesLogViewModel { get; }

        public SuggestionsViewModel SuggestionsViewModel { get; }

        public RatingViewModel RatingViewModel { get; }

        public IOnboardingStorage OnboardingStorage => onboardingStorage;

        public IMvxNavigationService NavigationService => navigationService;

        public IMvxAsyncCommand StartTimeEntryCommand { get; }

        public IMvxAsyncCommand AlternativeStartTimeEntryCommand { get; }

        public IMvxAsyncCommand StopTimeEntryCommand { get; }

        public IMvxAsyncCommand EditTimeEntryCommand { get; }

        public IMvxAsyncCommand OpenSettingsCommand { get; }

        public IMvxAsyncCommand OpenReportsCommand { get; }

        public IMvxAsyncCommand OpenSyncFailuresCommand { get; }

        public IMvxCommand RefreshCommand { get; }

        public IMvxCommand ToggleManualMode { get; }

        public MainViewModel(
            IScheduler scheduler,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IRatingService ratingService,
            IUserPreferences userPreferences,
            IFeedbackService feedbackService,
            IAnalyticsService analyticsService,
            IOnboardingStorage onboardingStorage,
            IInteractorFactory interactorFactory,
            IMvxNavigationService navigationService,
            IRemoteConfigService remoteConfigService,
            ISuggestionProviderContainer suggestionProviders)
        {
            Ensure.Argument.IsNotNull(scheduler, nameof(scheduler));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(ratingService, nameof(ratingService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(feedbackService, nameof(feedbackService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(remoteConfigService, nameof(remoteConfigService));
            Ensure.Argument.IsNotNull(suggestionProviders, nameof(suggestionProviders));

            this.scheduler = scheduler;
            this.dataSource = dataSource;
            this.timeService = timeService;
            this.userPreferences = userPreferences;
            this.analyticsService = analyticsService;
            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;
            this.onboardingStorage = onboardingStorage;

            SuggestionsViewModel = new SuggestionsViewModel(dataSource, interactorFactory, suggestionProviders);
            RatingViewModel = new RatingViewModel(timeService, dataSource, ratingService, feedbackService, analyticsService, onboardingStorage, navigationService);
            TimeEntriesLogViewModel = new TimeEntriesLogViewModel(timeService, dataSource, interactorFactory, onboardingStorage, analyticsService, navigationService);

            ratingViewExperiment = new RatingViewExperiment(timeService, dataSource, onboardingStorage, remoteConfigService);

            RefreshCommand = new MvxCommand(refresh);
            OpenReportsCommand = new MvxAsyncCommand(openReports);
            OpenSettingsCommand = new MvxAsyncCommand(openSettings);
            OpenSyncFailuresCommand = new MvxAsyncCommand(openSyncFailures);
            EditTimeEntryCommand = new MvxAsyncCommand(editTimeEntry, () => CurrentTimeEntryId.HasValue);
            StopTimeEntryCommand = new MvxAsyncCommand(stopTimeEntry, () => isStopButtonEnabled);
            StartTimeEntryCommand = new MvxAsyncCommand(startTimeEntry, () => CurrentTimeEntryId.HasValue == false);
            AlternativeStartTimeEntryCommand = new MvxAsyncCommand(alternativeStartTimeEntry, () => CurrentTimeEntryId.HasValue == false);
        }

        public void Init(string action)
        {
            urlNavigationAction = action;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await TimeEntriesLogViewModel.Initialize();
            await SuggestionsViewModel.Initialize();
            await RatingViewModel.Initialize();

            TimeEntryCardVisibility = dataSource
                .TimeEntries
                .CurrentlyRunningTimeEntry
                .Throttle(currentTimeEntryDueTime, scheduler) // avoid overwhelming the UI with frequent updates
                .Do(setRunningEntry)
                .Select(timeEntry => timeEntry != null);

            var tickDisposable = timeService
                .CurrentDateTimeObservable
                .Where(_ => currentTimeEntryStart != null)
                .Subscribe(currentTime => CurrentTimeEntryElapsedTime = currentTime - currentTimeEntryStart.Value);

            var syncManagerDisposable = dataSource
                .SyncManager
                .ProgressObservable
                .Subscribe(progress => SyncingProgress = progress);

            var isEmptyChangedDisposable = Observable.Empty<Unit>()
                .Merge(dataSource.TimeEntries.Updated.Select(_ => Unit.Default))
                .Merge(dataSource.TimeEntries.Deleted.Select(_ => Unit.Default))
                .Merge(dataSource.TimeEntries.Created.Select(_ => Unit.Default))
                .Subscribe((Unit _) =>
                {
                    RaisePropertyChanged(nameof(ShouldShowTimeEntriesLog));
                    RaisePropertyChanged(nameof(ShouldShowWelcomeBack));
                    RaisePropertyChanged(nameof(ShouldShowEmptyState));
                    RaisePropertyChanged(nameof(IsLogEmpty));
                    RaisePropertyChanged(nameof(TimeEntriesCount));
                });

            var getNumberOfSyncFailuresDisposable = interactorFactory
                .GetItemsThatFailedToSync()
                .Execute()
                .Select(i => i.Count())
                .Subscribe(n => NumberOfSyncFailures = n);

            disposeBag.Add(tickDisposable);
            disposeBag.Add(syncManagerDisposable);
            disposeBag.Add(isEmptyChangedDisposable);
            disposeBag.Add(getNumberOfSyncFailuresDisposable);

            switch (urlNavigationAction)
            {
                case ApplicationUrls.Main.Action.Continue:
                    await continueMostRecentEntry();
                    break;

                case ApplicationUrls.Main.Action.Stop:
                    await stopTimeEntry();
                    break;
            }

            ratingViewExperiment
                .RatingViewShouldBeVisible
                .Subscribe(presentRatingViewIfNeeded)
                .DisposedBy(disposeBag);
        }

        private void presentRatingViewIfNeeded(bool shouldBevisible)
        {
            if (!shouldBevisible) return;

            if (onboardingStorage.RatingViewOutcome().HasValue) return;

            navigationService.ChangePresentation(new ToggleRatingViewVisibilityHint());
            analyticsService.RatingViewWasShown.Track();
            onboardingStorage.SetRatingViewOutcome(RatingViewOutcome.NoInteraction, timeService.CurrentDateTime);
            timeService.RunAfterDelay(ratingViewTimeout, () =>
            {
                navigationService.ChangePresentation(new ToggleRatingViewVisibilityHint());
            });
        }

        private async Task continueMostRecentEntry()
        {
            await interactorFactory.ContinueMostRecentTimeEntry().Execute();
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            IsInManualMode = userPreferences.IsManualModeEnabled;
        }

        private void setRunningEntry(IThreadSafeTimeEntry timeEntry)
        {
            CurrentTimeEntryId = timeEntry?.Id;
            currentTimeEntryStart = timeEntry?.Start;
            CurrentTimeEntryDescription = timeEntry?.Description ?? "";
            CurrentTimeEntryElapsedTime = timeService.CurrentDateTime - currentTimeEntryStart ?? TimeSpan.Zero;

            CurrentTimeEntryTask = timeEntry?.Task?.Name ?? "";
            CurrentTimeEntryProject = timeEntry?.Project?.DisplayName() ?? "";
            CurrentTimeEntryProjectColor = timeEntry?.Project?.DisplayColor() ?? "";
            CurrentTimeEntryClient = timeEntry?.Project?.Client?.Name ?? "";

            isStopButtonEnabled = timeEntry != null;

            StopTimeEntryCommand.RaiseCanExecuteChanged();
            StartTimeEntryCommand.RaiseCanExecuteChanged();
            EditTimeEntryCommand.RaiseCanExecuteChanged();

            RaisePropertyChanged(nameof(IsTimeEntryRunning));
        }

        private void refresh()
        {
            dataSource.SyncManager.ForceFullSync();
        }

        private Task openSettings()
            => navigationService.Navigate<SettingsViewModel>();

        private async Task openReports()
        {
            var workspace = await interactorFactory.GetDefaultWorkspace().Execute();
            await navigationService.Navigate<ReportsViewModel, long>(workspace.Id);
        }

        private Task openSyncFailures()
            => navigationService.Navigate<SyncFailuresViewModel>();

        private Task startTimeEntry()
            => startTimeEntry(IsInManualMode);

        private Task alternativeStartTimeEntry()
            => startTimeEntry(!IsInManualMode);

        private Task startTimeEntry(bool initializeInManualMode)
        {
            OnboardingStorage.StartButtonWasTapped();

            var parameter = initializeInManualMode
                ? StartTimeEntryParameters.ForManualMode(timeService.CurrentDateTime)
                : StartTimeEntryParameters.ForTimerMode(timeService.CurrentDateTime);
            return navigationService.Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(parameter);
        }

        private async Task stopTimeEntry()
        {
            OnboardingStorage.StopButtonWasTapped();

            isStopButtonEnabled = false;
            StopTimeEntryCommand.RaiseCanExecuteChanged();

            await dataSource.TimeEntries.Stop(timeService.CurrentDateTime)
                .Do(_ => dataSource.SyncManager.PushSync());

            CurrentTimeEntryElapsedTime = TimeSpan.Zero;
        }

        private Task editTimeEntry()
            => navigationService.Navigate<EditTimeEntryViewModel, long>(CurrentTimeEntryId.Value);
    }
}
