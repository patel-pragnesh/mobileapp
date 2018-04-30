using System;
using Foundation;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using UIKit;
using Toggl.Daneel.Extensions;

namespace Toggl.Daneel.Binding
{
    public sealed class DatePickerDateTimeOffsetTargetBinding : MvxTargetBinding<UIDatePicker, DateTimeOffset>
    {
        public const string BindingName = "DateTimeOffset";
        
        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;
        
        public DatePickerDateTimeOffsetTargetBinding(UIDatePicker target) : base(target)
        {
            Target.ValueChanged += onValueChanged;
        }

        protected override void SetValue(DateTimeOffset value)
        {
            Target.SetDate(value.ToNSDate(), true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            
            if (!isDisposing) return;
            Target.ValueChanged -= onValueChanged;
        }

        private void onValueChanged(object sender, EventArgs e)
            => FireValueChanged(Target.Date.ToDateTimeOffset());
    }
}
