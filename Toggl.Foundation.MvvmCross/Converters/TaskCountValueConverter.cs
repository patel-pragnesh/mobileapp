using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public sealed class TaskCountValueConverter : MvxValueConverter<int, string>
    {
        protected override string Convert(int value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == 0)
                return "";

            return $"{value} Task{(value == 1 ? "" : "s")}";
        }
    }
}
