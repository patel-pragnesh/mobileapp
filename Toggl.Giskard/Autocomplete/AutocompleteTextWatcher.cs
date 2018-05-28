using System;
using System.Linq;
using Android.Text;
using Java.Lang;
using MvvmCross.Base;
using Toggl.Foundation.MvvmCross.Autocomplete;
using Object = Java.Lang.Object;

namespace Toggl.Giskard.Autocomplete
{
    public sealed class AutocompleteTextWatcher : Object, ITextWatcher, IAutocompleteEventProvider
    {
        public event EventHandler TextChanged;
        public event EventHandler ProjectDeleted;
        public event EventHandler CursorPositionChanged;
        public event EventHandler<TagDeletedEventArgs> TagDeleted;

        public void AfterTextChanged(IEditable sequence)
        {
        }

        public void BeforeTextChanged(ICharSequence sequence, int start, int count, int after)
        {
            var isDeleting = count > after;
            if (!isDeleting) return;

            var spannable = sequence as SpannableStringBuilder;
            var deletedSpan = spannable.GetSpans(start, start, Class.FromType(typeof(TokenSpan))).LastOrDefault();
            if (deletedSpan == null) return;

            if (deletedSpan is ProjectTokenSpan)
            {
                ProjectDeleted?.Raise(this);
            }
            else if (deletedSpan is TagsTokenSpan tagSpan)
            {
                TagDeleted?.Invoke(this, new TagDeletedEventArgs(start, tagSpan.TagIndex));
            }
        }

        public void OnTextChanged(ICharSequence sequence, int start, int before, int count)
        {
            TextChanged?.Raise(this);
        }
    }
}
