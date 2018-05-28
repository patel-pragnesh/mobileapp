using Foundation;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.Converters;
using UIKit;

namespace Toggl.Daneel.Converters
{
    public sealed class ReportPercentageLabelValueConverter : BaseReportPercentageLabelValueConverter<NSAttributedString>
    {
        private const int fontSize = 24;

        private static readonly UIColor normalColor = Color.Reports.PercentageActivated.ToNativeColor();
        private static readonly UIColor disabledColor = Color.Reports.Disabled.ToNativeColor();

        private readonly UIStringAttributes normalAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Medium),
            ForegroundColor = normalColor
        };

        private readonly UIStringAttributes disabledAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize, UIFontWeight.Medium),
            ForegroundColor = disabledColor
        };

        protected override NSAttributedString GetFormattedString(string percentage, bool isDisabled)
        {
            var attributes = isDisabled ? disabledAttributes : normalAttributes;
            return new NSAttributedString(percentage, attributes);
        }
    }
}
