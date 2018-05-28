using Android.Support.Design.Widget;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android.Binding.Target;

namespace Toggl.Giskard.Bindings
{
    public sealed class FabVisibilityTargetBinding : MvxAndroidTargetBinding<FloatingActionButton, bool>
    {
        public const string BindingName = "FabVisibility";

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        public FabVisibilityTargetBinding(FloatingActionButton target)
            : base(target)
        {
        }

        protected override void SetValueImpl(FloatingActionButton target, bool value)
        {
            if (value)
            {
                target.Show();
            }
            else
            {
                target.Hide();
            }
        }
    }
}
