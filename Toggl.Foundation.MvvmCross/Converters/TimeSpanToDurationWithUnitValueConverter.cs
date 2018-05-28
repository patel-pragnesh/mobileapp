using System;
using System.Globalization;
using System.Text;
using MvvmCross.Converters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public sealed class TimeSpanToDurationWithUnitValueConverter : MvxValueConverter<TimeSpan, string>
    {
        protected override string Convert(TimeSpan value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value >= TimeSpan.FromHours(1))
                return $@"{(int)value.TotalHours:00}:{value:mm\:ss}";

            if (value >= TimeSpan.FromMinutes(1))
                return $@"{value:mm\:ss} {Resources.UnitMin}";
                               
            return $"{value:ss} {Resources.UnitSecond}";
        }
    }
}
