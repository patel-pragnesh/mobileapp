using MvvmCross.ViewModels;
using MvvmCross.UI;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectableColorViewModel : MvxNotifyPropertyChanged
    {
        public MvxColor Color { get; set; }

        public bool Selected { get; set; }

        public SelectableColorViewModel(MvxColor color, bool selected)
        {
            Ensure.Argument.IsNotNull(color, nameof(color));

            Color = color;
            Selected = selected;
        }
    }
}
