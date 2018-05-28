using System;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Collections;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SelectProjectTemplateSelector : IMvxTemplateSelector
    {
        public const int ProjectSuggestion = 0;
        public const int TaskSuggestion = 1;
        public const int WorkspaceHeader = 2;
        public const int CreateEntity = 3;

        public int ItemTemplateId { get; set; }

        public int GetItemLayoutId(int fromViewType)
        {
            switch (fromViewType)
            {
                case ProjectSuggestion:
                    return Resource.Layout.SelectProjectActivityProjectCell;
                case TaskSuggestion:
                    return Resource.Layout.SelectProjectActivityTaskCell;
                case WorkspaceHeader:
                    return Resource.Layout.SelectProjectActivityWorkspaceHeader;
                case CreateEntity:
                    return Resource.Layout.AbcCreateEntityCell;
            }

            throw new ArgumentOutOfRangeException(nameof(fromViewType));
        }

        public int GetItemViewType(object forItemObject)
        {
            switch (forItemObject)
            {
                case ProjectSuggestion _:
                    return ProjectSuggestion;
                case TaskSuggestion _:
                    return TaskSuggestion;
                case WorkspaceGroupedCollection<AutocompleteSuggestion> _:
                    return WorkspaceHeader;
                case string _:
                    return CreateEntity;
            }

            throw new ArgumentException(nameof(forItemObject));
        }
    }
}