namespace Toggl.Giskard.Extensions
{
    public struct PopupOffsets
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
