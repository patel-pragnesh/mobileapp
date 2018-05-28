using System;
using System.Globalization;
using Android.Graphics;
using Android.Support.V4.Graphics;
using MvvmCross.Converters;
using Toggl.Multivac.Extensions;
using Int = Java.Lang.Integer;

namespace Toggl.Giskard.Converters
{
    public sealed class TransparentColorValueConverter : MvxValueConverter<string, Color>
    {
        private const int defaultOpacity = 255;

        protected override Color Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            var opacity = parameter is long opacityParameter
                ? (int)Math.Round(2.55 * opacityParameter.Clamp(0, 100))
                : defaultOpacity;

            var argb = ColorUtils.SetAlphaComponent(Color.ParseColor(value).ToArgb(), opacity);
            return new Color(argb);
        }
    }
}
