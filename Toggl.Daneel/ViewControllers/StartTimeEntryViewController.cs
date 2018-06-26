﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Foundation;
using MvvmCross.Binding;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.WeakSubscription;
using MvvmCross.Plugins.Color.iOS;
using MvvmCross.Plugins.Visibility;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Onboarding.CreationView;
using Toggl.Daneel.Onboarding.StartTimeEntryView;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac;
using UIKit;
using static Toggl.PrimeRadiant.Extensions.OnboardingStepExtensions;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public sealed partial class StartTimeEntryViewController : KeyboardAwareViewController<StartTimeEntryViewModel>
    {
        private UIImage greyCheckmarkButtonImage;

        private UIImage greenCheckmarkButtonImage;

        private IDisposable descriptionDisposable;
        private IDisposable addProjectOrTagOnboardingDisposable;
        private IDisposable disabledConfirmationButtonOnboardingDisposable;


        private ISubject<bool> isDescriptionEmptySubject;

        public StartTimeEntryViewController() 
            : base(nameof(StartTimeEntryViewController))
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            descriptionDisposable?.Dispose();
            descriptionDisposable = null;

            addProjectOrTagOnboardingDisposable?.Dispose();
            addProjectOrTagOnboardingDisposable = null;

            disabledConfirmationButtonOnboardingDisposable?.Dispose();
            disabledConfirmationButtonOnboardingDisposable = null;

            TimeInput.LostFocus -= onTimeInputLostFocus;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();
            prepareOnboarding();

            var source = new StartTimeEntryTableViewSource(SuggestionsTableView);
            SuggestionsTableView.Source = source;
            source.ToggleTasksCommand = new MvxCommand<ProjectSuggestion>(toggleTaskSuggestions);

            var parametricDurationConverter = new ParametricTimeSpanToDurationValueConverter();
            var invertedVisibilityConverter = new MvxInvertedVisibilityValueConverter();
            var invertedBoolConverter = new BoolToConstantValueConverter<bool>(false, true);
            var buttonColorConverter = new BoolToConstantValueConverter<UIColor>(
                Color.StartTimeEntry.ActiveButton.ToNativeColor(),
                Color.StartTimeEntry.InactiveButton.ToNativeColor()
            );

            var bindingSet = this.CreateBindingSet<StartTimeEntryViewController, StartTimeEntryViewModel>();

            //TableView
            bindingSet.Bind(source)
                      .For(v => v.ObservableCollection)
                      .To(vm => vm.Suggestions);

            bindingSet.Bind(source)
                      .For(v => v.UseGrouping)
                      .To(vm => vm.UseGrouping);

            bindingSet.Bind(source)
                      .For(v => v.SelectSuggestionCommand)
                      .To(vm => vm.SelectSuggestionCommand);

            bindingSet.Bind(source)
                      .For(v => v.CreateCommand)
                      .To(vm => vm.CreateCommand);
            
            bindingSet.Bind(source)
                      .For(v => v.IsSuggestingProjects)
                      .To(vm => vm.IsSuggestingProjects);
            
            bindingSet.Bind(source)
                      .For(v => v.Text)
                      .To(vm => vm.CurrentQuery);

            bindingSet.Bind(source)
                      .For(v => v.SuggestCreation)
                      .To(vm => vm.SuggestCreation);

            bindingSet.Bind(source)
                      .For(v => v.ShouldShowNoTagsInfoMessage)
                      .To(vm => vm.ShouldShowNoTagsInfoMessage);

            bindingSet.Bind(source)
                      .For(v => v.ShouldShowNoProjectsInfoMessage)
                      .To(vm => vm.ShouldShowNoProjectsInfoMessage);

            //Text
            bindingSet.Bind(TimeInput)
                      .For(v => v.Duration)
                      .To(vm => vm.DisplayedTime)
                      .Mode(MvxBindingMode.OneWayToSource);

            bindingSet.Bind(DescriptionTextView)
                      .For(v => v.BindTextFieldInfo())
                      .To(vm => vm.TextFieldInfo);

            bindingSet.Bind(TimeLabel)
                      .For(v => v.Text)
                      .To(vm => vm.DisplayedTime)
                      .WithConversion(parametricDurationConverter, DurationFormat.Improved);

            bindingSet.Bind(Placeholder)
                      .To(vm => vm.PlaceholderText);

            bindingSet.Bind(DescriptionRemainingLengthLabel)
                      .To(vm => vm.DescriptionRemainingBytes);

            //Buttons
            bindingSet.Bind(TagsButton)
                      .For(v => v.TintColor)
                      .To(vm => vm.IsSuggestingTags)
                      .WithConversion(buttonColorConverter);
            
            bindingSet.Bind(BillableButton)
                      .For(v => v.TintColor)
                      .To(vm => vm.IsBillable)
                      .WithConversion(buttonColorConverter);

            bindingSet.Bind(ProjectsButton)
                      .For(v => v.TintColor)
                      .To(vm => vm.IsSuggestingProjects)
                      .WithConversion(buttonColorConverter);

            bindingSet.Bind(DoneButton)
                      .For(v => v.Enabled)
                      .To(vm => vm.DescriptionLengthExceeded)
                      .WithConversion(invertedBoolConverter);

            //Visibility
            bindingSet.Bind(BillableButtonWidthConstraint)
                      .For(v => v.Constant)
                      .To(vm => vm.IsBillableAvailable)
                      .WithConversion(new BoolToConstantValueConverter<nfloat>(42, 0));

            bindingSet.Bind(DescriptionRemainingLengthLabel)
                      .For(v => v.BindVisible())
                      .To(vm => vm.DescriptionLengthExceeded)
                      .WithConversion(invertedVisibilityConverter);

            //Commands
            bindingSet.Bind(DoneButton).To(vm => vm.DoneCommand);
            bindingSet.Bind(CloseButton).To(vm => vm.BackCommand);
            bindingSet.Bind(BillableButton).To(vm => vm.ToggleBillableCommand);
            bindingSet.Bind(StartDateButton).To(vm => vm.SetStartDateCommand);
            bindingSet.Bind(DateTimeButton).To(vm => vm.ChangeTimeCommand);
            bindingSet.Bind(TagsButton).To(vm => vm.ToggleTagSuggestionsCommand);
            bindingSet.Bind(ProjectsButton).To(vm => vm.ToggleProjectSuggestionsCommand);

            bindingSet.Apply();
        }

        private void switchTimeLabelAndInput()
        {
            TimeLabel.Hidden = !TimeLabel.Hidden;
            TimeInput.Hidden = !TimeInput.Hidden;
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            BottomDistanceConstraint.Constant = e.FrameEnd.Height;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            BottomDistanceConstraint.Constant = 0;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            if (TimeInput.IsEditing)
            {
                TimeInput.EndEditing(true);
                DescriptionTextView.BecomeFirstResponder();
            }
        }

        private void prepareViews()
        {
            //This is needed for the ImageView.TintColor bindings to work
            foreach (var button in getButtons())
            {
                button.SetImage(
                    button.ImageForState(UIControlState.Normal)
                          .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    UIControlState.Normal
                );
                button.TintColor = Color.StartTimeEntry.InactiveButton.ToNativeColor();
            }

            TimeInput.TintColor = Color.StartTimeEntry.Cursor.ToNativeColor();

            DescriptionTextView.TintColor = Color.StartTimeEntry.Cursor.ToNativeColor();
            DescriptionTextView.BecomeFirstResponder();

            Placeholder.TextView = DescriptionTextView;
            Placeholder.Text = Resources.StartTimeEntryPlaceholder;

            prepareTimeViews();
        }

        private void prepareTimeViews()
        {
            var tapRecognizer = new UITapGestureRecognizer(() =>
            {
                if(!TimeLabel.Hidden)
                    ViewModel.DurationTapped.Execute();

                switchTimeLabelAndInput();

                if (!TimeInput.Hidden)
                {
                    TimeInput.FormattedDuration = TimeLabel.Text;
                    TimeInput.BecomeFirstResponder();
                }
            });

            TimeLabel.UserInteractionEnabled = true;
            TimeLabel.AddGestureRecognizer(tapRecognizer);

            TimeInput.LostFocus += onTimeInputLostFocus;
        }

        private void onTimeInputLostFocus(object sender, EventArgs e)
            => switchTimeLabelAndInput();

        private IEnumerable<UIButton> getButtons()
        {
            yield return TagsButton;
            yield return ProjectsButton;
            yield return BillableButton;
            yield return StartDateButton;
            yield return DateTimeButton;
            yield return DateTimeButton;
        }

        private void toggleTaskSuggestions(ProjectSuggestion parameter)
        {
            var offset = SuggestionsTableView.ContentOffset;
            var frameHeight = SuggestionsTableView.Frame.Height;

            ViewModel.ToggleTaskSuggestionsCommand.Execute(parameter);

            SuggestionsTableView.CorrectOffset(offset, frameHeight);
        }

        private void prepareOnboarding()
        {
            prepareAddProjectOnboardingStep();
            prepareDisableConfirmationButtonOnboardingStep();
        }

        private void prepareAddProjectOnboardingStep()
        {
            var onboardingStorage = ViewModel.OnboardingStorage;
            var addProjectOrtagOnboardingStep = new AddProjectOrTagOnboardingStep(
                onboardingStorage,
                ViewModel.DataSource
            );

            addProjectOrTagOnboardingDisposable = addProjectOrtagOnboardingStep
                .ManageDismissableTooltip(AddProjectOnboardingBubble, onboardingStorage);
        }

        private void prepareDisableConfirmationButtonOnboardingStep()
        {
            greyCheckmarkButtonImage = UIImage.FromBundle("icCheckGrey");
            greenCheckmarkButtonImage = UIImage.FromBundle("doneGreen");

            isDescriptionEmptySubject = new BehaviorSubject<bool>(String.IsNullOrEmpty(ViewModel.Description));

            descriptionDisposable = ViewModel.WeakSubscribe(() => ViewModel.Description, onDescriptionChanged);

            var disabledConfirmationButtonOnboardingStep
                = new DisabledConfirmationButtonOnboardingStep(
                    ViewModel.OnboardingStorage,
                    isDescriptionEmptySubject.AsObservable());

            disabledConfirmationButtonOnboardingDisposable
                = disabledConfirmationButtonOnboardingStep
                    .ShouldBeVisible
                    .Subscribe(visible => InvokeOnMainThread(() =>
                    {
                        var image = visible ? greyCheckmarkButtonImage : greenCheckmarkButtonImage;
                        DoneButton.SetImage(image, UIControlState.Normal);
                    }));
        }

        private void onDescriptionChanged(object sender, PropertyChangedEventArgs args)
        {
            isDescriptionEmptySubject.OnNext(String.IsNullOrEmpty(ViewModel.Description));
        }
    }
}

