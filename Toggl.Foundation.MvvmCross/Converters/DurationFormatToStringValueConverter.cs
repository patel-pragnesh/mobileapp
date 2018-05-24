using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using Toggl.Foundation.MvvmCross.Transformations;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public class DurationFormatToStringValueConverter : MvxValueConverter<DurationFormat, string>
    {
        protected override string Convert(DurationFormat value, Type targetType, object parameter, CultureInfo culture)
            => DurationFormatToString.Convert(value);
    }
}
