﻿using Foundation;
using MvvmCross.Binding;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Extensions;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using System;
using Toggl.Daneel.Views;
using Toggl.Multivac;

namespace Toggl.Daneel.ViewControllers
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    [MvxFromStoryboard("Login")]
    public sealed partial class LoginViewController : MvxViewController<LoginViewModel>
    {
        private const int iPhoneSeScreenHeight = 568;
        private const int topConstraintForBiggerScreens = 92;

        public LoginViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.NavigationBarHidden = true;

            var loginButtonColorConverter = new BoolToConstantValueConverter<UIColor>(UIColor.White, UIColor.Black);
            var loginButtonTitleConverter = new BoolToConstantValueConverter<string>("", Resources.LoginTitle);

            var bindingSet = this.CreateBindingSet<LoginViewController, LoginViewModel>();

            //Text
            bindingSet.Bind(ErrorLabel).To(vm => vm.ErrorMessage);
            bindingSet.Bind(EmailTextField)
                      .To(vm => vm.Email)
                      .WithConversion(new EmailToStringValueConverter());

            bindingSet.Bind(PasswordTextField)
                      .To(vm => vm.Password)
                      .WithConversion(new PasswordToStringValueConverter());

            bindingSet.Bind(LoginButton)
                      .For(v => v.BindAnimatedTitle())
                      .To(vm => vm.IsLoading)
                      .WithConversion(loginButtonTitleConverter);

            //Commands
            bindingSet.Bind(LoginButton).To(vm => vm.LoginCommand);
            bindingSet.Bind(GoogleLoginButton).To(vm => vm.GoogleLoginCommand);
            bindingSet.Bind(ForgotPasswordButton).To(vm => vm.ForgotPasswordCommand);
            bindingSet.Bind(PasswordManagerButton).To(vm => vm.StartPasswordManagerCommand);
            bindingSet.Bind(ShowPasswordButton).To(vm => vm.TogglePasswordVisibilityCommand);

            bindingSet.Bind(SignupCard)
                      .For(v => v.BindTap())
                      .To(vm => vm.SignupCommand);

            //Visibilty
            bindingSet.Bind(ErrorLabel)
                      .For(v => v.BindAnimatedVisibility())
                      .To(vm => vm.HasError);

            bindingSet.Bind(ActivityIndicator)
                     .For(v => v.BindVisibilityWithFade())
                     .To(vm => vm.IsLoading);

            bindingSet.Bind(PasswordManagerButton)
                      .For(v => v.BindVisible())
                      .To(vm => vm.IsPasswordManagerAvailable);

            bindingSet.Bind(PasswordTextField)
                      .For(v => v.BindSecureTextEntry())
                      .To(vm => vm.IsPasswordMasked);

            bindingSet.Bind(ShowPasswordButton)
                      .For(v => v.BindVisible())
                      .To(vm => vm.IsShowPasswordButtonVisible);

            bindingSet.Bind(PasswordTextField)
                      .For(v => v.BindFirstResponder())
                      .To(vm => vm.IsShowPasswordButtonVisible)
                      .Mode(MvxBindingMode.OneWayToSource);
            //Color
            bindingSet.Bind(LoginButton)
                      .For(v => v.TintColor)
                      .To(vm => vm.HasError)
                      .WithConversion(loginButtonColorConverter);

            bindingSet.Apply();

            prepareViews();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (View.Frame.Height > iPhoneSeScreenHeight)
                TopConstraint.Constant = topConstraintForBiggerScreens;

            SignupCard.SetupBottomCard();
            GoogleLoginButton.SetupGoogleButton();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            ActivityIndicator.Alpha = 0;
            ActivityIndicator.StartAnimation();
        }

        private void prepareViews()
        {
            NavigationController.NavigationBarHidden = true;

            LoginButton.SetTitleColor(
                Color.Login.DisabledButtonColor.ToNativeColor(),
                UIControlState.Disabled
            );

            EmailTextField.ShouldReturn += _ => {
                PasswordTextField.BecomeFirstResponder();
                return false;
            };

            PasswordTextField.ShouldReturn += _ =>
            {
                ViewModel.LoginCommand.Execute();
                PasswordTextField.ResignFirstResponder();
                return false;
            };

            View.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                EmailTextField.ResignFirstResponder();
                PasswordTextField.ResignFirstResponder();
            }));

            LoginShakeTriggerButton.TouchUpInside += (sender, e) =>
            {
                if (!ViewModel.Email.IsValid)
                {
                    EmailTextField.Shake();
                }
                if (!ViewModel.Password.IsValid)
                {
                    PasswordTextField.Shake();
                }
            };

            PasswordTextField.ResignFirstResponder();

            ShowPasswordButton.SetupShowPasswordButton();
            prepareForgotPasswordButton();
        }

        private void prepareForgotPasswordButton()
        {
            var boldFont = UIFont.SystemFontOfSize(12, UIFontWeight.Medium);
            var color = Color.Login.ForgotPassword.ToNativeColor();
            var text = new NSMutableAttributedString(
                Resources.LoginForgotPassword, foregroundColor: color);
            var boldText = new NSAttributedString(
                Resources.LoginGetHelpLoggingIn,
                foregroundColor: color,
                font: boldFont);
            text.Append(boldText);
            ForgotPasswordButton.SetAttributedTitle(text, UIControlState.Normal);
        }
    }
}

