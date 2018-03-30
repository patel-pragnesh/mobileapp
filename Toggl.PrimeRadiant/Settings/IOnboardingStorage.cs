using System;
using Toggl.PrimeRadiant.Onboarding;

namespace Toggl.PrimeRadiant.Settings
{
    public interface IOnboardingStorage
    {
        IObservable<bool> IsNewUser { get; }
        IObservable<bool> StartButtonWasTappedBefore { get; }

        void SetIsNewUser(bool isNewUser);
        void SetLastOpened(DateTimeOffset dateString);
        void SetCompletedOnboarding();

        string GetLastOpened();
        bool CompletedOnboarding();

        void StartButtonWasTapped();

        bool WasDismissed(IDismissable dismissable);
        void Dismiss(IDismissable dismissable);

        void Reset();
    }
}
