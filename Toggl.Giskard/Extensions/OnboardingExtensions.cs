﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Android.Views;
using Android.Widget;
using MvvmCross.Platform.Core;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Extensions;
using Toggl.PrimeRadiant.Onboarding;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Giskard.Extensions
{
    public static class OnboardingExtensions
    {
        private const int windowTokenCheckInterval = 100;

        public static IDisposable ManageDismissableTooltip(this IOnboardingStep step, PopupWindow tooltip, View anchor, Func<PopupWindow, View, PopupOffsets> popupOffsetsGenerator, IOnboardingStorage storage)
        {
            Ensure.Argument.IsNotNull(anchor, nameof(tooltip));
            Ensure.Argument.IsNotNull(anchor, nameof(anchor));

            var dismissableStep = step.ToDismissable(step.GetType().FullName, storage);

            void OnDismiss(object sender, EventArgs args)
            {
                tooltip.Dismiss();
                dismissableStep.Dismiss();
            }

            tooltip.ContentView.Click += OnDismiss;

            return dismissableStep.ManageVisibilityOf(tooltip, anchor, popupOffsetsGenerator);
        }

        private static IDisposable ManageVisibilityOf(this IOnboardingStep step, PopupWindow popupWindowTooltip, View anchor, Func<PopupWindow, View, PopupOffsets> popupOffsetsGenerator)
        {

            void toggleVisibilityOnMainThread(bool shouldBeVisible)
            {
                if (shouldBeVisible)
                {
                    showPopupTooltip(popupWindowTooltip, anchor, popupOffsetsGenerator);
                }
                else
                {
                    popupWindowTooltip.Dismiss();
                }
            }

            return step.ShouldBeVisible
                .combineWithWindowTokenAvailabilityFrom(anchor)
                .Subscribe(toggleVisibilityOnMainThread);
        }

        private static void showPopupTooltip(PopupWindow popupWindow, View anchor, Func<PopupWindow, View, PopupOffsets> popupOffsetsGenerator)
        {
            anchor.Post(() =>
            {
                popupWindow.ContentView.Measure(View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified), View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                var offsets = popupOffsetsGenerator(popupWindow, anchor);
                popupWindow.ShowAsDropDown(anchor, offsets.HorizontalOffset, offsets.VerticalOffset);
            });
        }

        private static IObservable<bool> combineWithWindowTokenAvailabilityFrom(this IObservable<bool> shouldBeVisibleObservable, View anchor)
        {
            var viewTokenObservable = Observable.Create<bool>(observer =>
            {
                if (anchor == null)
                {
                    observer.OnNext(false);
                    observer.OnCompleted();
                    return Disposable.Empty;
                }

                void checkForToken()
                {
                    if (anchor.WindowToken == null)
                    {
                        observer.OnNext(false);
                    }
                    else
                    {
                        observer.OnNext(true);
                        observer.OnCompleted();
                    }
                }

                return Observable
                    .Interval(TimeSpan.FromMilliseconds(windowTokenCheckInterval))
                    .Subscribe(_ => checkForToken());
            });

            return shouldBeVisibleObservable.CombineLatest(viewTokenObservable,
                (shouldBeVisible, windowTokenIsReady)
                    => visibleWhenBothAreReady(shouldBeVisible, windowTokenIsReady, viewTokenObservable));
        }

        private static bool visibleWhenBothAreReady(bool shouldBeVisible, bool windowTokenIsReady, IObservable<bool> tokenObservable)
        {
            if (shouldBeVisible)
            {
                return windowTokenIsReady;
            }

            tokenObservable.DisposeIfDisposable();
            return false;
        }
    }
}
