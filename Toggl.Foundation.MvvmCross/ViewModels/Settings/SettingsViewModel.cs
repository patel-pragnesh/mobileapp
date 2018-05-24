using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels.Settings;
using Toggl.Foundation.Services;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave.Network;

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

        private long workspaceId;
        private BeginningOfWeek currentBeginningOfWeek;

        private readonly BehaviorSubject<bool> isManualModeEnabledSubject = new BehaviorSubject<bool>(false);

        public string Title { get; } = Resources.Settings;

        public IObservable<IDatabaseUser> CurrentUser { get; }

        public IObservable<bool> IsManualModeEnabled { get; set; }

        public IObservable<bool> IsRunningSync { get; }

        public IObservable<bool> IsSynced { get; }

        public string Version { get; private set; } = "";

        public string WorkspaceName { get; private set; } = "";

        public string CurrentPlan { get; private set; } = "";

        public bool UseTwentyFourHourClock { get; set; }

        public bool AddMobileTag { get; set; }

        public bool IsLoggingOut { get; private set; }
        public DateFormat DateFormat { get; private set; }

        public DurationFormat DurationFormat { get; private set; }

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

            IsRunningSync =
                dataSource.SyncManager
                    .ProgressObservable
                    .Select(isRunningSync);

            IsSynced =
                dataSource.SyncManager
                    .ProgressObservable
                    .SelectMany(checkSynced);

            CurrentUser =
                dataSource.User.Current
                    .Do(user => currentBeginningOfWeek = user.BeginningOfWeek);

            IsManualModeEnabled =
                isManualModeEnabledSubject
                    .AsObservable()
                    .Do(toggleManualModeSetting);
        }

        public override async Task Initialize()
        {
            var user = await dataSource.User.Current.FirstAsync();
            var defaultWorkspace = await interactorFactory.GetDefaultWorkspace().Execute();

            Version = userAgent.Version;
            workspaceId = defaultWorkspace.Id;
            WorkspaceName = defaultWorkspace.Name;

            var workspaces = await interactorFactory.GetAllWorkspaces().Execute();
            foreach (var workspace in workspaces)
            {
                Workspaces.Add(new SelectableWorkspaceViewModel(workspace, workspace.Id == workspaceId));
            }

            dataSource.Preferences.Current
                .Subscribe(updateFromPreferences);
        }

        private void updateFromPreferences(IDatabasePreferences preferences)
        {
            DateFormat = preferences.DateFormat;
            DurationFormat = preferences.DurationFormat;
            UseTwentyFourHourClock = preferences.TimeOfDayFormat.IsTwentyFourHoursFormat;
        }

        public void Rate() => throw new NotImplementedException();

        public void Help() => navigationService
            .Navigate<BrowserViewModel, BrowserParameters>(
                BrowserParameters.WithUrlAndTitle(platformConstants.HelpUrl, Resources.Help));

        public void Update() => throw new NotImplementedException();

        public void EditProfile()
        {
        }

        public async Task PickWorkspace()
        {
            var parameters = WorkspaceParameters.Create(workspaceId, Resources.SetDefaultWorkspace, allowQuerying: false);
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

        public void EditSubscription() => throw new NotImplementedException();

        public void ToggleAddMobileTag() => AddMobileTag = !AddMobileTag;

        public async Task ToggleUseTwentyFourHourClock()
        {
            UseTwentyFourHourClock = !UseTwentyFourHourClock;
            var timeFormat = UseTwentyFourHourClock
                ? TimeFormat.TwentyFourHoursFormat
                : TimeFormat.TwelveHoursFormat;

            var preferencesDto = new EditPreferencesDTO { TimeOfDayFormat = timeFormat };
            await updatePreferences(preferencesDto);
        }

        public void ToggleManualMode()
        {
            isManualModeEnabledSubject.OnNext(!isManualModeEnabledSubject.Value);
        }

        public Task Back() => navigationService.Close(this);

        public async Task TryLogout()
        {
            await isSynced().SelectMany(isSynced =>
            {
                if (isSynced)
                    return logout();


                
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
                .WithLatestFrom(IsRunningSync, Functions.And);

        public async Task SelectDateFormat()
        {
            var newDateFormat = await navigationService
                .Navigate<SelectDateFormatViewModel, DateFormat, DateFormat>(DateFormat);

            if (DateFormat == newDateFormat)
                return;

            var preferencesDto = new EditPreferencesDTO { DateFormat = newDateFormat };
            var newPreferences = await updatePreferences(preferencesDto);
            DateFormat = newPreferences.DateFormat;
        }

        public async Task SelectBeginningOfWeek()
        {
            var newBeginningOfWeek = await navigationService
                .Navigate<SelectBeginningOfWeekViewModel, BeginningOfWeek, BeginningOfWeek>(currentBeginningOfWeek);

            if (currentBeginningOfWeek == newBeginningOfWeek)
                return;

            var userDto = new EditUserDTO { BeginningOfWeek = newBeginningOfWeek };
            await dataSource.User.Update(userDto);

            dataSource.SyncManager.PushSync();
        }

        public Task OpenAboutPage()
            => navigationService.Navigate<AboutViewModel>();

        public async Task SelectDurationFormat()
        {
            var newDurationFormat = await navigationService
                .Navigate<SelectDurationFormatViewModel, DurationFormat, DurationFormat>(DurationFormat);

            if (DurationFormat == newDurationFormat)
                return;

            var preferencesDto = new EditPreferencesDTO { DurationFormat = newDurationFormat };
            var newPreferences = await updatePreferences(preferencesDto);
            DurationFormat = newPreferences.DurationFormat;
        }

        private async Task<IDatabasePreferences> updatePreferences(EditPreferencesDTO preferencesDto)
        {
            var newPreferences = await dataSource.Preferences.Update(preferencesDto);
            dataSource.SyncManager.PushSync();
            return newPreferences;
        }

        private async Task changeDefaultWorkspace(long selectedWorkspaceId)
        {
            if (selectedWorkspaceId == workspaceId) return;

            var workspace = await interactorFactory.GetWorkspaceById(selectedWorkspaceId).Execute();
            workspaceId = selectedWorkspaceId;
            WorkspaceName = workspace.Name;

            await dataSource.User.UpdateWorkspace(workspaceId);
            dataSource.SyncManager.PushSync();
        }


        private void toggleManualModeSetting(bool isManualModeEnabled)
        {
            if (isManualModeEnabled)
            {
                userPreferences.EnableManualMode();
            }
            else
            {
                userPreferences.EnableTimerMode();
            }
        }

        private IObservable<bool> checkSynced(SyncProgress progress)
        {
            if (IsLoggingOut || progress != SyncProgress.Synced)
                return Observable.Return(false);

            return isSynced();
        }

        private bool isRunningSync(SyncProgress progress)
            => IsLoggingOut == false && progress == SyncProgress.Syncing;
    }
}
