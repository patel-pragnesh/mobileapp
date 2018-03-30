using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using MvvmCross.Plugins.Color.Droid;
using Toggl.Foundation.MvvmCross.Converters;
using TogglColor = Toggl.Foundation.MvvmCross.Helper.Color;

namespace Toggl.Giskard.Converters
{
    public sealed class ReportTimeSpanLabelValueConverter : BaseReportTimeSpanLabelValueConverter<ISpannable>
    {
        private static readonly Color disabledColor = TogglColor.Reports.Disabled.ToAndroidColor();
        private static readonly Color normalColor = TogglColor.Reports.TotalTimeActivated.ToAndroidColor();

        protected override ISpannable GetFormattedString(string timeString, int lengthOfHours, bool isDisabled)
        {
            var color = isDisabled ? disabledColor : normalColor;
            
            var spannable = new SpannableString(timeString);
            var hourSpanEnd = lengthOfHours + 3;
            spannable.SetSpan(new TypefaceSpan("sans-serif"), 0, hourSpanEnd, SpanTypes.InclusiveInclusive);
            spannable.SetSpan(new TypefaceSpan("sans-serif-light"), hourSpanEnd, hourSpanEnd + 3, SpanTypes.InclusiveInclusive);
            spannable.SetSpan(new ForegroundColorSpan(color), 0, spannable.Length(), SpanTypes.InclusiveInclusive);

            return spannable;
        }
    }
}
