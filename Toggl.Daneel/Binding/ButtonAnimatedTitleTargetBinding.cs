using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class ButtonAnimatedTitleTargetBinding
        : MvxTargetBinding<UIButton, string>
    {
        public const string BindingName = "AnimatedTitle";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public ButtonAnimatedTitleTargetBinding(UIButton target) : base(target)
        {
        }

        protected override void SetValue(string value)
        {
            UIView.Transition(
                Target,
                Animation.Timings.EnterTiming,
                UIViewAnimationOptions.TransitionCrossDissolve,
                () => Target.SetTitle(value, UIControlState.Normal),
                null
            );
        }
    }
}
