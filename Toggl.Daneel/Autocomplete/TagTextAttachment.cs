using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Autocomplete
{
    public sealed class TagTextAttachment : TokenTextAttachment 
    {
        private static readonly UIColor borderColor = Color.StartTimeEntry.TokenBorder.ToNativeColor();

        public TagTextAttachment(
            NSAttributedString stringToDraw, 
            nfloat textVerticalOffset,
            nfloat fontDescender,
            int leftMargin,
            int rightMargin)
            : base (fontDescender)
        {
            var size = new CGSize(
                stringToDraw.Size.Width + leftMargin + rightMargin + (TokenPadding * 2),
                LineHeight
            );

            UIGraphics.BeginImageContextWithOptions(size, false, 0.0f);
            using (var context = UIGraphics.GetCurrentContext())
            {
                var tokenPath = UIBezierPath.FromRoundedRect(new CGRect(
                    x: leftMargin,
                    y: TokenVerticallOffset,
                    width: size.Width - leftMargin - rightMargin,
                    height: TokenHeight
                ), TokenCornerRadius);
                context.AddPath(tokenPath.CGPath);
                context.SetStrokeColor(borderColor.CGColor);
                context.StrokePath();

                stringToDraw.DrawString(new CGPoint(leftMargin + TokenPadding, textVerticalOffset));

                var image = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                Image = image;
            }
        }
    }
}
