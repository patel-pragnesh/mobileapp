using System;
namespace Toggl.Foundation.Autocomplete.Span
{
    public class TextSpan : ISpan
    {
        public string Text { get; }

        public TextSpan(string text)
        {
            Text = text;
        }
    }
}
