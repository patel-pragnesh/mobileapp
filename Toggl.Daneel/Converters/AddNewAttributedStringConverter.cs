using System;
using System.Globalization;
using Foundation;
using MvvmCross.Converters;
using Toggl.Daneel.Extensions;

namespace Toggl.Foundation.MvvmCross.Converters
{
    public sealed class AddNewAttributedStringConverter : MvxValueConverter<string, NSAttributedString>
    {
        private readonly NSAttributedString emptyString;

        public AddNewAttributedStringConverter(string emptyText, double fontHeight)
        {
            emptyString = emptyText.PrependWithAddIcon(fontHeight);
        }

        protected override NSAttributedString Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value))
                return emptyString;

            return new NSAttributedString(value);
        }
    }
}
