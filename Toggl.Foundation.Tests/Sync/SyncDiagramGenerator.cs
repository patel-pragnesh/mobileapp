using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Text;
using FsCheck;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Sync;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Xunit;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class SyncDiagramGenerator
    {
        class Configurator : ITransitionConfigurator
        {
            public Dictionary<IStateResult, (object State, Type ParameterType)> Transitions { get; }
                = new Dictionary<IStateResult, (object, Type)>();

            public void ConfigureTransition(IStateResult result, ISyncState state)
            {
                Transitions.Add(result, (state, null));
            }

            public void ConfigureTransition<T>(StateResult<T> result, ISyncState<T> state)
            {
                Transitions.Add(result, (state, typeof(T)));
            }
        }

        class Node
        {
            public string Id { get; set; }
            public string Label { get; set; }
        }

        class Edge
        {
            public string Label { get; set; }
            public Node From { get; set; }
            public Node To { get; set; }
        }

        [Fact, LogIfTooSlow]
        public void GenerateDOTFile()
        {
            var entryPoints = new StateMachineEntryPoints();
            var configurator = new Configurator();
            configureTransitions(configurator, entryPoints);

            var allStates = getAllStates(configurator);
            var allStateResults = getAllStateResultsByState(allStates);

            var stateNodes = makeNodesForStates(allStates);

            var edges = getEdgesBetweenStates(allStateResults, configurator, stateNodes);

            var nodes = stateNodes.Values.ToList();

            addEntryPoints(edges, nodes, entryPoints, configurator, stateNodes);

            addDeadEnds(edges, nodes, allStateResults, configurator, stateNodes);

            idNodes(nodes);

            var fileContent = writeDotFile(nodes, edges);

            File.WriteAllText("sync-graph.gv", fileContent);
        }

        private string writeDotFile(List<Node> nodes, List<Edge> edges)
        {
            var builder = new StringBuilder();

            builder.AppendLine("digraph");

            foreach (var node in nodes)
            {
                builder.AppendLine($"{node.Id} [label={node.Label}];");
            }

            foreach (var edge in edges)
            {
                builder.AppendLine($"{edge.From.Id} -> {edge.To.Id} [label={edge.Label}];");
            }

            return builder.ToString();
        }

        private void idNodes(List<Node> nodes)
        {
            string previousLabel = null;
            var i = 0;
            foreach (var node in nodes.OrderBy(n => n.Label))
            {
                if (node.Label != previousLabel)
                {
                    node.Id = node.Label;
                    i = 0;
                }
                else
                {
                    i++;
                    node.Id = node.Label + i;
                }
                previousLabel = node.Label;
            }
        }

        private void addDeadEnds(List<Edge> edges, List<Node> nodes,
            List<(object State, List<(IStateResult Result, string Name)> StateResults)> allStateResults, Configurator configurator,
            Dictionary<object, Node> stateNodes)
        {
            foreach (var (state, result) in allStateResults
                .SelectMany(results => results.StateResults
                    .Where(r => !configurator.Transitions.ContainsKey(r.Result))
                    .Select(r => (results.State, r))))
            {
                var node = new Node
                {
                    Label = "Dead End"
                };
                nodes.Add(node);

                var edge = new Edge
                {
                    From = stateNodes[state],
                    To = node,
                    Label = result.Name
                };
                edges.Add(edge);
            }
        }

        private void addEntryPoints(List<Edge> edges, List<Node> nodes, StateMachineEntryPoints entryPoints,
            Configurator configurator, Dictionary<object, Node> stateNodes)
        {
            foreach (var (property, stateResult) in entryPoints.GetType()
                .GetProperties()
                .Where(isStateResultProperty)
                .Select(p => (p, (IStateResult)p.GetValue(entryPoints))))
            {
                var node = new Node
                {
                    Label = property.Name
                };
                nodes.Add(node);

                if (configurator.Transitions.TryGetValue(stateResult, out var state))
                {
                    var edge = new Edge
                    {
                        From = node,
                        To = stateNodes[state.State],
                        Label = ""
                    };
                    edges.Add(edge);
                }
            }


        }

        private List<Edge> getEdgesBetweenStates(
            List<(object State, List<(IStateResult Result, string Name)> StateResults)> allStateResults,
            Configurator configurator, Dictionary<object, Node> stateNodes)
        {
            return allStateResults
                .SelectMany(results =>
                    results.StateResults
                        .Where(sr => configurator.Transitions.ContainsKey(sr.Result))
                        .Select(sr => edge(results.State, configurator.Transitions[sr.Result], stateNodes, sr.Name))
                )
                .ToList();
        }

        private Edge edge(object fromState, (object State, Type ParameterType) transition,
            Dictionary<object, Node> stateNodes, string name)
        {
            return new Edge
            {
                From = stateNodes[fromState],
                To = stateNodes[transition.State],
                Label = transition.ParameterType == null
                    ? name
                    : $"{name}<{transition.ParameterType.Name}>"
            };
        }

        private Dictionary<object, Node> makeNodesForStates(List<object> allStates)
        {
            return allStates.ToDictionary(s => s,
                s => new Node
                {
                    Label = s.GetType().Name
                });
        }

        private static List<(object State, List<(IStateResult Result, string Name)> StateResults)> getAllStateResultsByState(
            List<object> allStates)
        {
            return allStates
                .Select(state => (state, state.GetType()
                    .GetProperties()
                    .Where(isStateResultProperty)
                    .Select(p => ((IStateResult)p.GetValue(state), p.Name))
                    .ToList())
                ).ToList();
        }

        private static bool isStateResultProperty(PropertyInfo p)
        {
            return typeof(IStateResult).IsAssignableFrom(p.PropertyType);
        }

        private static List<object> getAllStates(Configurator configurator)
        {
            return configurator.Transitions.Values
                .Select(t => t.State)
                .Distinct()
                .ToList();
        }

        private static void configureTransitions(Configurator configurator, StateMachineEntryPoints entryPoints)
        {
            TogglSyncManager.ConfigureTransitions(
                configurator,
                Substitute.For<ITogglDatabase>(),
                Substitute.For<ITogglApi>(),
                Substitute.For<ITogglDataSource>(),
                Substitute.For<IRetryDelayService>(),
                Substitute.For<IScheduler>(),
                Substitute.For<ITimeService>(),
                entryPoints,
                Substitute.For<IObservable<Unit>>()
            );
        }
    }
}
