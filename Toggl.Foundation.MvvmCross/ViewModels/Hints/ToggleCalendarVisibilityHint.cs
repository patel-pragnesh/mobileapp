using System;
using MvvmCross.ViewModels;

namespace Toggl.Foundation.MvvmCross.ViewModels.Hints
{
    public sealed class ToggleCalendarVisibilityHint : MvxPresentationHint
    {
        public bool ForceHide { get; }

        public ToggleCalendarVisibilityHint(bool forceHide = false)
        {
            ForceHide = forceHide;
        }
    }
}
