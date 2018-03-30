using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.PrimeRadiant.Onboarding
{
    public sealed class DismissableOnboardingStep : IDismissable, IOnboardingStep
    {
        private readonly ISubject<bool> shouldBeVisibleSubject;
        private readonly IOnboardingStorage onboardingStorage;

        private IDisposable shouldBeVisibleSubscription;

        public IObservable<bool> ShouldBeVisible { get; }

        public string Key { get; }

        public DismissableOnboardingStep(IOnboardingStep onboardingStep, string key, IOnboardingStorage onboardingStorage)
        {
            Ensure.Argument.IsNotNull(onboardingStep, nameof(onboardingStep));
            Ensure.Argument.IsNotNullOrWhiteSpaceString(key, nameof(key));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));

            this.onboardingStorage = onboardingStorage;

            Key = key;

            var wasDismissed = onboardingStorage.WasDismissed(this);
            shouldBeVisibleSubject = new BehaviorSubject<bool>(!wasDismissed);
            shouldBeVisibleSubscription = onboardingStep.ShouldBeVisible.Subscribe(shouldBeVisibleSubject.OnNext);

            ShouldBeVisible = shouldBeVisibleSubject.AsObservable();
        }

        public void Dismiss()
        {
            shouldBeVisibleSubscription?.Dispose();
            shouldBeVisibleSubscription = null;

            onboardingStorage.Dismiss(this);
            shouldBeVisibleSubject.OnNext(false);
        }
    }
}
