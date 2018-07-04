using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Toggl.Multivac;

namespace Toggl.Giskard.Helper
{
    public static class PopupWindowFactory
    {
        public static PopupWindow PopupWindowWithText(Context context, int contentViewLayoutId, int tooltipTextViewId, int tooltipTextStringId)
        {
            Ensure.Argument.IsNotNull(context, nameof(context));
            Ensure.Argument.IsNotNull(tooltipTextViewId, nameof(tooltipTextViewId));
            Ensure.Argument.IsNotZero(tooltipTextViewId, nameof(tooltipTextViewId));
            Ensure.Argument.IsNotNull(tooltipTextStringId, nameof(tooltipTextStringId));
            Ensure.Argument.IsNotZero(tooltipTextStringId, nameof(tooltipTextStringId));

            var popupWindow = new PopupWindow(context);
            var popupWindowContentView = LayoutInflater.From(context).Inflate(contentViewLayoutId, null, false);
            var tooltipTextView = popupWindowContentView.FindViewById<TextView>(tooltipTextViewId);

            if (tooltipTextView == null)
            {
                throw new AndroidRuntimeException("The tooltipTextViewId must be present and must be a TextView");
            }

            tooltipTextView.Text = context.GetString(tooltipTextStringId);
            popupWindow.ContentView = popupWindowContentView;
            popupWindow.SetBackgroundDrawable(null);
            return popupWindow;
        }
    }
}
