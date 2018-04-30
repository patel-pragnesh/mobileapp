using System;
using Xamarin.UITest.Queries;

namespace Toggl.Tests.UI
{
    public static class Onboarding
    {
        public const string FirstOnboardingElement = SkipButton;
        public const string SkipButton = "OnboardingSkip";
        public const string NextButton = "OnboardingNext";
        public const string LoginButton = "OnboardingLogin";
        public const string SignUpButton = "OnboardingSignUp";
        public const string FirstLabel = "OnboardingFirstLabel";
        public const string SecondLabel = "OnboardingSecondLabel";
        public const string ThirdLabel = "OnboardingThirdLabel";
        public const string PreviousButton = "OnboardingPrevious";
    }

    public static class Login
    {
        public const string EmailText = "LoginEmail";
        public const string ErrorLabel = "LoginError";
        public const string PasswordText = "LoginPassword";
        public const string ShowPasswordButton = "LoginShowPassword";
        public const string ForgotPasswordButton = "LoginForgotPassword";
        public static readonly Func<AppQuery, AppQuery> BackButton = x => x.Text("Back");
        public static readonly Func<AppQuery, AppQuery> NextButton = x => x.Text("Next");
    }

    public static class Main
    {
        public const string StartTimeEntryButton = "MainStartTimeEntry";
        public const string StopTimeEntryButton = "MainStopTimeEntry";
    }

    public static class StartTimeEntry
    {
        public const string DoneButton = "StartTimeEntryDone";
        public const string DescriptionText = "StartTimeEntryDescription";
    }
}
