using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SelectClientTemplateSelector : IMvxTemplateSelector
    {
        public const int Normal = 0;
        public const int CreateEntity = 1;

        public int ItemTemplateId { get; set; }

        public int GetItemLayoutId(int fromViewType)
            => fromViewType == Normal ? Resource.Layout.SelectClientActivityCell : Resource.Layout.AbcCreateEntityCell;

        public int GetItemViewType(object forItemObject)
            => forItemObject is string ? CreateEntity : Normal;
    }
}
