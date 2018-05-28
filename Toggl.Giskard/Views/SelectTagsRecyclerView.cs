using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using MvvmCross.Commands;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.Adapters;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.SelectTagsRecyclerView")]
    public sealed class SelectTagsRecyclerView : MvxRecyclerView
    {
        public SelectTagsRecyclerAdapter SelectTagsRecyclerAdapter => Adapter as SelectTagsRecyclerAdapter;

        public IMvxAsyncCommand CreateCommand
        {
            get => SelectTagsRecyclerAdapter.CreateCommand;
            set => SelectTagsRecyclerAdapter.CreateCommand = value;
        }

        public string Text
        {
            get => SelectTagsRecyclerAdapter.Text;
            set => SelectTagsRecyclerAdapter.Text = value;
        }

        public bool IsSuggestingCreation
        {
            get => SelectTagsRecyclerAdapter.IsSuggestingCreation;
            set => SelectTagsRecyclerAdapter.IsSuggestingCreation = value;
        }

        public SelectTagsRecyclerView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SelectTagsRecyclerView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public SelectTagsRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle, new SelectTagsRecyclerAdapter())
        {
        }
    }
}