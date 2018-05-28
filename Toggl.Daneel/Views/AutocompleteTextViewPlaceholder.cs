using System;
using Foundation;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Daneel.Autocomplete;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Views
{
    [Register(nameof(AutocompleteTextViewPlaceholder))]
    public sealed class AutocompleteTextViewPlaceholder : UILabel
    {
        private AutocompleteTextView textView;

        private AutocompleteTextViewDelegate textViewDelegate;

        public AutocompleteTextView TextView
        {
            get => textView;
            set
            {
                textView = value;

                // AttributedText at this point has a length of 1, which is
                // a strange behaviour for an otherwise empty TextView.
                // It is set here to an empty string so that the initial visibility  
                // of the placeholder can be correctly inferred.
                textView.AttributedText = new NSAttributedString("");

                textViewDelegate = textView.AutocompleteTextViewInfoDelegate;
                textViewDelegate.TextChanged += handleEvent;
                textViewDelegate.ProjectDeleted += handleEvent;
                textViewDelegate.TagDeleted += handleEvent;
                textViewDelegate.IsWritingMultistageCharacter += handleMultistageCharacterTyping;

                setupProperties();
                updateVisibility();
            }
        }

        public AutocompleteTextViewPlaceholder(IntPtr handle) : base(handle)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (textViewDelegate != null)
            {
                textViewDelegate.TextChanged -= handleEvent;
                textViewDelegate.ProjectDeleted -= handleEvent;
                textViewDelegate.TagDeleted -= handleEvent;
                textViewDelegate.IsWritingMultistageCharacter -= handleMultistageCharacterTyping;
            }
        }

        private void setupProperties()
        {
            Font = UIFont.SystemFontOfSize(textView.Font.PointSize);
            TextColor = Color.StartTimeEntry.Placeholder.ToNativeColor();
            Text = Resources.StartTimeEntryPlaceholder;
            TextAlignment = UITextAlignment.Left;
            TranslatesAutoresizingMaskIntoConstraints = false;
        }

        private void handleEvent(object sender, EventArgs args)
        {
            updateVisibility();
        }

        private void handleMultistageCharacterTyping(object sender, EventArgs args)
        {
            Hidden = true;
        }

        private void updateVisibility()
        {
            var hasText = textView.AttributedText?.Length > 0;
            var isInMultiStageEdit = textView.MarkedTextRange != null;

            Hidden = hasText || isInMultiStageEdit;
        }
    }
}
