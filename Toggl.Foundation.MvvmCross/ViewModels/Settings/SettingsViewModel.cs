using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.Transformations;
using Toggl.Foundation.MvvmCross.ViewModels.Settings;
using Toggl.Foundation.Services;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave.Network;
using static Toggl.Multivac.Extensions.CommonFunctions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SettingsViewModel : MvxViewModel
    {
        private const string feedbackRecipient = "support@toggl.com";
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly UserAgent userAgent;
        private readonly IMailService mailService;
        private readonly ITogglDataSource dataSource;
        private readonly IDialogService dialogService;
        private readonly IUserPreferences userPreferences;
        private readonly IAnalyticsService analyticsService;
        private readonly IPlatformConstants platformConstants;
        private readonly IInteractorFactory interactorFactory;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMvxNavigationService navigationService;

        private readonly IDisposable userSubscription;
        private readonly IDisposable observableSubscription;

        private IThreadSafeUser currentUser;
        private IThreadSafePreferences currentPreferences;

        public string Title { get; } = Resources.Settings;

        internal IObservable<string> Email { get; }

        internal IObservable<string> WorkspaceName { get; }

        internal IObservable<string> CurrentDateFormat { get; }

        internal IObservable<bool> UseTwentyFourHourFormat { get; }

        internal IObservable<string> Duration { get; }

        internal IObservable<string> BeginningOfWeek { get; }

        internal IObservable<bool> IsManualModeEnabled { get; set; }

        internal IObservable<bool> IsRunningSync { get; }

        internal IObservable<bool> IsSynced { get; }

        public string Version => userAgent.Version;

        public string CurrentPlan { get; private set; } = "";

        public bool AddMobileTag { get; set; }

        public bool IsLoggingOut { get; private set; }

        public MvxObservableCollection<SelectableWorkspaceViewModel> Workspaces { get; }
            = new MvxObservableCollection<SelectableWorkspaceViewModel>();

        public SettingsViewModel(
            UserAgent userAgent,
            IMailService mailService,
            ITogglDataSource dataSource,
            IDialogService dialogService,
            IInteractorFactory interactorFactory,
            IPlatformConstants platformConstants,
            IUserPreferences userPreferences,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService,
            IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(userAgent, nameof(userAgent));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(mailService, nameof(mailService));
            Ensure.Argument.IsNotNull(dialogService, nameof(dialogService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(platformConstants, nameof(platformConstants));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.userAgent = userAgent;
            this.dataSource = dataSource;
            this.mailService = mailService;
            this.dialogService = dialogService;
            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;
            this.platformConstants = platformConstants;
            this.userPreferences = userPreferences;
            this.onboardingStorage = onboardingStorage;
            this.analyticsService = analyticsService;

            userSubscription = dataSource.User.Current
                .Subscribe(user => currentUser = user);

            observableSubscription = 
                dataSource.Preferences.Current
                    .Subscribe(preferences => currentPreferences = preferences);

            IsManualModeEnabled = userPreferences.IsManualModeEnabledObservable;

            IsSynced = dataSource.SyncManager.ProgressObservable.SelectMany(checkSynced);

            IsRunningSync =
                dataSource.SyncManager
                    .ProgressObservable
                    .Select(isRunningSync);

            Email = 
                dataSource.User.Current
                    .Select(user => user.Email.ToString())
                    .DistinctUntilChanged();

            WorkspaceName =
                dataSource.User.Current
                    .DistinctUntilChanged(user => user.DefaultWorkspaceId)
                    .SelectMany(_ => interactorFactory.GetDefaultWorkspace().Execute())
                    .Select(workspace => workspace.Name);

            BeginningOfWeek =
                dataSource.User.Current
                    .Select(user => user.BeginningOfWeek)
                    .DistinctUntilChanged()
                    .Select(beginningOfWeek => beginningOfWeek.ToString());

            CurrentDateFormat =
                dataSource.Preferences.Current
                    .Select(preferences => preferences.DateFormat.Localized)
                    .DistinctUntilChanged();

            Duration =
                dataSource.Preferences.Current
                    .Select(preferences => preferences.DurationFormat)
                    .Select(DurationFormatToString.Convert)
                    .DistinctUntilChanged();

            UseTwentyFourHourFormat =
                dataSource.Preferences.Current
                    .Select(preferences => preferences.TimeOfDayFormat.IsTwentyFourHoursFormat)
                    .DistinctUntilChanged();
        }

        public override async Task Initialize()
        {
            currentUser = await dataSource.User.Current.FirstAsync();
            currentPreferences = await dataSource.Preferences.Current.FirstAsync();

            var workspaces = await interactorFactory.GetAllWorkspaces().Execute();
            foreach (var workspace in workspaces)
            {
                Workspaces.Add(new SelectableWorkspaceViewModel(workspace, workspace.Id == currentUser.DefaultWorkspaceId));
            }
        }

        public void Rate() => throw new NotImplementedException();

        public void Update() => throw new NotImplementedException();

        public void EditProfile() => throw new NotImplementedException();

        public void EditSubscription() => throw new NotImplementedException();

        public void ToggleAddMobileTag() => AddMobileTag = !AddMobileTag;

        public Task Back() 
            => navigationService.Close(this);

        public Task OpenAboutPage()
            => navigationService.Navigate<AboutViewModel>();

        public void Help() => navigationService
            .Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(platformConstants.HelpUrl, Resources.Help));

        public async Task PickWorkspace()
        {
            var parameters = WorkspaceParameters.Create(currentUser.DefaultWorkspaceId, Resources.SetDefaultWorkspace, allowQuerying: false);
            var selectedWorkspaceId =
                await navigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(parameters);

            await changeDefaultWorkspace(selectedWorkspaceId);
        }

        public async Task SelectDefaultWorkspace(SelectableWorkspaceViewModel workspace)
        {
            foreach (var ws in Workspaces)
                ws.Selected = ws.WorkspaceId == workspace.WorkspaceId;

            await changeDefaultWorkspace(workspace.WorkspaceId);
        }

        public async Task SubmitFeedback()
        {
            var version = userAgent.ToString();
            var phone = platformConstants.PhoneModel;
            var os = platformConstants.OperatingSystem;

            var messageBuilder = new StringBuilder();
            messageBuilder.Append("\n\n"); // 2 leading newlines, so user user can type something above this info
            messageBuilder.Append($"Version: {version}\n");
            if (phone != null)
            {
                messageBuilder.Append($"Phone: {phone}\n");
            }
            messageBuilder.Append($"OS: {os}");

            var mailResult = await mailService.Send(
                feedbackRecipient,
                platformConstants.FeedbackEmailSubject,
                messageBuilder.ToString()
            );

            if (mailResult.Success || string.IsNullOrEmpty(mailResult.ErrorTitle))
                return;

            await dialogService.Alert(
                mailResult.ErrorTitle,
                mailResult.ErrorMessage,
                Resources.Ok
            );
        }

        public void ToggleManualMode()
        {
            if (userPreferences.IsManualModeEnabled)
            {
                userPreferences.EnableTimerMode();
            }
            else
            {
                userPreferences.EnableManualMode();
            }
        }

        public async Task TryLogout()
        {
            await isSynced().SelectMany(isSynced =>
            {
                if (isSynced)
                    return logout();

                string title = "", message = "";
                
                return dialogService
                    .Confirm(title, message, Resources.SettingsDialogButtonSignOut, Resources.Cancel)
                    .SelectMany(shouldLogout => 
                    {
                        if (shouldLogout)
                            Observable.Return(Unit.Default);

                        return logout();
                    });
            });
        }

        public async Task SelectBeginningOfWeek()
        {
            var newBeginningOfWeek = await navigationService
                .Navigate<SelectBeginningOfWeekViewModel, BeginningOfWeek, BeginningOfWeek>(currentUser.BeginningOfWeek);

            if (currentUser.BeginningOfWeek == newBeginningOfWeek)
                return;

            await dataSource.User.Update(new EditUserDTO { BeginningOfWeek = newBeginningOfWeek });
            dataSource.SyncManager.PushSync();
        }

        public async Task ToggleUseTwentyFourHourClock()
        {
            var timeFormat = currentPreferences.TimeOfDayFormat.IsTwentyFourHoursFormat
                ? TimeFormat.TwelveHoursFormat
                : TimeFormat.TwentyFourHoursFormat;

            await updatePreferences(timeFormat: timeFormat);
        }

        public async Task SelectDateFormat()
        {
            var newDateFormat = await navigationService
                .Navigate<SelectDateFormatViewModel, DateFormat, DateFormat>(currentPreferences.DateFormat);

            if (currentPreferences.DateFormat == newDateFormat)
                return;

            await updatePreferences(dateFormat: newDateFormat);
        }

        public async Task SelectDurationFormat()
        {
            var newDurationFormat = await navigationService
                .Navigate<SelectDurationFormatViewModel, DurationFormat, DurationFormat>(currentPreferences.DurationFormat);

            if (currentPreferences.DurationFormat == newDurationFormat)
                return;

            await updatePreferences(newDurationFormat);
        }

        private async Task updatePreferences(DurationFormat? durationFormat = null, DateFormat? dateFormat = null, TimeFormat? timeFormat = null)
        {
            var preferencesDto = new EditPreferencesDTO
            {
                DurationFormat = durationFormat,
                DateFormat = dateFormat,
                TimeOfDayFormat = timeFormat
            };

            await dataSource.Preferences.Update(preferencesDto);
            dataSource.SyncManager.PushSync();
        }

        private async Task changeDefaultWorkspace(long selectedWorkspaceId)
        {
            if (selectedWorkspaceId == currentUser.DefaultWorkspaceId) return;

            await dataSource.User.UpdateWorkspace(selectedWorkspaceId);
            dataSource.SyncManager.PushSync();
        }

        private IObservable<bool> checkSynced(SyncProgress progress)
        {
            if (IsLoggingOut || progress != SyncProgress.Synced)
                return Observable.Return(false);

            return isSynced();
        }

        private bool isRunningSync(SyncProgress progress)
            => IsLoggingOut == false && progress == SyncProgress.Syncing;

        private IObservable<Unit> logout()
        {
            IsLoggingOut = true;
            analyticsService.TrackLogoutEvent(LogoutSource.Settings);
            userPreferences.Reset();

            return dataSource.Logout().Do(_ => navigationService.Navigate<LoginViewModel>());
        }

        private IObservable<bool> isSynced()
            => dataSource
                .HasUnsyncedData()
                .WithLatestFrom(IsRunningSync, And);
    }
}
