using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Onboarding;
using UIKit;

namespace Toggl.Daneel.Onboarding.MainView
{
    public sealed class SwipeRightOnboardingStep : IOnboardingStep
    {
        private const int minimumTimeEntriesCount = 5;

        public IObservable<bool> ShouldBeVisible { get; }

        public SwipeRightOnboardingStep(IObservable<int> timeEntriesCountObservable)
        {
            Ensure.Argument.IsNotNull(timeEntriesCountObservable, nameof(timeEntriesCountObservable));

            ShouldBeVisible = UIDevice.CurrentDevice.CheckSystemVersion(11, 0)
                ? timeEntriesCountObservable.Select(timeEntriesCount => timeEntriesCount >= minimumTimeEntriesCount)
                : Observable.Return(false);
        }
    }
}
