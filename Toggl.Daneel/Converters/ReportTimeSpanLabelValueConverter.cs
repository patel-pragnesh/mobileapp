using Foundation;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.Converters;
using UIKit;

namespace Toggl.Daneel.Converters
{
    public sealed class ReportTimeSpanLabelValueConverter : BaseReportTimeSpanLabelValueConverter<NSAttributedString>
    {
        private const int fontSize = 24;

        private static readonly UIColor normalColor = Color.Reports.TotalTimeActivated.ToNativeColor();
        private static readonly UIColor disabledColor = Color.Reports.Disabled.ToNativeColor();

        private readonly UIStringAttributes normalAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Medium),
            ForegroundColor = normalColor
        };

        private readonly UIStringAttributes normalSecondsAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Light),
            ForegroundColor = normalColor
        };

        private readonly UIStringAttributes disabledAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Medium),
            ForegroundColor = disabledColor
        };

        private readonly UIStringAttributes disabledSecondsAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Light),
            ForegroundColor = disabledColor
        };

        protected override NSAttributedString GetFormattedString(string timeString, int lengthOfHours, bool isDisabled)
        {
            var result = new NSMutableAttributedString(timeString);

            var attributes = isDisabled ? disabledAttributes : normalAttributes;
            var secondsAttributes = isDisabled ? disabledSecondsAttributes : normalSecondsAttributes;

            result.AddAttributes(attributes, new NSRange(0, lengthOfHours + 3));
            result.AddAttributes(secondsAttributes, new NSRange(lengthOfHours + 3, 3));

            return result;
        }
    }
}
