using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace Toggl.Giskard.Extensions
{
    //Default is Left | Bottom relative to the anchor when using ShowAsDropDown
    public static class PopupWindowWithDefaultGravityExtenstions
    {
        public static PopupOffsets LeftVerticallyCenteredOffsetsTo(this PopupWindow popupWindow,
            View anchor,
            int dpExtraLeftMargin = 0,
            int dpExtraTopMargin = 0,
            int dpExtraRightMargin = 0,
            int dpExtraBottomMargin = 0)
        {
            var contentWindow = popupWindow.ContentView;
            if (contentWindow == null)
            {
                throw new AndroidRuntimeException("The contentView must be set before calling this method");
            }
            
            var horizontalOffset = -(contentWindow.MeasuredWidth + dpExtraRightMargin.DpToPixels(contentWindow.Context));
            var verticalOffset = -(contentWindow.MeasuredHeight + Math.Abs(contentWindow.MeasuredHeight - anchor.Height) / 2);
            return new PopupOffsets(horizontalOffset, verticalOffset);
        }
    }
}
