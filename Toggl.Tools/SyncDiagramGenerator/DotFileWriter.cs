using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SyncDiagramGenerator
{
    internal sealed class DotFileWriter
    {
        public void WriteToFile(string outPath, List<Node> nodes, List<Edge> edges)
        {
            Console.WriteLine($"Assigning unique ids to {nodes.Count} nodes");

            assignUniqueIdsTo(nodes);

            Console.WriteLine($"Serialising {nodes.Count} nodes and {edges.Count} edges to DOT format");

            var fileContent = serialise(nodes, edges);

            Console.WriteLine($"Writing DOT file to {outPath}");

            File.WriteAllText(outPath, fileContent);
        }

        private void assignUniqueIdsTo(List<Node> nodes)
        {
            string previousId = null;
            var i = 0;
            foreach (var node in nodes.OrderBy(n => n.Label))
            {
                var id = node.Label;
                if (id != previousId)
                {
                    node.Id = $"\"{id}\"";
                    i = 0;
                }
                else
                {
                    i++;
                    node.Id = $"\"{id + i}\"";
                }

                previousId = id;
            }
        }

        private string serialise(List<Node> nodes, List<Edge> edges)
        {
            var builder = new StringBuilder();

            builder.AppendLine("digraph SyncGraph {");

            foreach (var node in nodes)
            {
                var nodeAttributes = getAttributes(node);
                var attributeString = string.Join(",", nodeAttributes.Select(a => $"{a.Key}=\"{a.Value}\""));
                builder.AppendLine($"{node.Id} [{attributeString}];");
            }

            foreach (var edge in edges)
            {
                builder.AppendLine($"{edge.From.Id} -> {edge.To.Id} [label=\"{edge.Label}\"];");
            }

            builder.AppendLine("}");

            return builder.ToString();
        }

        private List<(string Key, string Value)> getAttributes(Node node)
        {
            var attributes = new List<(string, string)>
            {
                ("label", node.Label)
            };

            switch (node.Type)
            {
                case Node.NodeType.EntryPoint:
                    attributes.Add(("color", "green"));
                    break;
                case Node.NodeType.DeadEnd:
                    attributes.Add(("color", "orange"));
                    break;
                case Node.NodeType.InvalidTransitionState:
                    attributes.Add(("color", "red"));
                    break;
            }

            return attributes;
        }
    }
}
