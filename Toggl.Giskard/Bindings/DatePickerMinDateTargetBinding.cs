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
    public sealed class DatePickerMinDateTargetBinding : MvxAndroidTargetBinding<DatePicker, DateTimeOffset>
    {
        public const string BindingName = "MinDate";

        public DatePickerMinDateTargetBinding(DatePicker target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueImpl(DatePicker target, DateTimeOffset value)
        {
            var utcValue = value.ToUniversalTime();
            var lowerBoundary = (DateTimeOffset)utcValue.Date;

            // Workaround for a DatePicker bug in which
            // there's an early return if the year is the same
            // and the dates are different, which is bad logic.
            // https://stackoverflow.com/a/19722636/93770
            target.MinDate = 0;

            target.MinDate = lowerBoundary.ToUnixTimeMilliseconds();
        }
    }
}
