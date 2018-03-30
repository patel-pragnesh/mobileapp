using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class ImageViewAnimatedImageTargetBinding : MvxTargetBinding<UIImageView, UIImage>
    {
        public const string BindingName = "AnimatedImage";

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        public ImageViewAnimatedImageTargetBinding(UIImageView target)
            : base(target)
        {
        }

        protected override void SetValue(UIImage value)
        {
            UIView.Transition(
                Target,
                Animation.Timings.EnterTiming,
                UIViewAnimationOptions.TransitionCrossDissolve,
                () => Target.Image = value,
                null);
        }
    }
}
