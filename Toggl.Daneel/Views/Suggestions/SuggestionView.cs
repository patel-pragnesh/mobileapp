using Foundation;
using System;
using UIKit;
using ObjCRuntime;
using CoreGraphics;
using Toggl.Foundation.Suggestions;
using MvvmCross.Plugin.Color.Platforms.Ios;
using MvvmCross.UI;

namespace Toggl.Daneel
{
    public sealed partial class SuggestionView : UIView
    {
        private const float noProjectDistance = 11;
        private const float hasProjectDistance = 0;

        public SuggestionView (IntPtr handle) : base (handle)
        {
        }

        public static SuggestionView Create()
        {
            var arr = NSBundle.MainBundle.LoadNib(nameof(SuggestionView), null, null);
            return Runtime.GetNSObject<SuggestionView>(arr.ValueAt(0));
        }

        private Suggestion suggestion;
        public Suggestion Suggestion
        {
            get => suggestion;
            set
            {
                if (suggestion == value) return;
                suggestion = value;
                onSuggestionChanged();
            }
        }

        private void onSuggestionChanged()
        {
            if (Suggestion == null) return;

            Hidden = false;
            DescriptionLabel.Text = Suggestion.Description;

            var hasProject = Suggestion.ProjectId != null;
            DescriptionTopDistanceConstraint.Constant = hasProject ? hasProjectDistance : noProjectDistance;

            if (!hasProject)
            {
                hideProjectTaskClient();
                return;
            }

            var projectColor = MvxColor
                .ParseHexString(Suggestion.ProjectColor)
                .ToNativeColor();
            ProjectDot.TintColor = projectColor;
            ProjectLabel.TextColor = projectColor;

            ClientLabel.Text = Suggestion.ClientName;
            ProjectLabel.Text = Suggestion.TaskId == null
                ? Suggestion.ProjectName
                : $"{Suggestion.ProjectName}: {Suggestion.TaskName}";
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            ProjectDot.Image = ProjectDot
                .Image
                .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

            FadeView.FadeRight = true;

            if (Suggestion == null)
                Hidden = true;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var shadowPath = UIBezierPath.FromRect(Bounds);
            Layer.ShadowPath?.Dispose();
            Layer.ShadowPath = shadowPath.CGPath;

            Layer.CornerRadius = 8;
            Layer.ShadowRadius = 4;
            Layer.ShadowOpacity = 0.1f;
            Layer.MasksToBounds = false;
            Layer.ShadowOffset = new CGSize(0, 2);
            Layer.ShadowColor = UIColor.Black.CGColor;
        }

        private void hideProjectTaskClient()
        {
            ProjectDot.Hidden
                = ProjectLabel.Hidden
                = ClientLabel.Hidden
                = true;
        }
    }
}