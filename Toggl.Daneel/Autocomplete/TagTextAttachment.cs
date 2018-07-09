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

        public TagTextAttachment(NSAttributedString stringToDraw, nfloat textVerticalOffset, nfloat fontDescender)
            : base (fontDescender)
        {
            var size = new CGSize(
                stringToDraw.Size.Width + TokenMargin + TokenMargin + (TokenPadding * 2),
                LineHeight
            );

            UIGraphics.BeginImageContextWithOptions(size, false, 0.0f);
            using (var context = UIGraphics.GetCurrentContext())
            {
                var tokenPath = UIBezierPath.FromRoundedRect(new CGRect(
                    x: TokenMargin,
                    y: TokenVerticallOffset,
                    width: size.Width - TokenMargin - TokenMargin,
                    height: TokenHeight
                ), TokenCornerRadius);
                context.AddPath(tokenPath.CGPath);
                context.SetStrokeColor(borderColor.CGColor);
                context.StrokePath();

                stringToDraw.DrawString(new CGPoint(TokenMargin + TokenPadding, textVerticalOffset));

                var image = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                Image = image;
            }
        }
    }
}
