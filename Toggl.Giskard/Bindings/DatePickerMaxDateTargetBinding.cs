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
            target.MaxDate = value.ToUnixTimeMilliseconds();
        }
    }
}
