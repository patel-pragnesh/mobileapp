using Foundation;
using System;
using UIKit;
using ObjCRuntime;
using Toggl.Daneel.Extensions;

namespace Toggl.Daneel
{
    public sealed partial class MostUsedPage : UIView
    {
        public MostUsedPage (IntPtr handle) : base (handle)
        {
        }

        public static MostUsedPage Create()
        {
            var arr = NSBundle.MainBundle.LoadNib(nameof(MostUsedPage), null, null);
            return Runtime.GetNSObject<MostUsedPage>(arr.ValueAt(0));
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
