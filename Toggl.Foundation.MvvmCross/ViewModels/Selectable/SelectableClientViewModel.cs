using MvvmCross.ViewModels;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectableClientViewModel : MvxNotifyPropertyChanged
    {
        public string Name { get; set; }

        public bool Selected { get; set; }

        public SelectableClientViewModel(string name, bool selected)
        {
            Ensure.Argument.IsNotNullOrWhiteSpaceString(name, nameof(name));

            Name = name;
            Selected = selected;
        }

        public override string ToString() => Name;
    }
}
