using Toggl.PrimeRadiant.Onboarding;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.PrimeRadiant.Extensions
{
    public static class OnboardingStepExtensions
    {
        public static DismissableOnboardingStep ToDismissable(this IOnboardingStep step, string key, IOnboardingStorage onboardingStorage)
            => new DismissableOnboardingStep(step, key, onboardingStorage);
    }
}
