using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Plugin.Color.Platforms.Ios;
using MvvmCross.Plugin.Visibility;
using Toggl.Daneel.Combiners;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Views
{
    public partial class StartTimeEntryViewCell : MvxTableViewCell
    {
        private const float NoProjectDistance = 16;
        private const float HasProjectDistance = 8;

        public static readonly NSString Key = new NSString(nameof(StartTimeEntryViewCell));
        public static readonly UINib Nib;

        static StartTimeEntryViewCell()
        {
            Nib = UINib.FromName(nameof(StartTimeEntryViewCell), NSBundle.MainBundle);
        }

        protected StartTimeEntryViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var visibilityConverter = new MvxVisibilityValueConverter();
                var descriptionTopDistanceValueConverter = new BoolToConstantValueConverter<nfloat>(HasProjectDistance, NoProjectDistance);
                var projectTaskClientCombiner = new ProjectTaskClientValueCombiner(
                    ProjectLabel.Font.CapHeight,
                    Color.Suggestions.ClientColor.ToNativeColor(),
                    true
                );

                var bindingSet = this.CreateBindingSet<StartTimeEntryViewCell, TimeEntrySuggestion>();

                //Text
                bindingSet.Bind(DescriptionLabel).To(vm => vm.Description);

                bindingSet.Bind(ProjectLabel)
                    .For(v => v.AttributedText)
                    .ByCombining(projectTaskClientCombiner,
                        v => v.ProjectName,
                        v => v.TaskName,
                        v => v.ClientName,
                        v => v.ProjectColor);

                //Visibility
                bindingSet.Bind(DescriptionTopDistanceConstraint)
                          .For(v => v.Constant)
                          .To(vm => vm.HasProject)
                          .WithConversion(descriptionTopDistanceValueConverter);

                bindingSet.Bind(ProjectLabel)
                          .For(v => v.BindVisibility())
                          .To(vm => vm.HasProject)
                          .WithConversion(visibilityConverter);

                bindingSet.Apply();
            });
        }
    }
}
