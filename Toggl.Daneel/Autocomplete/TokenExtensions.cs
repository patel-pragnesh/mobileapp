using System;
using Foundation;
using MvvmCross.UI;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Multivac.Extensions;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Toggl.Daneel.Autocomplete
{
    public static class TokenExtensions
    {
        private const int lineHeight = 24;
        private const int maxTextLength = 50;

        private const int textFieldInfoTokenLeftMargin = 6;
        private const int textFieldInfoTokenRightMargin = 0;

        private static readonly nfloat textVerticalOffset;
        private static readonly NSParagraphStyle paragraphStyle;
        private static readonly UIStringAttributes tagAttributes;
        private static readonly UIFont tokenFont = UIFont.SystemFontOfSize(12, UIFontWeight.Regular);
        private static readonly UIFont regularFont = UIFont.SystemFontOfSize(16, UIFontWeight.Regular);

        public static readonly NSString Project = new NSString(nameof(Project));
        public static readonly NSString TagIndex = new NSString(nameof(TagIndex));

        static TokenExtensions()
        {
            paragraphStyle = new NSMutableParagraphStyle
            {
                MinimumLineHeight = lineHeight,
                MaximumLineHeight = lineHeight
            };

            tagAttributes = new UIStringAttributes
            {
                Font = tokenFont,
                ForegroundColor = Color.StartTimeEntry.TokenText.ToNativeColor()
            };

            textVerticalOffset = (lineHeight / 2) - (tokenFont.CapHeight / 2) - 3;
        }

        public static string GetDescription(this UITextView self) => self.Text.Replace("￼", "");

        public static NSAttributedString GetAttributedText(this TextFieldInfo self)
        {
            var result = new NSMutableAttributedString(self.Text, new UIStringAttributes
            {
                Font = regularFont,
                ParagraphStyle = paragraphStyle
            });

            addProjectAttachmentsIfNeeded(self, result);
            addTagAttachmentsIfNeeded(self, result);

            return result;
        }

        public static TokenTextAttachment GetTagToken(this string tag, int leftMargin, int rightMargin)
            => new TokenTextAttachment(
                new NSMutableAttributedString(tag.TruncatedAt(maxTextLength), tagAttributes),
                textVerticalOffset,
                regularFont.Descender,
                leftMargin,
                rightMargin);

        public static IEnumerable<TokenTextAttachment> GetTagTokens(this IEnumerable<string> tags, int leftMargin, int rightMargin)
            => tags.Select(tag => tag.GetTagToken(leftMargin, rightMargin));

        private static void addProjectAttachmentsIfNeeded(TextFieldInfo info, NSMutableAttributedString finalString)
        {
            if (string.IsNullOrEmpty(info.ProjectColor)) return;

            var color = MvxColor.ParseHexString(info.ProjectColor).ToNativeColor();

            var projectName = new NSAttributedString(info.ProjectName.TruncatedAt(maxTextLength), new UIStringAttributes
            {
                Font = tokenFont,
                ForegroundColor = color
            });

            var textAttachment = new TokenTextAttachment(
                projectName,
                textVerticalOffset,
                color,
                regularFont.Descender,
                textFieldInfoTokenLeftMargin,
                textFieldInfoTokenRightMargin
            );
            var tokenString = new NSMutableAttributedString(NSAttributedString.FromAttachment(textAttachment));
            var attributes = new UIStringAttributes { ParagraphStyle = paragraphStyle };
            attributes.Dictionary[Project] = new NSObject();
            tokenString.AddAttributes(attributes, new NSRange(0, tokenString.Length));

            finalString.Append(tokenString);
        }

        private static void addTagAttachmentsIfNeeded(TextFieldInfo info, NSMutableAttributedString finalString)
        {
            var tagTokens = info
                .Tags
                .Select(tag => tag.Name)
                .GetTagTokens(textFieldInfoTokenLeftMargin, textFieldInfoTokenRightMargin)
                .Select(NSAttributedString.FromAttachment)
                .Select(tagToken => new NSMutableAttributedString(tagToken));

            int i = 0;
            foreach (var tagToken in tagTokens)
            {
                var attributes = new UIStringAttributes { ParagraphStyle = paragraphStyle };
                attributes.Dictionary[TagIndex] = new NSNumber(i);
                tagToken.AddAttributes(attributes, new NSRange(0, tagToken.Length));
                finalString.Append(tagToken);
                i++;
            }
        }
    }
}
