using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using MvvmCross.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Giskard.Adapters;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.StartTimeEntryRecyclerView")]
    public sealed class StartTimeEntryRecyclerView : MvxRecyclerView
    {
        public StartTimeEntryRecyclerAdapter StartTimeEntryRecyclerAdapter => Adapter as StartTimeEntryRecyclerAdapter;

        public IMvxAsyncCommand CreateCommand
        {
            get => StartTimeEntryRecyclerAdapter.CreateCommand;
            set => StartTimeEntryRecyclerAdapter.CreateCommand = value;
        }

        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand
        {
            get => StartTimeEntryRecyclerAdapter.ToggleTasksCommand;
            set => StartTimeEntryRecyclerAdapter.ToggleTasksCommand = value;
        }

        public string Text
        {
            get => StartTimeEntryRecyclerAdapter.Text;
            set => StartTimeEntryRecyclerAdapter.Text = value;
        }

        public bool IsSuggestingCreation
        {
            get => StartTimeEntryRecyclerAdapter.IsSuggestingCreation;
            set => StartTimeEntryRecyclerAdapter.IsSuggestingCreation = value;
        }

        public bool IsSuggestingProjects
        {
            get => StartTimeEntryRecyclerAdapter.IsSuggestingProjects;
            set => StartTimeEntryRecyclerAdapter.IsSuggestingProjects = value;
        }

        public bool UseGrouping
        {
            get => StartTimeEntryRecyclerAdapter.UseGrouping;
            set => StartTimeEntryRecyclerAdapter.UseGrouping = value;
        }

        public StartTimeEntryRecyclerView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public StartTimeEntryRecyclerView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public StartTimeEntryRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle, new StartTimeEntryRecyclerAdapter())
        {
            SetItemViewCacheSize(20);
            DrawingCacheEnabled = true;
            DrawingCacheQuality = DrawingCacheQuality.High;
        }
    }
}
