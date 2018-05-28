using System;
using System.Globalization;
using MvvmCross.Converters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public sealed class EmailToStringValueConverter : MvxValueConverter<Email, string>
    {
        protected override string Convert(Email value, Type targetType, object parameter, CultureInfo culture)
            => value.ToString();

        protected override Email ConvertBack(string value, Type targetType, object parameter, CultureInfo culture)
            => Email.From(value);
    }
}
