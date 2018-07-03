using Android.Views;

namespace Toggl.Giskard.Activities
{
    public partial class AboutActivity
    {
        private View licensesButton;
        private View privacyPolicyButton;
        private View termsOfServiceButton;

        protected override void InitializeViews()
        {
            licensesButton = FindViewById(Resource.Id.AboutLicensesButton);
            privacyPolicyButton = FindViewById(Resource.Id.AboutPrivacyPolicyButton);
            termsOfServiceButton = FindViewById(Resource.Id.AboutTermsOfServiceButton);
        }
    }
}
