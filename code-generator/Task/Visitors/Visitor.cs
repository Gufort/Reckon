using ProgramTree;

namespace SimpleLang.Visitors
{
    public abstract class Visitor
    {
        public virtual void VisitProgramNode(ProgramNode n) { }
        public virtual void VisitCellDefNode(CellDefNode n) { }
        public virtual void VisitNumberNode(NumberNode n) { }
        public virtual void VisitStringNode(StringNode n) { }
        public virtual void VisitBoolNode(BoolNode n) { }
        public virtual void VisitErrorNode(ErrorNode n) { }
        public virtual void VisitCellRefNode(CellRefNode n) { }
        public virtual void VisitColRefNode(ColRefNode n) { }
        public virtual void VisitRowRefNode(RowRefNode n) { }
        public virtual void VisitNameNode(NameNode n) { }
        public virtual void VisitRangeNode(RangeNode n) { }
        public virtual void VisitSheetRefNode(SheetRefNode n) { }
        public virtual void VisitCallNode(CallNode n) { }
        public virtual void VisitBinOpNode(BinOpNode n) { }
        public virtual void VisitUnaryOpNode(UnaryOpNode n) { }
        public virtual void VisitLambdaNode(LambdaNode n) { }
    }
}