using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using MvvmCross.Plugins.Color.Droid;
using Toggl.Foundation.MvvmCross.Converters;
using TogglColor = Toggl.Foundation.MvvmCross.Helper.Color;

namespace Toggl.Giskard.Converters
{
    public sealed class ReportPercentageLabelValueConverter : BaseReportPercentageLabelValueConverter<ISpannable>
    {
        private static readonly Color normalColor = TogglColor.Reports.PercentageActivated.ToAndroidColor();
        private static readonly Color disabledColor = TogglColor.Reports.Disabled.ToAndroidColor();

        protected override ISpannable GetFormattedString(string percentage, bool isDisabled)
        {
            var color = isDisabled ? disabledColor : normalColor;
            var spannable = new SpannableString(percentage);
            spannable.SetSpan(new ForegroundColorSpan(color), 0, spannable.Length(), SpanTypes.ExclusiveExclusive);

            return spannable;
        }
    }
}
