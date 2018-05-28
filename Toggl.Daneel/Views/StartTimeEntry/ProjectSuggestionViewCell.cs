using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Commands;
using MvvmCross.Plugin.Color;
using MvvmCross.Plugin.Visibility;
using Toggl.Daneel.Combiners;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Converters;
using UIKit;

namespace Toggl.Daneel.Views
{
    public partial class ProjectSuggestionViewCell : MvxTableViewCell
    {
        private const float selectedProjectBackgroundAlpha = 0.12f;

        private const int fadeViewTrailingConstraintWithTasks = 72;
        private const int fadeViewTrailingConstraintWithoutTasks = 16;

        public static readonly NSString Key = new NSString(nameof(ProjectSuggestionViewCell));
        public static readonly UINib Nib;

        public bool TopSeparatorHidden
        {
            get => TopSeparatorView.Hidden;
            set => TopSeparatorView.Hidden = value;
        }

        public bool BottomSeparatorHidden
        {
            get => BottomSeparatorView.Hidden;
            set => BottomSeparatorView.Hidden = value;
        }

        static ProjectSuggestionViewCell()
        {
            Nib = UINib.FromName(nameof(ProjectSuggestionViewCell), NSBundle.MainBundle);
        }

        protected ProjectSuggestionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand { get; set; }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            FadeView.FadeRight = true;
            ClientNameLabel.LineBreakMode = UILineBreakMode.TailTruncation;
            ProjectNameLabel.LineBreakMode = UILineBreakMode.TailTruncation;
            ToggleTasksButton.TouchUpInside += togglTasksButton;

            this.DelayBind(() =>
            {
                var colorConverter = new MvxRGBValueConverter();
                var taskCountConverter = new TaskCountValueConverter();
                var visibilityConverter = new MvxVisibilityValueConverter();
                var projectSelectedColorCombiner
                    = new ProjectSelectedColorValueCombiner(selectedProjectBackgroundAlpha);
                var fadeViewTrailingConstantConverter = new BoolToConstantValueConverter<nfloat>(
                    fadeViewTrailingConstraintWithTasks,
                    fadeViewTrailingConstraintWithoutTasks
                );

                var bindingSet = this.CreateBindingSet<ProjectSuggestionViewCell, ProjectSuggestion>();

                //Text
                bindingSet.Bind(ProjectNameLabel).To(vm => vm.ProjectName);
                bindingSet.Bind(ClientNameLabel).To(vm => vm.ClientName);
                bindingSet.Bind(AmountOfTasksLabel)
                          .To(vm => vm.NumberOfTasks)
                          .WithConversion(taskCountConverter);

                //Color
                bindingSet.Bind(ProjectNameLabel)
                          .For(v => v.TextColor)
                          .To(vm => vm.ProjectColor)
                          .WithConversion(colorConverter);

                bindingSet.Bind(ProjectDotView)
                          .For(v => v.BackgroundColor)
                          .To(vm => vm.ProjectColor)
                          .WithConversion(colorConverter);

                bindingSet.Bind(SelectedProjectView)
                          .For(v => v.BackgroundColor)
                          .ByCombining(
                              projectSelectedColorCombiner,
                              vm => vm.Selected,
                              vm => vm.ProjectColor);

                //Visibility
                bindingSet.Bind(ToggleTaskImage)
                          .For(v => v.BindVisibility())
                          .To(vm => vm.NumberOfTasks)
                          .WithConversion(visibilityConverter);
                
                bindingSet.Bind(ToggleTasksButton)
                          .For(v => v.BindVisibility())
                          .To(vm => vm.NumberOfTasks)
                          .WithConversion(visibilityConverter);

                //Constraints
                bindingSet.Bind(FadeViewTrailingConstraint)
                          .For(c => c.Constant)
                          .To(MvxViewModel => MvxViewModel.HasTasks)
                          .WithConversion(fadeViewTrailingConstantConverter);
                
                bindingSet.Apply();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;
            ToggleTasksButton.TouchUpInside -= togglTasksButton;
        }

        private void togglTasksButton(object sender, EventArgs e)
            => ToggleTasksCommand?.Execute((ProjectSuggestion)DataContext);
    }
}
