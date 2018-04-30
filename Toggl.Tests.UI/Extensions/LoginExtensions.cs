using Xamarin.UITest;

namespace Toggl.Tests.UI.Extensions
{
    public static class LoginExtensions
    {
        public static void WaitForLoginScreen(this IApp app)
        {
            app.WaitForOnboardingScreen();
            #if __IOS__
            app.SkipToLastOnboardingPage();
            #endif
            app.OpenLoginFromOnboardingLastPage();
        }

        public static void WaitForSignUpScreen(this IApp app)
        {
            app.WaitForOnboardingScreen();
            #if __IOS__
            app.SkipToLastOnboardingPage();
            #endif
            app.OpenSignUpFromOnboardingLastPage();
        }

        public static void GoToPasswordScreen(this IApp app)
        {
            app.Tap(Login.NextButton);
            app.WaitForElement(Login.PasswordText);
        }

        public static void GoBackToOnboardingScreen(this IApp app)
        {
            app.PerformBack(Login.BackButton);
            app.WaitForElement(Onboarding.LoginButton);
        }

        public static void GoBackToEmailScreen(this IApp app)
        {
            app.PerformBack(Login.BackButton);
            app.WaitForElement(Login.EmailText);
        }

        public static void TryLoginAndFail(this IApp app)
        {
            app.Tap(Login.NextButton);

            app.WaitForElement(Login.ErrorLabel);
        }

        public static void LoginSuccesfully(this IApp app)
        {
            app.Tap(Login.NextButton);

            app.WaitForElement(Main.StartTimeEntryButton);
        }

        public static void TrySigningUpAndFail(this IApp app)
        {
            app.Tap(Login.NextButton);

            app.WaitForElement(Login.ErrorLabel);
        }

        public static void SignUpSuccesfully(this IApp app)
        {
            app.Tap(Login.NextButton);

            app.WaitForElement(Main.StartTimeEntryButton);
        }
    }
}
