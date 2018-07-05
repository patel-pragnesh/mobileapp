using Android.Views;
using Android.Widget;
using Toggl.Giskard.Views;
using static Toggl.Giskard.Resource.Id;

namespace Toggl.Giskard.Activities
{
    public partial class StartTimeEntryActivity
    {
        private TextView durationLabel;

        private AutocompleteEditText editText;

        private View selectProjectToolbarButton;

        private void initializeViews()
        {
            durationLabel = FindViewById<TextView>(StartTimeEntryDurationText);

            editText = FindViewById<AutocompleteEditText>(StartTimeEntryDescriptionTextField);

            selectProjectToolbarButton = FindViewById<View>(StartTimeEntryToolbarProject);
        }
    }
}
