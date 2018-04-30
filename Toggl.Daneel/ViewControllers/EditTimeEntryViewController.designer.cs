// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.ViewControllers
{
	[Register ("EditViewController")]
	partial class EditTimeEntryViewController
	{
		[Outlet]
		UIKit.UIStackView AddProjectAndTaskView { get; set; }

		[Outlet]
		UIKit.UIStackView AddTagsView { get; set; }

		[Outlet]
		UIKit.UISwitch BillableSwitch { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIView BillableView { get; set; }

		[Outlet]
		UIKit.UIView CategorizeWithProjectsBubbleView { get; set; }

		[Outlet]
		UIKit.UIButton CloseButton { get; set; }

		[Outlet]
		UIKit.UIButton ConfirmButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ConfirmButtonBottomConstraint { get; set; }

		[Outlet]
		UIKit.UIButton DeleteButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint DeleteButtonBottomConstraint { get; set; }

		[Outlet]
		Toggl.Daneel.Views.TextViewWithPlaceholder DescriptionTextView { get; set; }

		[Outlet]
		UIKit.UILabel DurationLabel { get; set; }

		[Outlet]
		UIKit.UIView DurationView { get; set; }

		[Outlet]
		UIKit.UILabel EndTimeLabel { get; set; }

		[Outlet]
		UIKit.UIView EndTimeView { get; set; }

		[Outlet]
		UIKit.UILabel ErrorMessageLabel { get; set; }

		[Outlet]
		UIKit.UIView ErrorView { get; set; }

		[Outlet]
		UIKit.UILabel ProjectTaskClientLabel { get; set; }

		[Outlet]
		UIKit.UILabel RemainingCharacterCount { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIScrollView ScrollView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIStackView ScrollViewContent { get; set; }

		[Outlet]
		UIKit.UILabel StartDateLabel { get; set; }

		[Outlet]
		UIKit.UIView StartDateView { get; set; }

		[Outlet]
		UIKit.UILabel StartTimeLabel { get; set; }

		[Outlet]
		UIKit.UIView StartTimeView { get; set; }

		[Outlet]
		UIKit.UIButton StopButton { get; set; }

		[Outlet]
		UIKit.UITextView TagsTextView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (AddProjectAndTaskView != null) {
				AddProjectAndTaskView.Dispose ();
				AddProjectAndTaskView = null;
			}

			if (AddTagsView != null) {
				AddTagsView.Dispose ();
				AddTagsView = null;
			}

			if (BillableSwitch != null) {
				BillableSwitch.Dispose ();
				BillableSwitch = null;
			}

			if (BillableView != null) {
				BillableView.Dispose ();
				BillableView = null;
			}

			if (CloseButton != null) {
				CloseButton.Dispose ();
				CloseButton = null;
			}

			if (ConfirmButton != null) {
				ConfirmButton.Dispose ();
				ConfirmButton = null;
			}

			if (ConfirmButtonBottomConstraint != null) {
				ConfirmButtonBottomConstraint.Dispose ();
				ConfirmButtonBottomConstraint = null;
			}

			if (DeleteButton != null) {
				DeleteButton.Dispose ();
				DeleteButton = null;
			}

			if (DeleteButtonBottomConstraint != null) {
				DeleteButtonBottomConstraint.Dispose ();
				DeleteButtonBottomConstraint = null;
			}

			if (DescriptionTextView != null) {
				DescriptionTextView.Dispose ();
				DescriptionTextView = null;
			}

			if (DurationLabel != null) {
				DurationLabel.Dispose ();
				DurationLabel = null;
			}

			if (DurationView != null) {
				DurationView.Dispose ();
				DurationView = null;
			}

			if (ProjectTaskClientLabel != null) {
				ProjectTaskClientLabel.Dispose ();
				ProjectTaskClientLabel = null;
			}

			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}

			if (ScrollViewContent != null) {
				ScrollViewContent.Dispose ();
				ScrollViewContent = null;
			}

			if (ErrorMessageLabel != null) {
				ErrorMessageLabel.Dispose ();
				ErrorMessageLabel = null;
			}

			if (ErrorView != null) {
				ErrorView.Dispose ();
				ErrorView = null;
			}

			if (StartDateLabel != null) {
				StartDateLabel.Dispose ();
				StartDateLabel = null;
			}

			if (StartDateView != null) {
				StartDateView.Dispose ();
				StartDateView = null;
			}

			if (StartTimeLabel != null) {
				StartTimeLabel.Dispose ();
				StartTimeLabel = null;
			}

			if (StartTimeView != null) {
				StartTimeView.Dispose ();
				StartTimeView = null;
			}

			if (EndTimeLabel != null) {
				EndTimeLabel.Dispose ();
				EndTimeLabel = null;
			}

			if (EndTimeView != null) {
				EndTimeView.Dispose ();
				EndTimeView = null;
			}

			if (StopButton != null) {
				StopButton.Dispose ();
				StopButton = null;
			}

			if (TagsTextView != null) {
				TagsTextView.Dispose ();
				TagsTextView = null;
			}

			if (CategorizeWithProjectsBubbleView != null) {
				CategorizeWithProjectsBubbleView.Dispose ();
				CategorizeWithProjectsBubbleView = null;
			}

			if (RemainingCharacterCount != null) {
				RemainingCharacterCount.Dispose ();
				RemainingCharacterCount = null;
			}
		}
	}
}
