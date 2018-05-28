using System;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android.Binding.Target;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Bindings
{
    public sealed class ViewMarginTargetBinding : MvxAndroidTargetBinding<View, int?>
    {
        public const string BindingName = "Margin";

        private readonly BoundMargin boundMargin;

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public ViewMarginTargetBinding(View target, BoundMargin boundMargin = BoundMargin.All)
                : base(target)
        {
            this.boundMargin = boundMargin;
        }

        protected override void SetValueImpl(View target, int? value)
        {
            if (value == null) return;

            Target.Post(() =>
            {
                var marginParams = Target.LayoutParameters as ViewGroup.MarginLayoutParams;
                if (marginParams == null) return;

                var pixels = value.Value.DpToPixels(AndroidGlobals.ApplicationContext);

                var top = boundMargin.HasFlag(BoundMargin.Top) ? (int?)pixels : null;
                var left = boundMargin.HasFlag(BoundMargin.Left) ? (int?)pixels : null;
                var right = boundMargin.HasFlag(BoundMargin.Right) ? (int?)pixels : null;
                var bottom = boundMargin.HasFlag(BoundMargin.Bottom) ? (int?)pixels : null;

                Target.LayoutParameters = marginParams.WithMargins(left, top, right, bottom);
            });
        }

        [Flags]
        public enum BoundMargin
        {
            Top = 1,
            Left = 2,
            Right = 4,
            Bottom = 8,
            All = Top | Left | Right | Bottom
        }
    }
}
