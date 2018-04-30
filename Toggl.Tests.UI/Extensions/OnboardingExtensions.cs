using Xamarin.UITest;

namespace Toggl.Tests.UI.Extensions
{
    public static class OnboardingExtensions
    {
        public static void WaitForOnboardingScreen(this IApp app)
            => app.WaitForElement(Onboarding.FirstOnboardingElement);

        public static void GoForwardToSecondOnboardingPage(this IApp app)
        {
            app.Tap(Onboarding.NextButton);
            app.WaitForElement(Onboarding.SecondLabel);
        }

        public static void GoForwardToThirdOnboardingPage(this IApp app)
        {
            app.Tap(Onboarding.NextButton);
            app.WaitForElement(Onboarding.ThirdLabel);
        }

        public static void GoForwardToFourthOnboardingPage(this IApp app)
        {
            app.Tap(Onboarding.NextButton);
            app.WaitForElement(Onboarding.LoginButton);
        }

        public static void SwipeForwardToSecondOnboardingPage(this IApp app)
        {
            app.SwipeRightToLeft(Onboarding.FirstLabel);
            app.WaitForElement(Onboarding.SecondLabel);
        }

        public static void SwipeForwardToThirdOnboardingPage(this IApp app)
        {
            app.SwipeRightToLeft(Onboarding.SecondLabel);
            app.WaitForElement(Onboarding.ThirdLabel);
        }

        public static void SwipeForwardToFourthOnboardingPage(this IApp app)
        {
            app.SwipeRightToLeft(Onboarding.ThirdLabel);
            app.WaitForElement(Onboarding.LoginButton);
        }

        public static void GoBackToFirstOnboardingPage(this IApp app)
        {
            app.Tap(Onboarding.PreviousButton);
            app.WaitForElement(Onboarding.FirstLabel);
        }

        public static void GoBackToSecondOnboardingPage(this IApp app)
        {
            app.Tap(Onboarding.PreviousButton);
            app.WaitForElement(Onboarding.SecondLabel);
        }

        public static void GoBackToThirdOnboardingPage(this IApp app)
        {
            app.Tap(Onboarding.PreviousButton);
            app.WaitForElement(Onboarding.ThirdLabel);
        }

        public static void SwipeBackToFirstOnboardingPage(this IApp app)
        {
            app.SwipeLeftToRight(Onboarding.SecondLabel);
            app.WaitForElement(Onboarding.FirstLabel);
        }

        public static void SwipeBackToSecondOnboardingPage(this IApp app)
        {
            app.SwipeLeftToRight(Onboarding.ThirdLabel);
            app.WaitForElement(Onboarding.SecondLabel);
        }

        public static void SkipToLastOnboardingPage(this IApp app)
        {
            app.Tap(Onboarding.SkipButton);
            app.WaitForElement(Onboarding.LoginButton);
        }

        public static void OpenLoginFromOnboardingLastPage(this IApp app)
        {
            app.Tap(Onboarding.LoginButton);
            app.WaitForElement(Login.EmailText);
        }

        public static void OpenSignUpFromOnboardingLastPage(this IApp app)
        {
            app.Tap(Onboarding.SignUpButton);
            app.WaitForElement(Login.EmailText);
        }
    }
}
