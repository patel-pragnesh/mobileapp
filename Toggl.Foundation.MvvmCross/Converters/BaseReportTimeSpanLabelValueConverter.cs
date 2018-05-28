using System;
using System.Globalization;
using MvvmCross.Converters;

namespace Toggl.Foundation.MvvmCross.Converters
{
    public abstract class BaseReportTimeSpanLabelValueConverter<TString> : MvxValueConverter<TimeSpan, TString>
    {
        private readonly TimeSpanToDurationValueConverter innerConverter = new TimeSpanToDurationValueConverter();

        protected sealed override TString Convert(TimeSpan value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeString = innerConverter.Convert(value, targetType, parameter, culture) as string;
            var lengthOfHours = ((int)value.TotalHours).ToString().Length;

            var isDisabled = value.Ticks == 0;

            return GetFormattedString(timeString, lengthOfHours, isDisabled);
        }

        protected abstract TString GetFormattedString(string timeString, int lengthOfHours, bool isDisabled);
    }
}
