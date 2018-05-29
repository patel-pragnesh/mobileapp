﻿using System;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Platforms.Android.Binding.Target;
using Toggl.Giskard.Extensions;
using MvvmCross.WeakSubscription;
using static Android.Views.View;

namespace Toggl.Giskard.Bindings
{
    public sealed class EditTextFocusTargetBinding : MvxAndroidTargetBinding<EditText, bool>
    {
        public const string BindingName = "Focus";

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        private IDisposable subscription;

        public EditTextFocusTargetBinding(EditText target) : base(target)
        {
            subscription = target.WeakSubscribe<EditText, FocusChangeEventArgs>(nameof(target.FocusChange), onIsFocusedChanged);
        }

        private void onIsFocusedChanged(object sender, FocusChangeEventArgs args)
        {
            FireValueChanged(args.HasFocus);
        }

        protected override void SetValueImpl(EditText target, bool value)
        {
            if (value) target.SetFocus();
            else target.RemoveFocus();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!isDisposing) return;

            subscription?.Dispose();
        }
    }
}
