using System;
using Foundation;
using ObjCRuntime;
using Toggl.Daneel.Extensions;
using UIKit;

namespace Toggl.Daneel
{
    public sealed partial class TrackPage : UIView
    {
        public TrackPage (IntPtr handle) : base (handle)
        {
        }

        public static TrackPage Create()
        {
            var arr = NSBundle.MainBundle.LoadNib(nameof(TrackPage), null, null);
            return Runtime.GetNSObject<TrackPage>(arr.ValueAt(0));
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            FirstCell.MockSuggestion();
            SecondCell.MockSuggestion();
            ThirdCell.MockSuggestion();
        }
    }
}
