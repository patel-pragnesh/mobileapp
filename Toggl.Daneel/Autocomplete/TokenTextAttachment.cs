using System;
using CoreGraphics;
using UIKit;

namespace Toggl.Daneel.Autocomplete
{
    public abstract class TokenTextAttachment : NSTextAttachment
    {
        protected const int LineHeight = 24;
        protected const int TokenHeight = 22;
        protected const int TokenPadding = 6;
        protected const float TokenCornerRadius = 6.0f;
        protected const int TokenVerticallOffset = (LineHeight - TokenHeight) / 2;

        public readonly nfloat fontDescender;

        public TokenTextAttachment(nfloat fontDescender)
        {
            this.fontDescender = fontDescender;
        }

        public override CGRect GetAttachmentBounds(NSTextContainer textContainer,
            CGRect proposedLineFragment, CGPoint glyphPosition, nuint characterIndex)
        {
            var rect = base.GetAttachmentBounds(textContainer,
                proposedLineFragment, glyphPosition, characterIndex);

            rect.Y = fontDescender;
            return rect;
        }
    }
}
