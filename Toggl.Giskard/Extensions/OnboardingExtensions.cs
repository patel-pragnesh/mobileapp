using System;
using Android.Views;
using Android.Widget;
using Toggl.PrimeRadiant.Extensions;
using Toggl.PrimeRadiant.Onboarding;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Giskard.Extensions
{
    public static class OnboardingExtensions
    {
        private const int delayBeforeShowingTooltip = 100;

        public static IDisposable ManageVisibilityOf(this IOnboardingStep step, PopupWindow popupWindow, View anchor, Func<PopupWindow, View, PopupOffsets> popupOffsetsGenerator)
        {
            void toggleVisibilityOnMainThread(bool shouldBeVisible)
            {
                if (popupWindow == null || anchor == null) return;

                if (shouldBeVisible)
                {
                    anchor.PostDelayed(() =>
                    {
                        popupWindow.ContentView.Measure(View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified), View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                        var offsets = popupOffsetsGenerator.Invoke(popupWindow, anchor);
                        popupWindow.ShowAsDropDown(anchor, offsets.HorizontalOffset, offsets.VerticalOffset);
                    }, delayBeforeShowingTooltip);
                }
                else
                {
                    popupWindow.Dismiss();
                }
            }

            return step.ShouldBeVisible.Subscribe(toggleVisibilityOnMainThread);
        }

        public static IDisposable ManageDismissableTooltip(this IOnboardingStep step, PopupWindow tooltip, View anchor, Func<PopupWindow, View, PopupOffsets> popupOffsetsGenerator, IOnboardingStorage storage)
        {
            var dismissableStep = step.ToDismissable(step.GetType().FullName, storage);

            void OnDismiss(object sender, EventArgs args)
            {
                tooltip.Dismiss();
                dismissableStep.Dismiss();
            }

            tooltip.ContentView.Click += OnDismiss;

            return dismissableStep.ManageVisibilityOf(tooltip, anchor, popupOffsetsGenerator);
        }
    }

    public class PopupOffsets
    {
        public int HorizontalOffset { get; }
        public int VerticalOffset { get; }

        public PopupOffsets(int horizontalOffset, int verticalOffset)
        {
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
        }
    }
}
