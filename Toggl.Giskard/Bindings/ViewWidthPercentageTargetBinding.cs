using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Toggl.Giskard.Bindings
{
    public sealed class ViewWidthPercentageTargetBinding : MvxAndroidTargetBinding<View, float?>
    {
        public const string BindingName = "WidthPercentage";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public ViewWidthPercentageTargetBinding(View target)
                : base(target)
        {
        }

        protected override void SetValueImpl(View target, float? value)
        {
            Target.Post(() =>
            {
                var percentage = value ?? 0;

                var availableWidth = (Target.Parent as View).Width;
                var targetWidth = (availableWidth / 100.0f) * percentage;

                var layoutParams = Target.LayoutParameters;
                layoutParams.Width = (int)targetWidth;
                Target.LayoutParameters = layoutParams;
            });
        }
    }
}
