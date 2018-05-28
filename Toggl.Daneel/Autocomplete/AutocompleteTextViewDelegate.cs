using System;
using Foundation;
using MvvmCross.Base;
using Toggl.Foundation.MvvmCross.Autocomplete;
using UIKit;

namespace Toggl.Daneel.Autocomplete
{
    public sealed class AutocompleteTextViewDelegate : UITextViewDelegate, IAutocompleteEventProvider
    {
        public event Action TextViewDidChange;
        public event EventHandler TextChanged;
        public event EventHandler ProjectDeleted;
        public event EventHandler CursorPositionChanged;
        public event EventHandler IsWritingMultistageCharacter;
        public event EventHandler<TagDeletedEventArgs> TagDeleted;

        public override void Changed(UITextView textView)
        {
            TextViewDidChange?.Invoke();

            // When the `MarkedTextRange` property of the UITextView is not null
            // then it means that the user is in the middle of inputting a multistage character.
            // Hold off on editing the attributedText until they are done.
            // Source: https://stackoverflow.com/questions/31430308/uitextview-attributedtext-with-japanese-keyboard-repeats-input
            if (textView.MarkedTextRange != null) return;

            TextChanged.Raise(this);
        }

        public override bool ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            if (isTypingMultistageCharacter(range, text))
            {
                IsWritingMultistageCharacter.Raise(this);
                return true;
            }

            if (!isPressingBackspace(range, text))
                return true;

            var cursorPosition = range.Location;
            var attrs = textView.AttributedText.GetAttributes(cursorPosition, out var attrRange);

            var isDeletingProject = attrs.ObjectForKey(TokenExtensions.Project) != null;
            if (isDeletingProject)
            {
                ProjectDeleted.Raise(this);
                return false;
            }

            var tagIndex = attrs.ObjectForKey(TokenExtensions.TagIndex) as NSNumber;
            var isDeletingTag = tagIndex != null;
            if (isDeletingTag)
            {
                TagDeleted?.Invoke(this, new TagDeletedEventArgs((int)cursorPosition, tagIndex.Int32Value));
                return false;
            }

            return true;
        }

        private bool isTypingMultistageCharacter(NSRange range, string text)
            => range.Length == 0 && text.Length > 0;

        private static bool isPressingBackspace(NSRange range, string text)
            => range.Length == 1 && text.Length == 0;

        internal void RaisePositionChanged()
        {
            CursorPositionChanged.Raise(this);
        }
    }
}
