using System;
using System.Windows.Input;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;

namespace Toggl.Giskard.Adapters
{
    public abstract class SelectCreatableEntityRecyclerAdapter : MvxRecyclerAdapter
    {
        public string Text { get; set; }

        public IMvxAsyncCommand CreateCommand { get; set; }

        private bool isSuggestingCreation;
        public bool IsSuggestingCreation
        {
            get => isSuggestingCreation;
            set
            {
                if (isSuggestingCreation == value) return;
                isSuggestingCreation = value;

                if (isSuggestingCreation)
                {
                    NotifyItemInserted(0);
                }
                else
                {
                    NotifyItemRemoved(0);
                }
            }
        }

        public SelectCreatableEntityRecyclerAdapter()
        {
        }

        public SelectCreatableEntityRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected abstract string SuggestingItemText { get; }

        protected abstract ICommand GetClickCommand(int viewType);

        public override int ItemCount
            => base.ItemCount + (IsSuggestingCreation ? 1 : 0);

        public override object GetItem(int viewPosition)
        {
            if (IsSuggestingCreation && viewPosition == 0)
                return SuggestingItemText;

            var actualViewPosition = viewPosition - (IsSuggestingCreation ? 1 : 0);
            return base.GetItem(actualViewPosition);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var inflatedView = InflateViewForHolder(parent, viewType, itemBindingContext);
            var viewHolder = new MvxRecyclerViewHolder(inflatedView, itemBindingContext)
            {
                Click = GetClickCommand(viewType),
                LongClick = ItemLongClick,
            };

            return viewHolder;
        }
    }
}