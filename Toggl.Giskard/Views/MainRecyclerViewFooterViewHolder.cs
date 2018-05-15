using System;
using Android.Runtime;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Views
{
    public class MainRecyclerViewFooterViewHolder : MvxRecyclerViewHolder
    {
        private bool isRunning = false;
        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                var heightInDp = isRunning ? 124 : 70;
                var layoutParams = ItemView.LayoutParameters;
                layoutParams.Height = heightInDp.DpToPixels(ItemView.Context);
                ItemView.LayoutParameters = layoutParams;
            }
        }

        public MainRecyclerViewFooterViewHolder(View itemView, IMvxAndroidBindingContext context)
            : base(itemView, context)
        {
        }

        public MainRecyclerViewFooterViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }
    }
}
