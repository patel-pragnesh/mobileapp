using System;
using System.Globalization;
using MvvmCross.Converters;
using Toggl.Multivac;
using static System.Math;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public sealed class ComplementValueConverter : MvxValueConverter<float, float>
    {
        protected override float Convert(float value, Type targetType, object parameter, CultureInfo culture)
            => Abs(value - 1);

        protected override float ConvertBack(float value, Type targetType, object parameter, CultureInfo culture)
            => Abs(value - 1);
    }
}
