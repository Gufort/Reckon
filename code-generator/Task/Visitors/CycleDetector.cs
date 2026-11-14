using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class CycleDetector : AutoVisitor
    {
        private Dictionary<string, HashSet<string>> deps;
        private HashSet<string> visited;
        private HashSet<string> inStack;
        private List<List<string>> cycles;

        public List<List<string>> Cycles { get; private set; }

        public void Analyze(ProgramNode program)
        {
            var ext = new DependencyExtractor();
            program.Visit(ext);
            deps = ext.Dependencies;
            visited = new HashSet<string>();
            inStack = new HashSet<string>();
            cycles = new List<List<string>>();

            foreach (var cell in deps.Keys)
            {
                if (!visited.Contains(cell))
                {
                    var path = new List<string>();
                    Dfs(cell, path);
                }
            }
            Cycles = cycles;
        }

        private void Dfs(string cell, List<string> path)
        {
            visited.Add(cell);
            inStack.Add(cell);
            path.Add(cell);

            if (deps.ContainsKey(cell))
            {
                foreach (var dep in deps[cell])
                {
                    if (inStack.Contains(dep))
                    {
                        var cycle = new List<string>();
                        int idx = path.IndexOf(dep);
                        for (int i = idx; i < path.Count; i++)
                            cycle.Add(path[i]);
                        cycles.Add(cycle);
                    }
                    else if (!visited.Contains(dep))
                    {
                        Dfs(dep, path);
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
            inStack.Remove(cell);
        }

        public bool HasCycles()
        {
            return cycles != null && cycles.Count > 0;
        }
    }
}