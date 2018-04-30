using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class TagsListTemplateSelector : MvxDefaultTemplateSelector
    {
        public TagsListTemplateSelector()
            : base(Resource.Layout.EditTimeEntryTagCell) { }
    }
}
