using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Foundation.Autocomplete.Suggestions;
using UIKit;

namespace Toggl.Daneel.Views
{
    public sealed partial class TagSuggestionViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(TagSuggestionViewCell));
        public static readonly UINib Nib;

        static TagSuggestionViewCell()
        {
            Nib = UINib.FromName(nameof(TagSuggestionViewCell), NSBundle.MainBundle);
        }

        public TagSuggestionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<TagSuggestionViewCell, TagSuggestion>();
                bindingSet.Bind(NameLabel).To(vm => vm.Name);
                bindingSet.Apply();
            });
        }
    }
}
