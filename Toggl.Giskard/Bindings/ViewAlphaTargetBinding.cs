using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android.Binding.Target;

namespace Toggl.Giskard.Bindings
{
    public sealed class ViewAlphaTargetBinding : MvxAndroidTargetBinding<View, float>
    {
        public const string BindingName = "Alpha";

        public ViewAlphaTargetBinding(View target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueImpl(View target, float value)
        {
            target.Alpha = value;
        }
    }
}
