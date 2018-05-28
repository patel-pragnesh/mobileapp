using System;
using System.Collections.Generic;
using System.Globalization;
using MvvmCross.Converters;
using Toggl.Multivac;
using static Toggl.Multivac.DurationFormat;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public class DurationFormatToStringValueConverter : MvxValueConverter<DurationFormat, string>
    {
        protected override string Convert(DurationFormat value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case DurationFormat.Classic: 
                    return Resources.DurationFormatClassic;

                case DurationFormat.Improved: 
                    return Resources.DurationFormatImproved;

                case DurationFormat.Decimal: 
                    return Resources.DurationFormatDecimal;

                default:
                    throw new ArgumentException(
                        $"Duration format must be either: {nameof(Classic)}, {nameof(Improved)} or {nameof(DurationFormat.Decimal)}");
            }
        }
    }
}
