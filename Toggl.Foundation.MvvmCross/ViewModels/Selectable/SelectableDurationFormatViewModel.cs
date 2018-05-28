using MvvmCross.ViewModels;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectableDurationFormatViewModel : MvxNotifyPropertyChanged
    {
        public DurationFormat DurationFormat { get; }

        public bool Selected { get; set; }

        public SelectableDurationFormatViewModel(DurationFormat durationFormat, bool selected)
        {
            DurationFormat = durationFormat;
            Selected = selected;
        }
    }
}
