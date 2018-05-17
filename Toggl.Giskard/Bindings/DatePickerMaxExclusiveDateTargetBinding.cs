using System;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;
using Android.Widget;

namespace Toggl.Giskard.Bindings
{
    public sealed class DatePickerMaxExclusiveDateTargetBinding : MvxAndroidTargetBinding<DatePicker, DateTimeOffset>
    {
        public const string BindingName = "MaxExclusiveDate";

        public DatePickerMaxExclusiveDateTargetBinding(DatePicker target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueImpl(DatePicker target, DateTimeOffset value)
        {
            var utcValue = value.ToUniversalTime();
            var upperBoundary = (DateTimeOffset)utcValue.Date;
            var nextYear = utcValue.AddYears(1);

            target.Post(() =>
            {
                // Workaround for a DatePicker bug in which
                // there's an early return if the year is the same
                // and the dates are different, which is bad logic.
                // https://stackoverflow.com/a/19722636/93770
                target.MaxDate = nextYear.ToUnixTimeMilliseconds();

                target.MaxDate = upperBoundary.ToUnixTimeMilliseconds();
            });
        }
    }
}
