using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Toggl.Daneel.Views
{
    public sealed partial class TimeEntriesEmptyLogView : UIView
    {
        public TimeEntriesEmptyLogView(IntPtr handle) : base(handle)
        {
        }

        public static TimeEntriesEmptyLogView Create()
        {
            var arr = NSBundle.MainBundle.LoadNib(nameof(TimeEntriesEmptyLogView), null, null);
            return Runtime.GetNSObject<TimeEntriesEmptyLogView>(arr.ValueAt(0));
        }
    }
}
