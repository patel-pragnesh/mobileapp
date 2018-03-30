using System;
using System.Globalization;
using MvvmCross.Platform.Converters;

namespace Toggl.Foundation.MvvmCross.Converters
{
    public abstract class BaseReportPercentageLabelValueConverter<TString> : MvxValueConverter<float?, TString>
    {
        protected sealed override TString Convert(float? value, Type targetType, object parameter, CultureInfo culture)
        {
            var isDisabled = value == null;
            var actualValue = isDisabled ? 0 : value.Value;

            var percentage = $"{actualValue.ToString("0.00")}%";

            return GetFormattedString(percentage, isDisabled);
        }

        protected abstract TString GetFormattedString(string percentage, bool isDisabled);
    }
}
