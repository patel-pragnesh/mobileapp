using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Toggl.Daneel.Autocomplete
{
    public sealed class ProjectTextAttachment : TokenTextAttachment
    {
        private const int dotPadding = 6;
        private const int dotDiameter = 4;
        private const int dotRadius = dotDiameter / 2;
        private const int dotYOffset = (LineHeight / 2) - dotRadius;

        public ProjectTextAttachment(
            NSAttributedString projectStringToDraw, 
            nfloat textVerticalOffset,
            UIColor projectColor,
            nfloat fontDescender,
            int leftMargin,
            int rightMargin)
            : base (fontDescender)
        {
            const int circleWidth = dotDiameter + dotPadding;
            var totalWidth = projectStringToDraw.Size.Width + circleWidth + leftMargin + rightMargin + (TokenPadding * 2);
            var size = new CGSize(totalWidth, LineHeight);

            UIGraphics.BeginImageContextWithOptions(size, false, 0.0f);
            using (var context = UIGraphics.GetCurrentContext())
            {
                var tokenPath = UIBezierPath.FromRoundedRect(new CGRect(
                    x: leftMargin,
                    y: TokenVerticallOffset,
                    width: totalWidth - leftMargin - rightMargin,
                    height: TokenHeight
                ), TokenCornerRadius);
                context.AddPath(tokenPath.CGPath);
                context.SetFillColor(projectColor.ColorWithAlpha(0.12f).CGColor);
                context.FillPath();

                var dot = UIBezierPath.FromRoundedRect(new CGRect(
                    x: dotPadding + leftMargin,
                    y: dotYOffset,
                    width: dotDiameter,
                    height: dotDiameter
                ), dotRadius);
                context.AddPath(dot.CGPath);
                context.SetFillColor(projectColor.CGColor);
                context.FillPath();

                projectStringToDraw.DrawString(new CGPoint(circleWidth + leftMargin + TokenPadding, textVerticalOffset));

                var image = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                Image = image;
            }
        }
    }
}
