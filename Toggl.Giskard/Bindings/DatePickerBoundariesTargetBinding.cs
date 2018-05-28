using MvvmCross.Binding;
using MvvmCross.Platforms.Android.Binding.Target;
using Toggl.Multivac;
using Toggl.Giskard.Views;

namespace Toggl.Giskard.Bindings
{
    public sealed class DatePickerBoundariesTargetBinding : MvxAndroidTargetBinding<TogglDroidDatePicker, DateTimeOffsetRange>
    {
        public const string BindingName = "Boundaries";

        public DatePickerBoundariesTargetBinding(TogglDroidDatePicker target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueImpl(TogglDroidDatePicker target, DateTimeOffsetRange value)
        {
            target.Post(() => target.Boundaries = value);
        }
    }
}
