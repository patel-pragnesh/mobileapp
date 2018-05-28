using System;
using System.Globalization;
using MvvmCross.Converters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    public class BoolToConstantValueConverter<T> : MvxValueConverter<bool, T>
    {
        private readonly T onValue;
        private readonly T offValue;

        public BoolToConstantValueConverter(T onValue, T offValue)
        {
            Ensure.Argument.IsNotNull(onValue, nameof(onValue));
            Ensure.Argument.IsNotNull(offValue, nameof(offValue));

            this.onValue = onValue;
            this.offValue = offValue;
        }

        protected override T Convert(bool value, Type targetType, object parameter, CultureInfo culture)
            => value ? onValue : offValue;
    }
}
