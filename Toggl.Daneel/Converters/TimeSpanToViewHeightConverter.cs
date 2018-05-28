using System;
using System.Globalization;
using MvvmCross.Converters;

namespace Toggl.Foundation.MvvmCross.Converters
{
    public sealed class TimeSpanToViewHeightConverter : MvxValueConverter<TimeSpan, nfloat>
    {
        private const int rounding = 5 * 60; //Round to 5min intervals

        private readonly TimeSpan minDuration = TimeSpan.Zero;
        private readonly TimeSpan MaxDuration = TimeSpan.FromHours(12);

        private readonly nfloat minViewHeight;
        private readonly nfloat maxViewHeight;

        private readonly nfloat functionSlope;

        public TimeSpanToViewHeightConverter(nfloat minViewHeight, nfloat maxViewHeight)
        {
            this.minViewHeight = minViewHeight;
            this.maxViewHeight = maxViewHeight;

            functionSlope = (nfloat)((maxViewHeight - minViewHeight) / (MaxDuration.TotalSeconds - minDuration.TotalSeconds));
        }
        
        protected override nfloat Convert(TimeSpan value, Type targetType, object parameter, CultureInfo culture)
        {
            var returnValue = functionSlope * value.TotalSeconds + minViewHeight;
            return (nfloat)returnValue;
        }

        protected override TimeSpan ConvertBack(nfloat value, Type targetType, object parameter, CultureInfo culture)
        {
            var seconds = (value - minViewHeight) / functionSlope;
            seconds = (nfloat)Math.Round(seconds / rounding) * rounding;
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
