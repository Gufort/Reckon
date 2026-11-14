using System.Collections.Generic;
using System.Linq;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class ReferenceChecker : AutoVisitor
    {
        private HashSet<string> definedCells;
        private List<string> undefinedReferences;
        private string currentCell;

        public List<string> UndefinedReferences { get; private set; }

        public void Analyze(ProgramNode program)
        {
            definedCells = new HashSet<string>();
            undefinedReferences = new List<string>();

            foreach (var def in program.Defs)
            {
                definedCells.Add(GetCellName(def.Lhs));
            }

            foreach (var def in program.Defs)
            {
                currentCell = GetCellName(def.Lhs);
                def.Rhs?.Visit(this);
            }

            UndefinedReferences = undefinedReferences;
        }

        public override void VisitCellRefNode(CellRefNode n)
        {
            if (!definedCells.Contains(n.Address))
                undefinedReferences.Add(n.Address);
        }

        public override void VisitNameNode(NameNode n)
        {
            if (!definedCells.Contains(n.Name))
                undefinedReferences.Add(n.Name);
        }

        private string GetCellName(Node lhs)
        {
            if (lhs is CellRefNode cr) return cr.Address;
            if (lhs is NameNode nn) return nn.Name;
            return "";
        }

        public bool HasUndefinedReferences()
        {
            return undefinedReferences != null && undefinedReferences.Count > 0;
        }
    }
}