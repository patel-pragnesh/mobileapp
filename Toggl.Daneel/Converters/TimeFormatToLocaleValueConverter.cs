using System;
using System.Globalization;
using Foundation;
using MvvmCross.Converters;
using Toggl.Multivac;

namespace Toggl.Daneel.Converters
{
    public sealed class TimeFormatToLocaleValueConverter : MvxValueConverter<TimeFormat, NSLocale>
    {
        private readonly NSLocale twentyFourHourTimeFormatLocale = new NSLocale("en_GB");
        private readonly NSLocale twelveHourTimeFormatLocale = new NSLocale("en_US");

        protected override NSLocale Convert(TimeFormat value, Type targetType, object parameter, CultureInfo culture)
            => value.IsTwentyFourHoursFormat
                ? twentyFourHourTimeFormatLocale
                : twelveHourTimeFormatLocale;
    }
}
