using System;
using System.Linq;
using Foundation;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Daneel.Views;
using Toggl.Daneel.Views.StartTimeEntry;
using Toggl.Foundation;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;
using MvvmCross.Commands;

namespace Toggl.Daneel.ViewSources
{
    public sealed class StartTimeEntryTableViewSource : CreateSuggestionGroupedTableViewSource<AutocompleteSuggestion>
    {
        private const int defaultRowHeight = 48;
        private const int noEntityCellHeight = 60;
        private const string tagCellIdentifier = nameof(TagSuggestionViewCell);
        private const string taskCellIdentifier = nameof(TaskSuggestionViewCell);
        private const string headerCellIdentifier = nameof(WorkspaceHeaderViewCell);
        private const string timeEntryCellIdentifier = nameof(StartTimeEntryViewCell);
        private const string projectCellIdentifier = nameof(ProjectSuggestionViewCell);
        private const string noEntityInfoCellIdentifier = nameof(NoEntityInfoViewCell);
        private const string emptySuggestionIdentifier = nameof(StartTimeEntryEmptyViewCell);
        private const string tagIconIdentifier = "icIllustrationTagsSmall";
        private const string projectIconIdentifier = "icIllustrationProjectsSmall";

        private readonly NoEntityInfoMessage noTagsInfoMessage
            = new NoEntityInfoMessage(
                text: Resources.NoTagsInfoMessage,
                imageResource: tagIconIdentifier,
                characterToReplace: '#');
        private readonly NoEntityInfoMessage noProjectsInfoMessge
            = new NoEntityInfoMessage(
                text: Resources.NoProjectsInfoMessage,
                imageResource: projectIconIdentifier,
                characterToReplace: '@');

        public bool UseGrouping { get; set; }

        public bool IsSuggestingProjects { get; set; }

        public bool ShouldShowNoTagsInfoMessage { get; set; }

        public bool ShouldShowNoProjectsInfoMessage { get; set; }

        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand { get; set; }

        public IMvxCommand<AutocompleteSuggestion> SelectSuggestionCommand { get; set; }

        public StartTimeEntryTableViewSource(UITableView tableView)
            : base(tableView, headerCellIdentifier, "")
        {
            UseAnimations = false;

            tableView.TableFooterView = new UIView();
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.SeparatorColor = Color.StartTimeEntry.SeparatorColor.ToNativeColor();
            tableView.RegisterNibForCellReuse(TagSuggestionViewCell.Nib, tagCellIdentifier);
            tableView.RegisterNibForCellReuse(TaskSuggestionViewCell.Nib, taskCellIdentifier);
            tableView.RegisterNibForCellReuse(StartTimeEntryViewCell.Nib, timeEntryCellIdentifier);
            tableView.RegisterNibForCellReuse(NoEntityInfoViewCell.Nib, noEntityInfoCellIdentifier);
            tableView.RegisterNibForCellReuse(ProjectSuggestionViewCell.Nib, projectCellIdentifier);
            tableView.RegisterNibForCellReuse(StartTimeEntryEmptyViewCell.Nib, emptySuggestionIdentifier);
            tableView.RegisterNibForHeaderFooterViewReuse(WorkspaceHeaderViewCell.Nib, headerCellIdentifier);
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            if (!UseGrouping) return null;

            return base.GetViewForHeader(tableView, section);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = base.GetCell(tableView, indexPath);
            cell.LayoutMargins = UIEdgeInsets.Zero;
            cell.SeparatorInset = UIEdgeInsets.Zero;
            cell.PreservesSuperviewLayoutMargins = false;

            switch (cell)
            {
                case ProjectSuggestionViewCell projectCell:
                    projectCell.ToggleTasksCommand = ToggleTasksCommand;

                    var previousItemPath = NSIndexPath.FromItemSection(indexPath.Item - 1, indexPath.Section);
                    var previous = GetItemAt(previousItemPath);
                    var previousIsTask = previous is TaskSuggestion;
                    projectCell.TopSeparatorHidden = !previousIsTask;

                    var nextItemPath = NSIndexPath.FromItemSection(indexPath.Item + 1, indexPath.Section);
                    var next = GetItemAt(nextItemPath);
                    var isLastItemInSection = next == null;
                    var isLastSection = indexPath.Section == tableView.NumberOfSections() - 1;
                    projectCell.BottomSeparatorHidden = isLastItemInSection && !isLastSection;
                    break;

                case NoEntityInfoViewCell noEntityCell:
                    noEntityCell.NoEntityInfoMessage = getNoEntityInfoMessage();
                    break;
            }

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (UseGrouping) return base.RowsInSection(tableview, section);

            return GetGroupAt(section).Count()
                + (SuggestCreation ? 1 : 0)
                + (ShouldShowNoTagsInfoMessage ? 1 : 0)
                + (ShouldShowNoProjectsInfoMessage ? 1 : 0);
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            if (!UseGrouping) return 1;

            return base.NumberOfSections(tableView);
        }

