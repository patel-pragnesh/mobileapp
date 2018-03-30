using System;
namespace Toggl.PrimeRadiant.Onboarding
{
    public interface IDismissable
    {
        string Key { get; }
        void Dismiss();
    }
}
