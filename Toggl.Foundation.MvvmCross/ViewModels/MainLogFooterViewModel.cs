using System;
using MvvmCross.Core.ViewModels;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class MainLogFooterViewModel : MvxViewModel
    {
        public bool IsRunning { get; set; }

        public MainLogFooterViewModel()
        {
        }
    }
}
