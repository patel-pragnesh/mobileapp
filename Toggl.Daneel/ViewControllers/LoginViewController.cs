﻿using System;
using System.Linq;
using System.Reactive.Linq;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac;
using UIKit;
using static Toggl.Daneel.Extensions.LoginSignupViewExtensions;
using static Toggl.Daneel.Extensions.UIExtensions;
using static Toggl.Daneel.Extensions.ViewExtensions;

namespace Toggl.Daneel.ViewControllers
{
    [MvxRootPresentation(WrapInNavigationController = true)]
    [MvxFromStoryboard("Login")]
    public sealed partial class LoginViewController : ReactiveViewController<LoginViewModel>
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
            PasswordManagerButton.Hidden = !ViewModel.IsPasswordManagerAvailable;

            //Text
            this.Bind(ViewModel.Email, EmailTextField.BindText());
            this.Bind(ViewModel.ErrorMessage, ErrorLabel.BindText());
            this.Bind(ViewModel.Password, PasswordTextField.BindText());
            this.Bind(EmailTextField.Text().Select(Email.From), ViewModel.SetEmail);
            this.Bind(PasswordTextField.Text().Select(Password.From), ViewModel.SetPassword);
            this.Bind(ViewModel.IsLoading.Select(loginButtonTitle), LoginButton.BindAnimatedTitle());

            //Visibility
            this.Bind(ViewModel.HasError, ErrorLabel.BindAnimatedIsVisible());
            this.Bind(ViewModel.IsLoading, ActivityIndicator.BindIsVisibleWithFade());
            this.Bind(ViewModel.IsPasswordMasked, PasswordTextField.BindSecureTextEntry());
            this.Bind(ViewModel.IsShowPasswordButtonVisible, ShowPasswordButton.BindIsVisible());
            this.Bind(PasswordTextField.FirstResponder, ViewModel.SetIsShowPasswordButtonVisible);

            //Commands
            this.Bind(SignupCard.Tapped(), ViewModel.Signup);
            this.BindVoid(LoginButton.Tapped(), ViewModel.Login);
            this.BindVoid(GoogleLoginButton.Tapped(), ViewModel.GoogleLogin);
            this.Bind(ForgotPasswordButton.Tapped(), ViewModel.ForgotPassword);
            this.Bind(PasswordManagerButton.Tapped(), ViewModel.StartPasswordManager);
            this.BindVoid(ShowPasswordButton.Tapped(), ViewModel.TogglePasswordVisibility);

            //Color
            this.Bind(ViewModel.HasError.Select(loginButtonTintColor), LoginButton.BindTintColor());
            this.Bind(ViewModel.LoginEnabled.Select(loginButtonTitleColor),LoginButton.BindTitleColor());

            prepareViews();

            UIColor loginButtonTintColor(bool hasError)
                => hasError ? UIColor.White : UIColor.Black;
            
            UIColor loginButtonTitleColor(bool enabled) => enabled 
                ? Color.Login.EnabledButtonColor.ToNativeColor()
                : Color.Login.DisabledButtonColor.ToNativeColor();
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

        public void ShakeEmailField() => EmailTextField.Shake();

        public void ShakePasswordField() => PasswordTextField.Shake();

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
                ViewModel.Login();
                PasswordTextField.ResignFirstResponder();
                return false;
            };

            View.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                EmailTextField.ResignFirstResponder();
                PasswordTextField.ResignFirstResponder();
            }));

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

        private string loginButtonTitle(bool isLoading)
            => isLoading ? "" : Resources.LoginTitle;

        private UIColor loginButtonTintColor(bool hasError)
            => hasError ? UIColor.White : UIColor.Black;
    }
}

