using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross;
using Toggl.Giskard.Extensions;
using MvvmCross.Platforms.Android;

namespace Toggl.Giskard.Views
{
    public sealed class SuggestionsRecyclerViewHolder : MvxRecyclerViewHolder
    {
        private static readonly int defaultCardSize;
        private static readonly int firstItemMargin;
        private static readonly int lastItemMargin;

        static SuggestionsRecyclerViewHolder()
        {
            var context = Mvx.Resolve<IMvxAndroidGlobals>().ApplicationContext;
            var service = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            var display = service.DefaultDisplay;
            var size = new Point();
            display.GetSize(size);

            firstItemMargin = 14.DpToPixels(context);
            defaultCardSize = 200.DpToPixels(context);
            lastItemMargin = size.X - defaultCardSize - firstItemMargin;
        }

        public bool IsSingleItem { get; set; }

        public bool IsFirstItem { get; set; }

        public bool IsLastItem { get; set; }

        public SuggestionsRecyclerViewHolder(View itemView, IMvxAndroidBindingContext context)
            : base(itemView, context)
        {
        }

        public SuggestionsRecyclerViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        public void RecalculateMargins()
        {
            var left = IsFirstItem ? firstItemMargin : 0;
            var right = IsSingleItem ? firstItemMargin : IsLastItem ? lastItemMargin : 0;

            var marginLayoutParams = ItemView.LayoutParameters as ViewGroup.MarginLayoutParams;
            var newLayoutParams = marginLayoutParams.WithMargins(left, null, right, null);
            newLayoutParams.Width = IsSingleItem ? ViewGroup.LayoutParams.MatchParent : defaultCardSize;
            ItemView.LayoutParameters = newLayoutParams;
        }
    }

}