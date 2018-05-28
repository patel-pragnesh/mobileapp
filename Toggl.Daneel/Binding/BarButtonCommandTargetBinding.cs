using System;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class BarButtonCommandTargetBinding : MvxTargetBinding<UIBarButtonItem, IMvxCommand>
    {
        public const string BindingName = "Command";

        private IMvxCommand command;

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public BarButtonCommandTargetBinding(UIBarButtonItem target)
            : base(target) 
        {
            Target.Clicked += onClicked;
        }

        protected override void SetValue(IMvxCommand value)
            => command = value;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!isDisposing) return;
            Target.Clicked -= onClicked;
        }

        private void onClicked(object sender, EventArgs e)
            => command.Execute();
    }
}
