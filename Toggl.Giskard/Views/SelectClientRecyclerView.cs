using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using MvvmCross.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.Adapters;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.SelectClientRecyclerView")]
    public sealed class SelectClientRecyclerView : MvxRecyclerView
    {
        public SelectClientRecyclerAdapter StartTimeEntryRecyclerAdapter => Adapter as SelectClientRecyclerAdapter;

        public IMvxAsyncCommand CreateCommand
        {
            get => StartTimeEntryRecyclerAdapter.CreateCommand;
            set => StartTimeEntryRecyclerAdapter.CreateCommand = value;
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

        public SelectClientRecyclerView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public SelectClientRecyclerView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public SelectClientRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle, new SelectClientRecyclerAdapter())
        {
        }
    }
}
