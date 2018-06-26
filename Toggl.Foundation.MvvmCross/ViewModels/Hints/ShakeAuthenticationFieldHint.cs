using MvvmCross.Core.ViewModels;

namespace Toggl.Foundation.MvvmCross.ViewModels.Hints
{
    public sealed class ShakeAuthenticationFieldHint : MvxPresentationHint
    {
        public bool ShakeEmailField { get; }

        public bool ShakePasswordField { get; }

        public ShakeAuthenticationFieldHint(bool shakeEmailField, bool shakePasswordField)
        {
            ShakeEmailField = shakeEmailField;
            ShakePasswordField = shakePasswordField;
        }
    }
}
