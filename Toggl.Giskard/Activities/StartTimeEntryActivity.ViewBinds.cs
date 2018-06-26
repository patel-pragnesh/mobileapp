using Android.Widget;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using static Toggl.Giskard.Resource.Id;

namespace Toggl.Giskard.Activities
{
    public partial class StartTimeEntryActivity
    {
        private TextView durationLabel;

        private void initializeViews()
        {
            durationLabel = FindViewById<TextView>(StartTimeEntryDurationText);
        }
    }
}
