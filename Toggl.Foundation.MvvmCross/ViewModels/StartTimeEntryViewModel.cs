﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using static Toggl.Foundation.Helper.Constants;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation;
using Toggl.PrimeRadiant.Settings;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Models.Interfaces;
using SelectTimeOrigin = Toggl.Foundation.MvvmCross.Parameters.SelectTimeParameters.Origin;

[assembly: MvxNavigation(typeof(StartTimeEntryViewModel), ApplicationUrls.StartTimeEntry)]
namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class StartTimeEntryViewModel : MvxViewModel<StartTimeEntryParameters>, ITimeEntryPrototype
    {
        //Fields
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IDialogService dialogService;
        private readonly IUserPreferences userPreferences;
        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;
        private readonly IAnalyticsService analyticsService;
        private readonly IAutocompleteProvider autocompleteProvider;
        private readonly Subject<TextFieldInfo> infoSubject = new Subject<TextFieldInfo>();
        private readonly Subject<AutocompleteSuggestionType> queryByTypeSubject = new Subject<AutocompleteSuggestionType>();
        private bool hasAnyTags;
        private bool hasAnyProjects;
        private long? lastProjectId;
        private DateFormat dateFormat;
        private TimeFormat timeFormat;

        private IDisposable queryDisposable;
        private IDisposable elapsedTimeDisposable;
        private IDisposable preferencesDisposable;

        private TextFieldInfo previousTextFieldInfo;
        private StartTimeEntryParameters parameter;

        private bool isRunning => !Duration.HasValue;

        private long defaultWorkspaceId;

        //Properties
        private int DescriptionByteCount
            => TextFieldInfo.Text.LengthInBytes();

        public int DescriptionRemainingBytes
            => MaxTimeEntryDescriptionLengthInBytes - DescriptionByteCount;

        public bool DescriptionLengthExceeded
            => DescriptionByteCount > MaxTimeEntryDescriptionLengthInBytes;

        public bool SuggestCreation
        {
            get
            {
                if (IsSuggestingProjects && TextFieldInfo.ProjectId.HasValue) return false;

                if (string.IsNullOrEmpty(CurrentQuery))
                    return false;

                if (IsSuggestingProjects)
                    return !Suggestions.Any(c => c.Any(s => s is ProjectSuggestion pS && pS.ProjectName == CurrentQuery))
                           && CurrentQuery.LengthInBytes() <= MaxProjectNameLengthInBytes;

                if (IsSuggestingTags)
                    return !Suggestions.Any(c => c.Any(s => s is TagSuggestion tS && tS.Name == CurrentQuery))
                           && CurrentQuery.LengthInBytes() <= MaxTagNameLengthInBytes;

                return false;
            }
        }

        public long[] TagIds => TextFieldInfo.Tags.Select(t => t.TagId).Distinct().ToArray();

        public long? ProjectId => TextFieldInfo.ProjectId;

        public long? TaskId => TextFieldInfo.TaskId;

        public string Description => TextFieldInfo.Text?.Trim() ?? "";

        public long WorkspaceId => TextFieldInfo.WorkspaceId;

        public bool IsDirty
            => TextFieldInfo.Text.Length > 0
                || TextFieldInfo.ProjectId.HasValue
                || TextFieldInfo.Tags.Length > 0
                || IsBillable
                || StartTime != parameter.StartTime
                || Duration != parameter.Duration;

        public bool UseGrouping { get; set; }

        public string CurrentQuery { get; private set; }

        public bool IsEditingTime { get; private set; }

        public bool IsSuggestingTags { get; private set; }

        public bool IsSuggestingProjects { get; private set; }

        public TextFieldInfo TextFieldInfo { get; set; }

        private TimeSpan displayedTime = TimeSpan.Zero;

        public TimeSpan DisplayedTime
        {
            get => displayedTime;
            set
            {
                if (isRunning)
                {
                    StartTime = timeService.CurrentDateTime - value;
                }
                else
                {
                    Duration = value;
                }

                displayedTime = value;

                RaisePropertyChanged();
            }
        }

        public bool IsBillable { get; private set; } = false;

        public bool IsBillableAvailable { get; private set; } = false;

        public bool IsEditingProjects { get; private set; } = false;

        public bool IsEditingTags { get; private set; } = false;

        public string PlaceholderText { get; private set; }

        public bool ShouldShowNoTagsInfoMessage
            => IsSuggestingTags && !hasAnyTags;

        public bool ShouldShowNoProjectsInfoMessage
            => IsSuggestingProjects && !hasAnyProjects;

        public DateTimeOffset StartTime { get; private set; }

        public TimeSpan? Duration { get; private set; }

        public NestableObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>, AutocompleteSuggestion> Suggestions { get; }
            = new NestableObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>, AutocompleteSuggestion>();

        public ITogglDataSource DataSource => dataSource;

        public IMvxAsyncCommand BackCommand { get; }

        public IMvxAsyncCommand DoneCommand { get; }

        public IMvxAsyncCommand<SelectTimeOrigin> SelectTimeCommand { get; }

        public IMvxAsyncCommand SetStartDateCommand { get; }

        public IMvxAsyncCommand ChangeTimeCommand { get; }

        public IMvxCommand ToggleBillableCommand { get; }

        public IMvxCommand DurationTapped { get; }

        public IMvxAsyncCommand CreateCommand { get; }

        public IMvxCommand ToggleTagSuggestionsCommand { get; }

        public IMvxCommand ToggleProjectSuggestionsCommand { get; }

        public IMvxAsyncCommand<AutocompleteSuggestion> SelectSuggestionCommand { get; }

        public IMvxCommand<ProjectSuggestion> ToggleTaskSuggestionsCommand { get; }

        public IOnboardingStorage OnboardingStorage { get; }

        public StartTimeEntryViewModel(
            ITimeService timeService,
            ITogglDataSource dataSource,
            IDialogService dialogService,
            IUserPreferences userPreferences,
            IOnboardingStorage onboardingStorage,
            IInteractorFactory interactorFactory,
            IMvxNavigationService navigationService,
            IAnalyticsService analyticsService,
            IAutocompleteProvider autocompleteProvider
        )
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(dialogService, nameof(dialogService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(autocompleteProvider, nameof(autocompleteProvider));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.dialogService = dialogService;
            this.userPreferences = userPreferences;
            this.navigationService = navigationService;
            this.interactorFactory = interactorFactory;
            this.analyticsService = analyticsService;
            this.autocompleteProvider = autocompleteProvider;

            OnboardingStorage = onboardingStorage;

            BackCommand = new MvxAsyncCommand(back);
            DoneCommand = new MvxAsyncCommand(done);
            CreateCommand = new MvxAsyncCommand(create);
            ChangeTimeCommand = new MvxAsyncCommand(changeTime);
            ToggleBillableCommand = new MvxCommand(toggleBillable);
            SetStartDateCommand = new MvxAsyncCommand(setStartDate);
            SelectTimeCommand = new MvxAsyncCommand<SelectTimeOrigin>(selectTime);
            ToggleTagSuggestionsCommand = new MvxCommand(toggleTagSuggestions);
            ToggleProjectSuggestionsCommand = new MvxCommand(toggleProjectSuggestions);
            SelectSuggestionCommand = new MvxAsyncCommand<AutocompleteSuggestion>(selectSuggestion);
            ToggleTaskSuggestionsCommand = new MvxCommand<ProjectSuggestion>(toggleTaskSuggestions);
            DurationTapped = new MvxCommand(durationTapped);
        }

        public void Init()
        {
            var now = timeService.CurrentDateTime;
            var startTimeEntryParameters = userPreferences.IsManualModeEnabled
                ? StartTimeEntryParameters.ForManualMode(now)
                : StartTimeEntryParameters.ForTimerMode(now);
            Prepare(startTimeEntryParameters);
        }

        public override void Prepare()
        {
            var queryByTypeObservable = queryByTypeSubject
                .AsObservable()
                .SelectMany(type => autocompleteProvider.Query(new QueryInfo("", type)));

            queryDisposable = infoSubject.AsObservable()
                .StartWith(TextFieldInfo)
                .Where(shouldUpdateSuggestions)
                .Select(QueryInfo.ParseFieldInfo)
                .Do(onParsedQuery)
                .SelectMany(autocompleteProvider.Query)
                .Merge(queryByTypeObservable)
                .Subscribe(onSuggestions);
        }

        public override void Prepare(StartTimeEntryParameters parameter)
        {
            this.parameter = parameter;
            StartTime = parameter.StartTime;
            Duration = parameter.Duration;

            PlaceholderText = parameter.PlaceholderText;

            setUpTimeSubscriptionIfNeeded();
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            var workspace = await interactorFactory.GetDefaultWorkspace().Execute();
            TextFieldInfo =
                await dataSource.User.Get().Select(user => TextFieldInfo.Empty(workspace.Id));
            defaultWorkspaceId = workspace.Id;

            await setBillableValues(lastProjectId);
            preferencesDisposable = dataSource.Preferences.Current
                .Subscribe(onPreferencesChanged);

            hasAnyTags = (await dataSource.Tags.GetAll()).Any();
            hasAnyProjects = (await dataSource.Projects.GetAll()).Any();
        }

        private void onPreferencesChanged(IThreadSafePreferences preferences)
        {
            dateFormat = preferences.DateFormat;
            timeFormat = preferences.TimeOfDayFormat;
        }

        private async Task selectSuggestion(AutocompleteSuggestion suggestion)
        {
            switch (suggestion)
            {
                case QuerySymbolSuggestion querySymbolSuggestion:

                    if (querySymbolSuggestion.Symbol == QuerySymbols.ProjectsString)
                    {
                        analyticsService.StartViewTapped.Track(StartViewTapSource.PickEmptyStateProjectSuggestion);
                        analyticsService.StartEntrySelectProject.Track(ProjectTagSuggestionSource.TableCellButton);
                    }

                    if (querySymbolSuggestion.Symbol == QuerySymbols.TagsString)
                    {
                        analyticsService.StartViewTapped.Track(StartViewTapSource.PickEmptyStateTagSuggestion);
                        analyticsService.StartEntrySelectTag.Track(ProjectTagSuggestionSource.TableCellButton);
                    }

                    TextFieldInfo = TextFieldInfo.WithTextAndCursor(querySymbolSuggestion.Symbol, 1);
                    break;

                case TimeEntrySuggestion timeEntrySuggestion:

                    analyticsService.StartViewTapped.Track(StartViewTapSource.PickTimeEntrySuggestion);

                    TextFieldInfo = TextFieldInfo.WithTextAndCursor(
                        timeEntrySuggestion.Description,
                        timeEntrySuggestion.Description.Length);

                    if (!timeEntrySuggestion.ProjectId.HasValue)
                    {
                        TextFieldInfo = TextFieldInfo.RemoveProjectInfo();
                        return;
                    }

                    if (timeEntrySuggestion.TaskId == null)
                    {
                        TextFieldInfo = TextFieldInfo.WithProjectInfo(
                            timeEntrySuggestion.WorkspaceId,
                            timeEntrySuggestion.ProjectId.Value,
                            timeEntrySuggestion.ProjectName,
                            timeEntrySuggestion.ProjectColor);
                        break;
                    }

                    TextFieldInfo = TextFieldInfo.WithProjectAndTaskInfo(
                        timeEntrySuggestion.WorkspaceId,
                        timeEntrySuggestion.ProjectId.Value,
                        timeEntrySuggestion.ProjectName,
                        timeEntrySuggestion.ProjectColor,
                        timeEntrySuggestion.TaskId.Value,
                        timeEntrySuggestion.TaskName);
                    break;

                case ProjectSuggestion projectSuggestion:

                    analyticsService.StartViewTapped.Track(StartViewTapSource.PickProjectSuggestion);

                    if (TextFieldInfo.WorkspaceId == projectSuggestion.WorkspaceId)
                    {
                        setProject(projectSuggestion);
                        break;
                    }

                    var shouldChangeProject = await dialogService.Confirm(
                        Resources.DifferentWorkspaceAlertTitle,
                        Resources.DifferentWorkspaceAlertMessage,
                        Resources.Ok,
                        Resources.Cancel);

                    if (!shouldChangeProject) break;

                    setProject(projectSuggestion);

                    break;

                case TaskSuggestion taskSuggestion:

                    analyticsService.StartViewTapped.Track(StartViewTapSource.PickTaskSuggestion);

                    if (TextFieldInfo.WorkspaceId == taskSuggestion.WorkspaceId)
                    {
                        setTask(taskSuggestion);
                        break;
                    }

                    var shouldChangeTask = await dialogService.Confirm(
                        Resources.DifferentWorkspaceAlertTitle,
                        Resources.DifferentWorkspaceAlertMessage,
                        Resources.Ok,
                        Resources.Cancel);

                    if (!shouldChangeTask) break;

                    setTask(taskSuggestion);

                    break;

                case TagSuggestion tagSuggestion:

                    analyticsService.StartViewTapped.Track(StartViewTapSource.PickTagSuggestion);

                    TextFieldInfo = TextFieldInfo
                        .RemoveTagQueryFromDescriptionIfNeeded()
                        .AddTag(tagSuggestion);
                    break;
            }
        }

        private async Task create()
        {
            if (IsSuggestingProjects)
                await createProject();
            else
                await createTag();
        }

        private async Task createProject()
        {
            var projectId = await navigationService.Navigate<EditProjectViewModel, string, long?>(CurrentQuery);
            if (projectId == null) return;

            var project = await dataSource.Projects.GetById(projectId.Value);
            var projectSuggestion = new ProjectSuggestion(project);

            setProject(projectSuggestion);
            hasAnyProjects = true;
        }

        private async Task createTag()
        {
            var createdTag = await dataSource.Tags
                .Create(CurrentQuery, TextFieldInfo.WorkspaceId);
            var tagSuggestion = new TagSuggestion(createdTag);
            await SelectSuggestionCommand.ExecuteAsync(tagSuggestion);
            hasAnyTags = true;
        }

        private void setProject(ProjectSuggestion projectSuggestion)
        {
            clearTagsIfNeeded(TextFieldInfo.WorkspaceId, projectSuggestion.WorkspaceId);

            TextFieldInfo = TextFieldInfo
                .RemoveProjectQueryFromDescriptionIfNeeded()
                .WithProjectInfo(
                    projectSuggestion.WorkspaceId,
                    projectSuggestion.ProjectId,
                    projectSuggestion.ProjectName,
                    projectSuggestion.ProjectColor);

            IsSuggestingProjects = false;
            queryByTypeSubject.OnNext(AutocompleteSuggestionType.None);
        }

        private void setTask(TaskSuggestion taskSuggestion)
        {
            clearTagsIfNeeded(TextFieldInfo.WorkspaceId, taskSuggestion.WorkspaceId);

            TextFieldInfo = TextFieldInfo
                .RemoveProjectQueryFromDescriptionIfNeeded()
                .WithProjectAndTaskInfo(
                    taskSuggestion.WorkspaceId,
                    taskSuggestion.ProjectId,
                    taskSuggestion.ProjectName,
                    taskSuggestion.ProjectColor,
                    taskSuggestion.TaskId,
                    taskSuggestion.Name
                );
        }

        private void clearTagsIfNeeded(long currentWorkspaceId, long newWorkspaceId)
        {
            if (currentWorkspaceId == newWorkspaceId) return;

            TextFieldInfo = TextFieldInfo.ClearTags();
        }

        private async void OnTextFieldInfoChanged()
        {
            infoSubject.OnNext(TextFieldInfo);

            if (TextFieldInfo.ProjectId == lastProjectId) return;
            lastProjectId = TextFieldInfo.ProjectId;
            await setBillableValues(lastProjectId);
        }

        private void OnDurationChanged()
        {
            if (Duration == null)
            {
                setUpTimeSubscriptionIfNeeded();
                return;
            }

            DisplayedTime = Duration.Value;

            elapsedTimeDisposable?.Dispose();
            elapsedTimeDisposable = null;
        }

        private void setUpTimeSubscriptionIfNeeded()
        {
            if (Duration != null || elapsedTimeDisposable != null) return;

            elapsedTimeDisposable = timeService.CurrentDateTimeObservable
                .Subscribe(currentTime => DisplayedTime = currentTime - StartTime);
        }

        private void durationTapped()
        {
            analyticsService.StartViewTapped.Track(StartViewTapSource.Duration);
        }

        private void toggleTagSuggestions()
        {
            if (IsSuggestingTags)
            {
                TextFieldInfo = TextFieldInfo.RemoveTagQueryFromDescriptionIfNeeded();
                IsSuggestingTags = false;
                return;
            }

            analyticsService.StartViewTapped.Track(StartViewTapSource.Tags);
            analyticsService.StartEntrySelectTag.Track(ProjectTagSuggestionSource.ButtonOverKeyboard);
            OnboardingStorage.ProjectOrTagWasAdded();
            appendSymbol(QuerySymbols.TagsString);
        }

        private void toggleProjectSuggestions()
        {
            if (IsSuggestingProjects)
            {
                IsSuggestingProjects = false;
                TextFieldInfo = TextFieldInfo.RemoveProjectQueryFromDescriptionIfNeeded();
                queryByTypeSubject.OnNext(AutocompleteSuggestionType.None);
                return;
            }

            analyticsService.StartViewTapped.Track(StartViewTapSource.Project);
            analyticsService.StartEntrySelectProject.Track(ProjectTagSuggestionSource.ButtonOverKeyboard);
            OnboardingStorage.ProjectOrTagWasAdded();

            if (TextFieldInfo.ProjectId != null)
            {
                IsSuggestingProjects = true;
                queryByTypeSubject.OnNext(AutocompleteSuggestionType.Projects);
                return;
            }

            appendSymbol(QuerySymbols.ProjectsString);
        }

        private void appendSymbol(string symbol)
        {
            var cursor = TextFieldInfo.DescriptionCursorPosition;
            var shouldAddWhitespace = cursor > 0 && Char.IsWhiteSpace(TextFieldInfo.Text[cursor - 1]) == false;
            var textToInsert = shouldAddWhitespace ? $" {symbol}" : symbol;
            var newText = TextFieldInfo.Text.Insert(cursor, textToInsert);
            TextFieldInfo = TextFieldInfo.WithTextAndCursor(newText, cursor + textToInsert.Length);
        }

        private void toggleTaskSuggestions(ProjectSuggestion projectSuggestion)
        {
            var grouping = Suggestions.FirstOrDefault(s => s.WorkspaceId == projectSuggestion.WorkspaceId);
            if (grouping == null) return;

            var suggestionIndex = grouping.IndexOf(projectSuggestion);
            if (suggestionIndex < 0) return;

            projectSuggestion.TasksVisible = !projectSuggestion.TasksVisible;

            var groupingIndex = Suggestions.IndexOf(grouping);
            Suggestions.Remove(grouping);
            Suggestions.Insert(groupingIndex,
                new WorkspaceGroupedCollection<AutocompleteSuggestion>(
                    grouping.WorkspaceName, grouping.WorkspaceId, getSuggestionsWithTasks(grouping)
                )
            );
        }

        private void toggleBillable()
        {
            analyticsService.StartViewTapped.Track(StartViewTapSource.Billable);
            IsBillable = !IsBillable;
        }

        private StartViewTapSource? getTapSourceFromBindingParameter(SelectTimeOrigin origin)
        {
            switch (origin)
            {
                case SelectTimeOrigin.StartTime:
                    return StartViewTapSource.StartTime;
                case SelectTimeOrigin.StartDate:
                    return StartViewTapSource.StartDate;
                case SelectTimeOrigin.Duration:
                    return StartViewTapSource.Duration;
                default:
                    return null;
            }
        }

        private async Task selectTime(SelectTimeOrigin origin)
        {
            if (getTapSourceFromBindingParameter(origin) is StartViewTapSource tapSource)
                analyticsService.StartViewTapped.Track(tapSource);

            IsEditingTime = true;

            var stopTime = Duration.HasValue ? (DateTimeOffset?)StartTime + Duration.Value : null;

            var parameters = SelectTimeParameters.CreateFromOrigin(origin, StartTime, stopTime)
                .WithFormats(dateFormat, timeFormat);

            var result = await navigationService
                .Navigate<SelectTimeViewModel, SelectTimeParameters, SelectTimeResultsParameters>(parameters)
                .ConfigureAwait(false);

            if (result == null)
                return;

            StartTime = result.Start;

            if (result.Stop.HasValue)
            {
                Duration = result.Stop - result.Start;
            }

            IsEditingTime = false;
        }

        private async Task changeTime()
        {
            analyticsService.StartViewTapped.Track(StartViewTapSource.StartTime);

            IsEditingTime = true;

            var currentDuration = DurationParameter.WithStartAndDuration(StartTime, Duration);

            var selectedDuration = await navigationService
                .Navigate<EditDurationViewModel, EditDurationParameters, DurationParameter>(new EditDurationParameters(currentDuration, isStartingNewEntry: true))
                .ConfigureAwait(false);

            StartTime = selectedDuration.Start;
            Duration = selectedDuration.Duration ?? Duration;

            IsEditingTime = false;
        }

        private async Task setStartDate()
        {
            analyticsService.StartViewTapped.Track(StartViewTapSource.StartDate);

            var parameters = isRunning
                ? DateTimePickerParameters.ForStartDateOfRunningTimeEntry(StartTime, timeService.CurrentDateTime)
                : DateTimePickerParameters.ForStartDateOfStoppedTimeEntry(StartTime);

            var duration = Duration;

            StartTime = await navigationService
                .Navigate<SelectDateTimeViewModel, DateTimePickerParameters, DateTimeOffset>(parameters)
                .ConfigureAwait(false);

            if (isRunning == false)
            {
                Duration = duration;
            }
        }

        private async Task back()
        {
            if (IsDirty)
            {
                var shouldDiscard = await dialogService.ConfirmDestructiveAction(ActionType.DiscardNewTimeEntry);
                if (!shouldDiscard)
                    return;
            }

            await navigationService.Close(this);
        }

        private async Task done()
        {
            await interactorFactory.CreateTimeEntry(this).Execute();
            await navigationService.Close(this);
        }

        private bool shouldUpdateSuggestions(TextFieldInfo textFieldInfo)
        {
            if (textFieldInfo.Text != previousTextFieldInfo.Text || textFieldInfo.CursorPosition >= previousTextFieldInfo.CursorPosition)
            {
                previousTextFieldInfo = TextFieldInfo;
                return true;
            }

            return false;
        }

        private void onParsedQuery(QueryInfo parsedQuery)
        {
            CurrentQuery = parsedQuery.Text?.Trim() ?? "";
            bool suggestsTags = parsedQuery.SuggestionType == AutocompleteSuggestionType.Tags;
            bool suggestsProjects = parsedQuery.SuggestionType == AutocompleteSuggestionType.Projects;

            if (!IsSuggestingTags && suggestsTags)
            {
                analyticsService.StartEntrySelectTag.Track(ProjectTagSuggestionSource.TextField);
            }

            if (!IsSuggestingProjects && suggestsProjects)
            {
                analyticsService.StartEntrySelectProject.Track(ProjectTagSuggestionSource.TextField);
            }

            IsSuggestingTags = suggestsTags;
            IsSuggestingProjects = suggestsProjects;
        }

        private void onSuggestions(IEnumerable<AutocompleteSuggestion> suggestions)
        {
            Suggestions.Clear();

            var filteredSuggestions = filterSuggestions(suggestions);
            var groupedSuggestions = groupSuggestions(filteredSuggestions).ToList();

            UseGrouping = groupedSuggestions.Count > 1;
            Suggestions.AddRange(groupedSuggestions);

            RaisePropertyChanged(nameof(SuggestCreation));
        }

        private IEnumerable<AutocompleteSuggestion> filterSuggestions(IEnumerable<AutocompleteSuggestion> suggestions)
        {
            if (TextFieldInfo.ProjectId.HasValue && !IsSuggestingProjects && !IsSuggestingTags)
            {
                return suggestions.OfType<TimeEntrySuggestion>()
                    .Where(suggestion => suggestion.ProjectId == TextFieldInfo.ProjectId.Value);
            }

            return suggestions;
        }

        private IEnumerable<WorkspaceGroupedCollection<AutocompleteSuggestion>> groupSuggestions(
            IEnumerable<AutocompleteSuggestion> suggestions)
        {
            var firstSuggestion = suggestions.FirstOrDefault();
            if (firstSuggestion is ProjectSuggestion)
                return suggestions
                    .GroupByWorkspaceAddingNoProject()
                    .OrderByDefaultWorkspaceAndName(defaultWorkspaceId);

            if (IsSuggestingTags)
                suggestions = suggestions.Where(suggestion => suggestion.WorkspaceId == TextFieldInfo.WorkspaceId);

            return suggestions
                .GroupBy(suggestion => new { suggestion.WorkspaceName, suggestion.WorkspaceId })
                .Select(grouping => new WorkspaceGroupedCollection<AutocompleteSuggestion>(
                    grouping.Key.WorkspaceName, grouping.Key.WorkspaceId, grouping.Distinct(AutocompleteSuggestionComparer.Instance)));
        }

        private async Task setBillableValues(long? currentProjectId)
        {
            var hasProject = currentProjectId.HasValue && currentProjectId.Value != ProjectSuggestion.NoProjectId;
            if (hasProject)
            {
                var projectId = currentProjectId.Value;
                IsBillableAvailable =
                    await interactorFactory.IsBillableAvailableForProject(projectId).Execute();

                IsBillable = IsBillableAvailable && await interactorFactory.ProjectDefaultsToBillable(projectId).Execute();
            }
            else
            {
                IsBillable = false;
                IsBillableAvailable = await interactorFactory.IsBillableAvailableForWorkspace(WorkspaceId).Execute();
            }
        }

        private IEnumerable<AutocompleteSuggestion> getSuggestionsWithTasks(
            IEnumerable<AutocompleteSuggestion> suggestions)
        {
            foreach (var suggestion in suggestions)
            {
                if (suggestion is TaskSuggestion) continue;

                yield return suggestion;

                if (suggestion is ProjectSuggestion projectSuggestion && projectSuggestion.TasksVisible)
                    foreach (var taskSuggestion in projectSuggestion.Tasks)
                        yield return taskSuggestion;
            }
        }

        public override void ViewDestroy()
        {
            base.ViewDestroy();
            queryDisposable?.Dispose();
            elapsedTimeDisposable?.Dispose();
            preferencesDisposable?.Dispose();
        }
    }
}
