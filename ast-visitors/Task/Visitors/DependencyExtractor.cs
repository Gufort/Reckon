using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    /// <summary>
    /// Для каждого определения ячейки или именованной формулы собирает
    /// множество адресов, от которых это определение зависит.
    /// </summary>
    public class DependencyExtractor : AutoVisitor
    {
        public Dictionary<string, HashSet<string>> Dependencies { get; } = new Dictionary<string, HashSet<string>>();

        private HashSet<string> currentDependencies;
        private readonly Stack<HashSet<string>> parameterScopes = new Stack<HashSet<string>>();

        public HashSet<string> GetDependencies(string cell)
        {
            HashSet<string> dependencies;
            if (cell != null && Dependencies.TryGetValue(cell, out dependencies))
                return dependencies;
            return new HashSet<string>();
        }

        public override void VisitProgramNode(ProgramNode n)
        {
            Dependencies.Clear();
            currentDependencies = null;
            parameterScopes.Clear();
            base.VisitProgramNode(n);
        }

        public override void VisitCellDefNode(CellDefNode n)
        {
            string name = ReckonNames.GetDefinitionName(n.Lhs);
            if (name == null)
            {
                base.VisitCellDefNode(n);
                return;
            }

            var dependencies = new HashSet<string>();
            var savedDependencies = currentDependencies;
            currentDependencies = dependencies;
            n.Rhs?.Visit(this);
            currentDependencies = savedDependencies;

            Dependencies[name] = dependencies;
        }

        public override void VisitCellRefNode(CellRefNode n)
        {
            AddDependency(ReckonNames.NormalizeAddress(n.Address));
        }

        public override void VisitNameNode(NameNode n)
        {
            AddDependency(n.Name);
        }

        public override void VisitRangeNode(RangeNode n)
        {
            if (n.From == null || n.To == null)
                return;

            foreach (var address in ReckonNames.ExpandRange(n.From.Address, n.To.Address))
                AddDependency(address);
        }

        public override void VisitSheetRefNode(SheetRefNode n)
        {
            if (n.Cell == null)
                return;

            AddDependency(n.Sheet + "!" + ReckonNames.NormalizeAddress(n.Cell.Address));
        }

        public override void VisitCallNode(CallNode n)
        {
            if (!ReckonNames.IsBuiltinFunction(n.Name))
                AddDependency(n.Name);
            base.VisitCallNode(n);
        }

        public override void VisitLambdaNode(LambdaNode n)
        {
            parameterScopes.Push(new HashSet<string>(n.Params ?? new List<string>()));
            n.Body?.Visit(this);
            parameterScopes.Pop();
        }

        private void AddDependency(string name)
        {
            if (currentDependencies == null || string.IsNullOrEmpty(name))
                return;

            foreach (var scope in parameterScopes)
                if (scope.Contains(name))
                    return;

            currentDependencies.Add(name);
        }
    }
}
