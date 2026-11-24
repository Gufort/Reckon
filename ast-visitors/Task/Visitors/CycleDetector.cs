using System;
using System.Collections.Generic;
using System.Linq;
using ProgramTree;

namespace SimpleLang.Visitors
{
    /// <summary>
    /// Строит граф зависимостей ячеек и именованных формул и находит в нём циклы.
    /// </summary>
    public class CycleDetector : AutoVisitor
    {
        private const byte White = 0;
        private const byte Gray = 1;

        public List<List<string>> Cycles { get; private set; } = new List<List<string>>();

        public void Analyze(ProgramNode program)
        {
            Cycles = new List<List<string>>();
            if (program == null)
                return;

            var extractor = new DependencyExtractor();
            program.Visit(extractor);
            var graph = extractor.Dependencies;

            var state = new Dictionary<string, byte>();
            var path = new List<string>();
            var reportedCycles = new HashSet<string>();

            foreach (var node in GetNodes(graph))
                if (!state.ContainsKey(node))
                    VisitNode(node, graph, state, path, reportedCycles);
        }

        public bool HasCycles()
        {
            return Cycles != null && Cycles.Count > 0;
        }

        private void VisitNode(string node, Dictionary<string, HashSet<string>> graph,
            Dictionary<string, byte> state, List<string> path, HashSet<string> reportedCycles)
        {
            state[node] = Gray;
            path.Add(node);

            HashSet<string> dependencies;
            if (graph.TryGetValue(node, out dependencies))
            {
                foreach (var dependency in dependencies.OrderBy(d => d, StringComparer.Ordinal))
                {
                    byte color;
                    if (!state.TryGetValue(dependency, out color))
                        color = White;

                    if (color == White)
                        VisitNode(dependency, graph, state, path, reportedCycles);
                    else if (color == Gray)
                        ReportCycle(path, dependency, reportedCycles);
                }
            }

            path.RemoveAt(path.Count - 1);
            state[node] = 2;
        }

        private static IEnumerable<string> GetNodes(Dictionary<string, HashSet<string>> graph)
        {
            var nodes = new HashSet<string>(graph.Keys);
            foreach (var dependencies in graph.Values)
                nodes.UnionWith(dependencies);
            return nodes.OrderBy(n => n, StringComparer.Ordinal);
        }

        private void ReportCycle(List<string> path, string start, HashSet<string> reportedCycles)
        {
            int startIndex = path.IndexOf(start);
            if (startIndex < 0)
                return;

            var cycle = path.Skip(startIndex).ToList();
            if (reportedCycles.Add(GetCycleKey(cycle)))
                Cycles.Add(cycle);
        }

        /// <summary>
        /// Цикл A B C и его повороты B C A, C A B — это один и тот же цикл.
        /// </summary>
        private static string GetCycleKey(List<string> cycle)
        {
            int first = 0;
            for (int i = 1; i < cycle.Count; i++)
                if (string.CompareOrdinal(cycle[i], cycle[first]) < 0)
                    first = i;

            return string.Join(" -> ", Enumerable.Range(0, cycle.Count)
                .Select(i => cycle[(first + i) % cycle.Count]));
        }
    }
}
