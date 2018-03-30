using System;
using Android.Graphics.Drawables;
using Android.Views;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;
using Toggl.Giskard.Extensions;
using MvvmCross.Plugins.Color.Droid;
using MvvmCross.Platform.UI;
using Android.Graphics;

namespace Toggl.Giskard.Bindings
{
    public sealed class DrawableColorTargetBinding : MvxAndroidTargetBinding<View, Color>
    {
        public const string BindingName = "DrawableColor";

        public DrawableColorTargetBinding(View target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueImpl(View target, Color value)
        {
            if (target?.Background is GradientDrawable drawable)
            {
                drawable.SetColor(value.ToArgb());
                drawable.InvalidateSelf();
            }
        }
    }
}
