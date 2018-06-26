using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using MvvmCross.Core.ViewModels;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class ViewLongPressCommandTargetBinding : MvxTargetBinding<UIView, IMvxCommand>
    {
        private UILongPressGestureRecognizer gestureRecognizer;
        private readonly UIImpactFeedbackGenerator feedbackGenerator
            = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium);

        public const string BindingName = "LongPress";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public ViewLongPressCommandTargetBinding(UIView target) : base(target)
        {
            feedbackGenerator.Prepare();
        }

        protected override void SetValue(IMvxCommand value)
        {
            if (gestureRecognizer != null)
                Target.RemoveGestureRecognizer(gestureRecognizer);

            gestureRecognizer = new UILongPressGestureRecognizer(() =>
            {
                if (gestureRecognizer.State == UIGestureRecognizerState.Began)
                {
                    value.Execute();
                    feedbackGenerator.ImpactOccurred();
                }
            });
            Target.AddGestureRecognizer(gestureRecognizer);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (!isDisposing) return;
            if (gestureRecognizer == null) return;

            Target.RemoveGestureRecognizer(gestureRecognizer);
        }
    }
}
