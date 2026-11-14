using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class DependencyExtractor : AutoVisitor
    {
        public Dictionary<string, HashSet<string>> Dependencies { get; } = new Dictionary<string, HashSet<string>>();
        private string currentCell;
        private HashSet<string> currentDeps;

        public override void VisitCellDefNode(CellDefNode n)
        {
            currentCell = GetCellName(n.Lhs);
            currentDeps = new HashSet<string>();
            n.Rhs?.Visit(this);
            Dependencies[currentCell] = currentDeps;
        }

        public override void VisitCellRefNode(CellRefNode n)
        {
            currentDeps.Add(n.Address);
        }

        public override void VisitNameNode(NameNode n)
        {
            currentDeps.Add(n.Name);
        }

        public override void VisitRangeNode(RangeNode n)
        {
            currentDeps.Add(n.From.Address);
            currentDeps.Add(n.To.Address);
        }

        private string GetCellName(Node lhs)
        {
            if (lhs is CellRefNode cr) return cr.Address;
            if (lhs is NameNode nn) return nn.Name;
            return "";
        }

        public HashSet<string> GetDependencies(string cell)
        {
            return Dependencies.TryGetValue(cell, out var deps) ? deps : new HashSet<string>();
        }
    }
}