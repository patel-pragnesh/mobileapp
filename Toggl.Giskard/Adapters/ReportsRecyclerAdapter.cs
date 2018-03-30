using System;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.TemplateSelectors;
using Toggl.Giskard.Views;

namespace Toggl.Giskard.Adapters
{
    public sealed class ReportsRecyclerAdapter : MvxRecyclerAdapter
    {
        public ReportsViewModel ViewModel { get; set; }

        public ReportsRecyclerAdapter()
        {
        }

        public ReportsRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType != ReportsTemplateSelector.Item)
                return base.OnCreateViewHolder(parent, viewType);

            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var inflatedView = InflateViewForHolder(parent, viewType, itemBindingContext);
            var viewHolder = new ReportsRecyclerViewHolder(inflatedView, itemBindingContext)
            {
                Click = ItemClick,
                LongClick = ItemLongClick
            };

            return viewHolder;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);

            if (holder is ReportsRecyclerViewHolder reportsViewHolder)
            {
                reportsViewHolder.IsLastItem = position == ItemCount - 1;
                reportsViewHolder.RecalculateSize();
            }
        }

        public override int ItemCount => 1 + base.ItemCount;

        public override object GetItem(int viewPosition) 
        {
            if (viewPosition == 0)
               return ViewModel;

            return base.GetItem(viewPosition - 1);
        }
    }
}
