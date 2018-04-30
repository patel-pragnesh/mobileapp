using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Onboarding;

namespace Toggl.Daneel.Onboarding.MainView
{
    public sealed class SwipeLeftOnboardingStep : IOnboardingStep
    {
        private const int minimumTimeEntriesCount = 5;

        public IObservable<bool> ShouldBeVisible { get; }

        public SwipeLeftOnboardingStep(
            IObservable<int> timeEntriesCountObservable,
            IObservable<bool> swipeRightOnboardingStepIsVisibleObservable)
        {
            Ensure.Argument.IsNotNull(timeEntriesCountObservable, nameof(timeEntriesCountObservable));
            Ensure.Argument.IsNotNull(
                swipeRightOnboardingStepIsVisibleObservable,
                nameof(swipeRightOnboardingStepIsVisibleObservable));

            ShouldBeVisible = Observable.CombineLatest(
                swipeRightOnboardingStepIsVisibleObservable,
                timeEntriesCountObservable,
                (swipeRightIsVisible, timeEntriesCount) => !swipeRightIsVisible
                && timeEntriesCount >= minimumTimeEntriesCount);
        }
    }
}
