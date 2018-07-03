using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace Toggl.Giskard.Activities
{
    public partial class SettingsActivity
    {
        private View helpView;
        private View aboutView;
        private View logoutView;
        private View feedbackView;
        private View manualModeView;
        private View avatarContainer;
        private View beginningOfWeekView;

        private TextView nameTextView;
        private TextView emailTextView;
        private TextView versionTextView;
        private TextView beginningOfWeekTextView;

        private ImageView avatarView;

        private Switch manualModeSwitch;

        private RecyclerView workspacesRecyclerView;

        protected override void InitializeViews()
        {
            helpView = FindViewById(Resource.Id.SettingsHelpButton);
            aboutView = FindViewById(Resource.Id.SettingsAboutContainer);
            logoutView = FindViewById(Resource.Id.SettingsLogoutButton);
            feedbackView = FindViewById(Resource.Id.SettingsSubmitFeedbackButton);
            manualModeView = FindViewById(Resource.Id.SettingsToggleManualModeView);
            avatarContainer = FindViewById(Resource.Id.SettingsViewAvatarImageContainer);
            beginningOfWeekView = FindViewById(Resource.Id.SettingsSelectBeginningOfWeekView);

            nameTextView = FindViewById<TextView>(Resource.Id.SettingsNameTextView);
            emailTextView = FindViewById<TextView>(Resource.Id.SettingsEmailTextView);
            versionTextView = FindViewById<TextView>(Resource.Id.SettingsAppVersionTextView);
            beginningOfWeekTextView = FindViewById<TextView>(Resource.Id.SettingsBeginningOfWeekTextView);

            avatarView = FindViewById<ImageView>(Resource.Id.SettingsViewAvatarImage);
            manualModeSwitch = FindViewById<Switch>(Resource.Id.SettingsIsManualModeEnabledSwitch);

            workspacesRecyclerView = FindViewById<RecyclerView>(Resource.Id.SettingsWorkspacesRecyclerView);
        }
    }
}
