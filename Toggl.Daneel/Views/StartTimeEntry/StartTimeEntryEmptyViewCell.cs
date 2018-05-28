using System;
using System.Globalization;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding.Views;
using MvvmCross.Converters;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Views
{
    public partial class StartTimeEntryEmptyViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(StartTimeEntryEmptyViewCell));
        public static readonly UINib Nib;

        static StartTimeEntryEmptyViewCell()
        {
            Nib = UINib.FromName(nameof(StartTimeEntryEmptyViewCell), NSBundle.MainBundle);
        }

        protected StartTimeEntryEmptyViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<StartTimeEntryEmptyViewCell, QuerySymbolSuggestion>();

                bindingSet.Bind(DescriptionLabel)
                          .For(v => v.AttributedText)
                          .To(vm => vm)
                          .WithConversion(new SuggestionAttributedStringValueConverter());

                bindingSet.Apply();
            });
        }

        private class SuggestionAttributedStringValueConverter : MvxValueConverter<QuerySymbolSuggestion, NSAttributedString>
        {
            protected override NSAttributedString Convert(QuerySymbolSuggestion value, Type targetType, object parameter, CultureInfo culture)
            {
                var a = $"{value.Symbol} {value.Description}";
                var result = new NSMutableAttributedString(a);
                result.AddAttributes(new UIStringAttributes
                {
                    Font = UIFont.BoldSystemFontOfSize(16),
                    ForegroundColor = Color.StartTimeEntry.BoldQuerySuggestionColor.ToNativeColor()
                }, new NSRange(0, 1));

                result.AddAttributes(new UIStringAttributes
                {
                    Font = UIFont.SystemFontOfSize(13),
                    ForegroundColor = Color.StartTimeEntry.Placeholder.ToNativeColor()
                }, new NSRange(2, value.Description.Length));

                return result;
            }
        }
    }
}
