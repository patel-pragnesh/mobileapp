namespace SyncDiagramGenerator
{
    internal sealed class Node
    {
        public enum NodeType
        {
            Regular = 0,
            EntryPoint = 1,
            DeadEnd = 2,
            InvalidTransitionState = 3,
        }

        public string Id { get; set; }
        public string Label { get; set; }
        public NodeType Type { get; set; }
    }
}
