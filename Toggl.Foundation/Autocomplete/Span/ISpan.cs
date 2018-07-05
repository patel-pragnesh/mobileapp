using System;
namespace Toggl.Foundation.Autocomplete.Span
{
    public interface ISpan
    {
    }

    public static class SpanExtensions
    {
        public static bool IsTextSpan(this ISpan span) => span is TextSpan;
    }
}
