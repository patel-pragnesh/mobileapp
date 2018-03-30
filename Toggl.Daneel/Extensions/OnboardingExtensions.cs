using System;
using CoreGraphics;
using Toggl.Daneel.Onboarding;
using Toggl.PrimeRadiant.Onboarding;
using UIKit;

namespace Toggl.Daneel.Extensions
{
    public static class Onboarding
    {
        private static readonly nfloat cellRadius = 8;

        private static readonly CGSize shadowOffset = new CGSize(0, 2);

        private static readonly CGColor shadowColor = UIColor.Black.CGColor;

        private static readonly nfloat shadowRadius = 6;

        private const float shadowOpacity = 0.1f;

        public static void MockSuggestion(this UIView view)
        {
            view.Layer.CornerRadius = cellRadius;
            view.Layer.ShadowOffset = shadowOffset;
            view.Layer.ShadowColor = shadowColor;
            view.Layer.ShadowOpacity = shadowOpacity;
            view.Layer.ShadowRadius = shadowRadius;
        }

        public static IDisposable ManageVisibilityOf(this IOnboardingStep step, UIView view)
            => step.ShouldBeVisible.Subscribe(
                visible => UIApplication.SharedApplication.InvokeOnMainThread(
                    () => view.Hidden = !visible));
    }
}
