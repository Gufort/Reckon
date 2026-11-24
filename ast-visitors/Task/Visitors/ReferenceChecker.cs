using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    /// <summary>
    /// Проверяет, что все ячейки и именованные формулы, на которые ссылаются
    /// определения, объявлены в программе.
    /// </summary>
    public class ReferenceChecker : AutoVisitor
    {
        public List<string> UndefinedReferences { get; private set; } = new List<string>();

        private readonly HashSet<string> defined = new HashSet<string>();
        private readonly HashSet<string> reported = new HashSet<string>();
        private readonly Stack<HashSet<string>> parameterScopes = new Stack<HashSet<string>>();

        public void Analyze(ProgramNode program)
        {
            UndefinedReferences = new List<string>();
            reported.Clear();
            defined.Clear();
            parameterScopes.Clear();

            if (program == null)
                return;

            if (program.Defs != null)
                foreach (var definition in program.Defs)
                {
                    string name = ReckonNames.GetDefinitionName(definition.Lhs);
                    if (name != null)
                        defined.Add(name);
                }

            program.Visit(this);
        }

        public bool HasUndefinedReferences()
        {
            return UndefinedReferences != null && UndefinedReferences.Count > 0;
        }

        public override void VisitCellDefNode(CellDefNode n)
        {
            // Левая часть определения — это объявление, а не ссылка.
            n.Rhs?.Visit(this);
        }

        public override void VisitLambdaNode(LambdaNode n)
        {
            parameterScopes.Push(new HashSet<string>(n.Params ?? new List<string>()));
            n.Body?.Visit(this);
            parameterScopes.Pop();
        }

        public override void VisitCellRefNode(CellRefNode n)
        {
            Check(ReckonNames.NormalizeAddress(n.Address));
        }

        public override void VisitNameNode(NameNode n)
        {
            if (!IsParameter(n.Name))
                Check(n.Name);
        }

        public override void VisitRangeNode(RangeNode n)
        {
            if (n.From == null || n.To == null)
                return;

            foreach (var address in ReckonNames.ExpandRange(n.From.Address, n.To.Address))
                Check(address);
        }

        public override void VisitSheetRefNode(SheetRefNode n)
        {
            // Ссылки на ячейки других листов нельзя проверить в рамках одной книги.
        }

        public override void VisitCallNode(CallNode n)
        {
            if (!ReckonNames.IsBuiltinFunction(n.Name))
                Check(n.Name);
            base.VisitCallNode(n);
        }

        private void Check(string name)
        {
            if (string.IsNullOrEmpty(name) || defined.Contains(name))
                return;
            if (reported.Add(name))
                UndefinedReferences.Add(name);
        }

        private bool IsParameter(string name)
        {
            foreach (var scope in parameterScopes)
                if (scope.Contains(name))
                    return true;
            return false;
        }
    }
}
