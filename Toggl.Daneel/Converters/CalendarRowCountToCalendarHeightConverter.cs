using System;
using System.Globalization;
using MvvmCross.Converters;

namespace Toggl.Daneel.Converters
{
    public sealed class CalendarRowCountToCalendarHeightConverter
        : MvxValueConverter<int, nfloat>
    {
        private readonly nfloat rowHeight;
        private readonly nfloat additionalHeight;

        public CalendarRowCountToCalendarHeightConverter(
            nfloat rowHeight, nfloat additionalHeight)
        {
            this.rowHeight = rowHeight;
            this.additionalHeight = additionalHeight;
        }

        protected override nfloat Convert(int value, Type targetType, object parameter, CultureInfo culture)
            => value * rowHeight + additionalHeight;
    }
}
