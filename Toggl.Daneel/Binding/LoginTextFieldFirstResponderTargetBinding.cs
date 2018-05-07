using System;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Daneel.Views;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class LoginTextFieldFirstResponderTargetBinding : MvxTargetBinding<LoginTextField, bool>
    {
        public const string BindingName = "FirstResponder";

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        public LoginTextFieldFirstResponderTargetBinding(LoginTextField target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            Target.IsFirstResponderChanged += onFirstResponderChanged;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!isDisposing) return;

            Target.IsFirstResponderChanged -= onFirstResponderChanged;
        }

        protected override void SetValue(bool value)
        {
            if (value)
                Target.BecomeFirstResponder();
            else
                Target.ResignFirstResponder();
        }

        private void onFirstResponderChanged(object sender, EventArgs args)
        {
            FireValueChanged(Target.IsFirstResponder);
        }
    }
}
