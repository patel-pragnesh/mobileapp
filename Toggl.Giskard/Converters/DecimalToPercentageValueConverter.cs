using System;
using System.Globalization;
using MvvmCross.Converters;

namespace Toggl.Giskard.Converters
{
    public sealed class DecimalToPercentageValueConverter : MvxValueConverter<float, int>
    {
        protected override int Convert(float value, Type targetType, object parameter, CultureInfo culture)
            => (int)(value * 100);

        protected override float ConvertBack(int value, Type targetType, object parameter, CultureInfo culture)
            => value / 100.0f;
    }
}
