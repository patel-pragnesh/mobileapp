namespace Toggl.Giskard.Extensions
{
    public class PopupOffsets
    {
        public int HorizontalOffset { get; }
        public int VerticalOffset { get; }

        public PopupOffsets(int horizontalOffset, int verticalOffset)
        {
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
        }
    }
}