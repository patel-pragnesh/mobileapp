using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.Adapters;
using Android.Views;
using MvvmCross.ViewModels;
using Toggl.Foundation.Autocomplete.Suggestions;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.SelectProjectRecyclerView")]
    public sealed class SelectProjectRecyclerView : MvxRecyclerView
    {
        public SelectProjectRecyclerAdapter SelectProjectRecyclerAdapter => Adapter as SelectProjectRecyclerAdapter;

        public string Text
        {
            get => SelectProjectRecyclerAdapter.Text;
            set => SelectProjectRecyclerAdapter.Text = value;
        }

        public bool UseGrouping
        {
            get => SelectProjectRecyclerAdapter.UseGrouping;
            set => SelectProjectRecyclerAdapter.UseGrouping = value;
        }

        public bool IsSuggestingCreation
        {
            get => SelectProjectRecyclerAdapter.IsSuggestingCreation;
            set => SelectProjectRecyclerAdapter.IsSuggestingCreation = value;
        }

        public IMvxAsyncCommand CreateCommand
        {
            get => SelectProjectRecyclerAdapter.CreateCommand;
            set => SelectProjectRecyclerAdapter.CreateCommand = value;
        }

        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand
        {
            get => SelectProjectRecyclerAdapter.ToggleTasksCommand;
            set => SelectProjectRecyclerAdapter.ToggleTasksCommand = value;
        }

        public SelectProjectRecyclerView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SelectProjectRecyclerView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public SelectProjectRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle, new SelectProjectRecyclerAdapter())
        {
            SetItemViewCacheSize(20);
            DrawingCacheEnabled = true;
            DrawingCacheQuality = DrawingCacheQuality.High;
        }
    }
}