        protected override object GetItemAt(NSIndexPath indexPath)
        {
            if (!UseGrouping && SuggestCreation)
            {
                var index = (int)indexPath.Item - 1;
                if (index < 0) return GetCreateSuggestionItem();
                if (ShouldShowNoTagsInfoMessage) return noTagsInfoMessage;
                if (ShouldShowNoProjectsInfoMessage) return noProjectsInfoMessge;

                var newIndexPath = NSIndexPath.FromRowSection(indexPath.Section, index);
                return GroupedItems.ElementAtOrDefault(indexPath.Section)?.ElementAtOrDefault(index);
            }

            if (ShouldShowNoTagsInfoMessage) return noTagsInfoMessage;
            if (ShouldShowNoProjectsInfoMessage) return noProjectsInfoMessge;

            return base.GetItemAt(indexPath);
        }

        protected override UITableViewHeaderFooterView GetOrCreateHeaderViewFor(UITableView tableView)
            => tableView.DequeueReusableHeaderFooterView(headerCellIdentifier);

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(getIdentifier(item), indexPath);

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
            => !UseGrouping ? 0 : base.GetHeightForHeader(tableView, section);

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (!UseGrouping && SuggestCreation)
            {
                var index = (int)indexPath.Item - 1;
                if (index < 0) return defaultRowHeight;

                return ShouldShowNoTagsInfoMessage || ShouldShowNoProjectsInfoMessage
                    ? noEntityCellHeight
                    : defaultRowHeight;
            }

            return ShouldShowNoTagsInfoMessage || ShouldShowNoProjectsInfoMessage
                ? defaultRowHeight + noEntityCellHeight
                : defaultRowHeight;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);

            var item = GetItemAt(indexPath);
            if (item is AutocompleteSuggestion autocompleteSuggestion)
                SelectSuggestionCommand.Execute(autocompleteSuggestion);
        }

        private string getIdentifier(object item)
        {
            switch (item)
            {
                case string _:
                    return CreateEntityCellIdentifier;

                case ProjectSuggestion _:
                    return projectCellIdentifier;

                case QuerySymbolSuggestion _:
                    return emptySuggestionIdentifier;

                case TagSuggestion _:
                    return tagCellIdentifier;

                case TaskSuggestion _:
                    return taskCellIdentifier;

                case NoEntityInfoMessage _:
                    return noEntityInfoCellIdentifier;

                default:
                    return timeEntryCellIdentifier;
            }
        }

        protected override object GetCreateSuggestionItem()
            => IsSuggestingProjects
                ? $"{Resources.CreateProject} \"{Text}\""
                : $"{Resources.CreateTag} \"{Text}\"";

        private NoEntityInfoMessage getNoEntityInfoMessage()
        {
            if (ShouldShowNoTagsInfoMessage)
                return noTagsInfoMessage;

            if (ShouldShowNoProjectsInfoMessage)
                return noProjectsInfoMessge;

            throw new InvalidOperationException("This method should not be called, when there is no info message to be shown");
        }

        protected override WorkspaceGroupedCollection<AutocompleteSuggestion> CloneCollection(WorkspaceGroupedCollection<AutocompleteSuggestion> collection)
            => new WorkspaceGroupedCollection<AutocompleteSuggestion>(collection.WorkspaceName, collection.WorkspaceId, collection);
    }
}
