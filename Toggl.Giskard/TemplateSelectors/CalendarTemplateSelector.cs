using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class CalendarTemplateSelector : MvxDefaultTemplateSelector
    {
        public CalendarTemplateSelector()
            : base(Resource.Layout.ReportsCalendarFragmentDayCell) { }
    }
}
