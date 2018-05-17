using System;
using Android.Graphics.Drawables;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;
using Toggl.Giskard.Extensions;
using MvvmCross.Plugins.Color.Droid;
using MvvmCross.Platform.UI;
using Android.Graphics;
using Android.Widget;

namespace Toggl.Giskard.Bindings
{
    public sealed class DatePickerMaxDateTargetBinding : MvxAndroidTargetBinding<DatePicker, DateTimeOffset>
    {
        public const string BindingName = "MaxDate";

        public DatePickerMaxDateTargetBinding(DatePicker target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueImpl(DatePicker target, DateTimeOffset value)
        {
            var utcValue = value.ToLocalTime();
            var upperBoundary = (DateTimeOffset)utcValue.Date.AddDays(1);
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
