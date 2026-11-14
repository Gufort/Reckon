using System;
using System.Collections.Generic;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class DependencyExtractor : AutoVisitor
    {
        public Dictionary<string, HashSet<string>> Dependencies
        {
            get { throw new NotImplementedException(); }
        }

        public override void VisitCellDefNode(CellDefNode n)
        {
            throw new NotImplementedException();
        }

        public override void VisitCellRefNode(CellRefNode n)
        {
        }

        public override void VisitNameNode(NameNode n)
        {
        }

        public override void VisitRangeNode(RangeNode n)
        {
        }

        public HashSet<string> GetDependencies(string cell)
        {
            throw new NotImplementedException();
        }
    }
}