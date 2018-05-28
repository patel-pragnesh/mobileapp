using System;
using System.Globalization;
using MvvmCross.Converters;
using Toggl.Foundation.Extensions;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    public sealed class ParametricTimeSpanToDurationValueConverter : MvxValueConverter<TimeSpan, string>
    {
        protected override string Convert(TimeSpan value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is DurationFormat == false)
                throw new ArgumentException(
                    $"Duration converter must be used with {typeof(DurationFormat)} parameter " +
                    $"- {parameter?.GetType().FullName ?? "null"} was used instead.");

            return value.ToFormattedString((DurationFormat)parameter);
        }
    }
}
