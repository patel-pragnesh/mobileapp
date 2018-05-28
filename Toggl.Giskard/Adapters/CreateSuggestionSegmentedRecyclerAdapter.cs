using System;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.ViewModels;

namespace Toggl.Giskard.Adapters
{
    public abstract class CreateSuggestionGroupedTableViewSource<TCollection, TItem> 
        : SegmentedRecyclerAdapter<TCollection, TItem>
        where TCollection : MvxObservableCollection<TItem>
    {
        public string Text { get; set; }

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

        public IMvxAsyncCommand CreateCommand { get; set; }

        public override int ItemCount 
            => base.ItemCount + (IsSuggestingCreation ? 1 : 0);

        protected abstract int SuggestCreationViewType { get; }

        protected CreateSuggestionGroupedTableViewSource()
        {
        }

        protected CreateSuggestionGroupedTableViewSource(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override object GetItem(int viewPosition)
        {
            if (!IsSuggestingCreation) 
                return base.GetItem(viewPosition);

            if (viewPosition == 0)
                return GetCreateSuggestionItem();

            return base.GetItem(viewPosition - 1);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var inflatedView = InflateViewForHolder(parent, viewType, itemBindingContext);
            var viewHolder = new MvxRecyclerViewHolder(inflatedView, itemBindingContext)
            {
                Click = viewType == SuggestCreationViewType ? CreateCommand : ItemClick,
                LongClick = ItemLongClick,
            };

            return viewHolder;
        }

        protected abstract object GetCreateSuggestionItem();
    }
}
