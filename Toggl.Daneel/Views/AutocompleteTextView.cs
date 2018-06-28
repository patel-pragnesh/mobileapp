using System;
using Foundation;
using Toggl.Daneel.Autocomplete;
using UIKit;

namespace Toggl.Daneel.Views
{
    [Register(nameof(AutocompleteTextView))]
    public sealed class AutocompleteTextView : UITextView
    {
        public AutocompleteTextViewDelegate AutocompleteTextViewInfoDelegate { get; } = new AutocompleteTextViewDelegate();

        public override NSAttributedString AttributedText
        {
            get => base.AttributedText;
            set
            {
                InputDelegate = inputDelegate;
                base.AttributedText = value;
                AutocompleteTextViewInfoDelegate.Changed(this);
            }
        }

        private AutocompleteTextViewInputDelegate inputDelegate = new AutocompleteTextViewInputDelegate();

        public AutocompleteTextView(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Delegate = AutocompleteTextViewInfoDelegate;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            Delegate = null;
        }
    }

    class AutocompleteTextViewInputDelegate : NSObject, IUITextInputDelegate
    {
        public void SelectionDidChange(IUITextInput uiTextInput)
        {
        }

        public void SelectionWillChange(IUITextInput uiTextInput)
        {
        }

        public void TextDidChange(IUITextInput textInput)
        {
        }

        public void TextWillChange(IUITextInput textInput)
        {
        }
    }
}
