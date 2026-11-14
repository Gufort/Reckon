using ProgramTree;

namespace SimpleLang.Visitors
{
    public class AutoVisitor : Visitor
    {
        public override void VisitProgramNode(ProgramNode n)
        {
            foreach (var def in n.Defs)
                def.Visit(this);
        }
        public override void VisitCellDefNode(CellDefNode n)
        {
            n.Lhs?.Visit(this);
            n.Rhs?.Visit(this);
        }
        public override void VisitNumberNode(NumberNode n) { }
        public override void VisitStringNode(StringNode n) { }
        public override void VisitBoolNode(BoolNode n) { }
        public override void VisitErrorNode(ErrorNode n) { }
        public override void VisitCellRefNode(CellRefNode n) { }
        public override void VisitColRefNode(ColRefNode n) { }
        public override void VisitRowRefNode(RowRefNode n) { }
        public override void VisitNameNode(NameNode n) { }
        public override void VisitRangeNode(RangeNode n)
        {
            n.From?.Visit(this);
            n.To?.Visit(this);
        }
        public override void VisitSheetRefNode(SheetRefNode n)
        {
            n.Cell?.Visit(this);
        }
        public override void VisitCallNode(CallNode n)
        {
            foreach (var arg in n.Args)
                arg?.Visit(this);
        }
        public override void VisitBinOpNode(BinOpNode n)
        {
            n.Left?.Visit(this);
            n.Right?.Visit(this);
        }
        public override void VisitUnaryOpNode(UnaryOpNode n)
        {
            n.Operand?.Visit(this);
        }
        public override void VisitLambdaNode(LambdaNode n)
        {
            n.Body?.Visit(this);
        }
    }
}