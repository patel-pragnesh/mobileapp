using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget.Helper;
using Android.Util;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Adapters;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.ReportsRecyclerView")]
    public sealed class ReportsRecyclerView : MvxRecyclerView
    {
        public ReportsRecyclerAdapter ReportsRecyclerAdapter => (ReportsRecyclerAdapter)Adapter;

        public ReportsViewModel ViewModel
        {
            get => ReportsRecyclerAdapter.ViewModel;
            set => ReportsRecyclerAdapter.ViewModel = value;
        }

        public ReportsRecyclerView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public ReportsRecyclerView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public ReportsRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle, new ReportsRecyclerAdapter())
        {
        }
    }
}
