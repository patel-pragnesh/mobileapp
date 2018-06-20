using Android.Views;
using MvvmCross.Droid.Support.V7.AppCompat;
using Toggl.Foundation.MvvmCross.ViewModels;
using static Toggl.Giskard.Resource.Id;

namespace Toggl.Giskard.Activities
{
    public sealed partial class EditTimeEntryActivity : MvxAppCompatActivity<EditTimeEntryViewModel>
    {
        private View startTimeArea;
        private View stopTimeArea;
        private View durationArea;

        private void initializeViews()
        {
            startTimeArea = FindViewById(EditTimeLeftPart);
            stopTimeArea = FindViewById(EditTimeRightPart);
            durationArea = FindViewById(EditDuration);
        }
    }
}
