using System;
using Toggl.Foundation.Reports;
using Toggl.Foundation.MvvmCross.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class ReportsTemplateSelector : IMvxTemplateSelector
    {
        public const int Header = 0;
        public const int Item = 1;

        public int GetItemLayoutId(int fromViewType)
        {
            switch (fromViewType)
            {
                case Header:
                    return Resource.Layout.ReportsActivityHeader;
                case Item:
                    return Resource.Layout.ReportsActivityItem;
            }

            throw new ArgumentOutOfRangeException(nameof(fromViewType));
        }


        public int GetItemViewType(object forItemObject)
        {
            if (forItemObject is ReportsViewModel)
                return Header;

            if (forItemObject is ChartSegment)
                return Item;

            throw new ArgumentOutOfRangeException(nameof(forItemObject));
        }
    }
}
