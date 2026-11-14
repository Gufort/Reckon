using System.Collections.Generic;
using SimpleLang.Visitors;

namespace ProgramTree
{
    public abstract class Node
    {
        public abstract void Visit(Visitor v);
    }

    public class ProgramNode : Node
    {
        public List<CellDefNode> Defs { get; set; }
        public ProgramNode(List<CellDefNode> defs) { Defs = defs; }
        public override void Visit(Visitor v) { v.VisitProgramNode(this); }
    }

    public class CellDefNode : Node
    {
        public Node Lhs { get; set; }
        public ExprNode Rhs { get; set; }
        public CellDefNode(Node lhs, ExprNode rhs) { Lhs = lhs; Rhs = rhs; }
        public override void Visit(Visitor v) { v.VisitCellDefNode(this); }
    }

    public abstract class ExprNode : Node
    {
    }

    public class NumberNode : ExprNode
    {
        public double Value { get; set; }
        public NumberNode(double value) { Value = value; }
        public NumberNode(int value) { Value = value; }
        public override void Visit(Visitor v) { v.VisitNumberNode(this); }
    }

    public class StringNode : ExprNode
    {
        public string Value { get; set; }
        public StringNode(string value) { Value = value; }
        public override void Visit(Visitor v) { v.VisitStringNode(this); }
    }

    public class BoolNode : ExprNode
    {
        public bool Value { get; set; }
        public BoolNode(bool value) { Value = value; }
        public override void Visit(Visitor v) { v.VisitBoolNode(this); }
    }

    public class ErrorNode : ExprNode
    {
        public string Code { get; set; }
        public ErrorNode(string code) { Code = code; }
        public override void Visit(Visitor v) { v.VisitErrorNode(this); }
    }

    public class CellRefNode : ExprNode
    {
        public string Address { get; set; }
        public CellRefNode(string address) { Address = address; }
        public override void Visit(Visitor v) { v.VisitCellRefNode(this); }
    }

    public class ColRefNode : ExprNode
    {
        public string Column { get; set; }
        public ColRefNode(string column) { Column = column; }
        public override void Visit(Visitor v) { v.VisitColRefNode(this); }
    }

    public class RowRefNode : ExprNode
    {
        public string Row { get; set; }
        public RowRefNode(string row) { Row = row; }
        public override void Visit(Visitor v) { v.VisitRowRefNode(this); }
    }

    public class NameNode : ExprNode
    {
        public string Name { get; set; }
        public NameNode(string name) { Name = name; }
        public override void Visit(Visitor v) { v.VisitNameNode(this); }
    }

    public class RangeNode : ExprNode
    {
        public CellRefNode From { get; set; }
        public CellRefNode To { get; set; }
        public RangeNode(string from, string to)
        {
            From = new CellRefNode(from);
            To = new CellRefNode(to);
        }
        public RangeNode(CellRefNode from, CellRefNode to) { From = from; To = to; }
        public override void Visit(Visitor v) { v.VisitRangeNode(this); }
    }

    public class SheetRefNode : ExprNode
    {
        public string Sheet { get; set; }
        public CellRefNode Cell { get; set; }
        public SheetRefNode(string sheet, string cell)
        {
            Sheet = sheet;
            Cell = new CellRefNode(cell);
        }
        public SheetRefNode(string sheet, CellRefNode cell) { Sheet = sheet; Cell = cell; }
        public override void Visit(Visitor v) { v.VisitSheetRefNode(this); }
    }

    public class CallNode : ExprNode
    {
        public string Name { get; set; }
        public List<ExprNode> Args { get; set; }
        public CallNode(string name, List<ExprNode> args) { Name = name; Args = args; }
        public override void Visit(Visitor v) { v.VisitCallNode(this); }
    }

    public class BinOpNode : ExprNode
    {
        public ExprNode Left { get; set; }
        public ExprNode Right { get; set; }
        public string Op { get; set; }
        public BinOpNode(ExprNode left, ExprNode right, string op) { Left = left; Right = right; Op = op; }
        public override void Visit(Visitor v) { v.VisitBinOpNode(this); }
    }

    public class UnaryOpNode : ExprNode
    {
        public ExprNode Operand { get; set; }
        public string Op { get; set; }
        public UnaryOpNode(ExprNode operand, string op) { Operand = operand; Op = op; }
        public override void Visit(Visitor v) { v.VisitUnaryOpNode(this); }
    }

    public class LambdaNode : ExprNode
    {
        public List<string> Params { get; set; }
        public ExprNode Body { get; set; }
        public LambdaNode(List<string> pars, ExprNode body) { Params = pars; Body = body; }
        public override void Visit(Visitor v) { v.VisitLambdaNode(this); }
    }
}