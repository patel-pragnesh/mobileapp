using System;

namespace Toggl.PrimeRadiant.Onboarding
{
    public interface IOnboardingStep
    {
        IObservable<bool> ShouldBeVisible { get; }
    }
}
