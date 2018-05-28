using System;
using System.Globalization;
using MvvmCross.Converters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public sealed class PasswordToStringValueConverter
        : MvxValueConverter<Password, string>
    {
        protected override string Convert(Password value, Type targetType, object parameter, CultureInfo culture)
            => value.ToString();

        protected override Password ConvertBack(string value, Type targetType, object parameter, CultureInfo culture)
            => Password.From(value);
    }
}
