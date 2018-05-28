using MvvmCross.ViewModels;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels.Selectable
{
    [Preserve]
    public sealed class SelectableDateFormatViewModel : MvxNotifyPropertyChanged
    {
        public DateFormat DateFormat { get; }

        public bool Selected { get; set; }

        public SelectableDateFormatViewModel(DateFormat dateFormat, bool selected)
        {
            DateFormat = dateFormat;
            Selected = selected;
        }
    }
}
