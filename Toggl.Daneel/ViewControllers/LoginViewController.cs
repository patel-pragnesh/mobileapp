using CoreGraphics;
using Foundation;
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

namespace Toggl.Daneel.ViewControllers
{
    [MvxRootPresentation]
    public sealed partial class LoginViewController : MvxViewController<NewLoginViewModel>
    {
        private const int iPhoneSeScreenHeight = 568;

        public LoginViewController() 
            : base(nameof(LoginViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var loginButtonColorConverter = new BoolToConstantValueConverter<UIColor>(UIColor.White, UIColor.Black);
            var loginButtonTitleConverter = new BoolToConstantValueConverter<string>("", Resources.LoginTitle);
            var invertedBoolConverter = new BoolToConstantValueConverter<bool>(false, true);

            var bindingSet = this.CreateBindingSet<LoginViewController, NewLoginViewModel>();

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
                TopConstraint.Constant = 132;

            prepareSignupCard();
            prepareGoogleLoginButton();
        }

        private void prepareViews()
        {
            ActivityIndicator.StartAnimation();

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
                PasswordTextField.ResignFirstResponder();
                return false;
            };

            PasswordTextField.ResignFirstResponder();

            prepareShowPasswordButton();
            prepareForgotPasswordButton();
        }

        private void prepareGoogleLoginButton()
        {
            var layer = GoogleLoginButton.Layer;
            var shadowPath = UIBezierPath.FromRect(GoogleLoginButton.Bounds);
            layer.MasksToBounds = false;
            layer.ShadowColor = UIColor.Black.CGColor;
            layer.ShadowOffset = new CGSize(0, 1);
            layer.ShadowOpacity = 0.24f;
            layer.ShadowPath = shadowPath.CGPath;
            layer.ShadowRadius = 1;

            //Add spacing between button title ang google logo
            var spacing = 17;
            GoogleLoginButton.ImageEdgeInsets = new UIEdgeInsets(0, 0, 0, spacing);
            GoogleLoginButton.TitleEdgeInsets = new UIEdgeInsets(0, spacing, 0, 0);
        }

        private void prepareSignupCard()
        {
            var layer = SignupCard.Layer;
            var shadowPath = UIBezierPath.FromRect(SignupCard.Bounds);
            layer.MasksToBounds = false;
            layer.ShadowColor = UIColor.Black.CGColor;
            layer.ShadowOffset = new CGSize(0, -2);
            layer.ShadowOpacity = 0.1f;
            layer.ShadowPath = shadowPath.CGPath;
            layer.ShadowRadius = 16;
        }

        private void prepareForgotPasswordButton()
        {
            var normalFont = UIFont.SystemFontOfSize(12, UIFontWeight.Regular);
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

        private void prepareShowPasswordButton()
        {
            var image = UIImage
                .FromBundle("icPassword")
                .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            ShowPasswordButton.SetImage(image, UIControlState.Normal);
            ShowPasswordButton.TintColor = UIColor.Black;
        }
    }
}

