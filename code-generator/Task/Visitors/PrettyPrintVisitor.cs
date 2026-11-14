using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class PrettyPrintVisitor : Visitor
    {
        public string Text = "";
        private int Indent = 0;

        private string IndentStr() { return new string(' ', Indent); }
        private void IndentPlus() { Indent += 2; }
        private void IndentMinus() { Indent -= 2; }

        public override void VisitProgramNode(ProgramNode n)
        {
            foreach (var def in n.Defs)
            {
                def.Visit(this);
                Text += Environment.NewLine;
            }
        }
        public override void VisitCellDefNode(CellDefNode n)
        {
            Text += IndentStr();
            n.Lhs?.Visit(this);
            Text += " := ";
            n.Rhs?.Visit(this);
        }
        public override void VisitNumberNode(NumberNode n)
        {
            Text += n.Value.ToString();
        }
        public override void VisitStringNode(StringNode n)
        {
            Text += "\"" + n.Value + "\"";
        }
        public override void VisitBoolNode(BoolNode n)
        {
            Text += n.Value ? "true" : "false";
        }
        public override void VisitErrorNode(ErrorNode n)
        {
            Text += n.Code;
        }
        public override void VisitCellRefNode(CellRefNode n)
        {
            Text += n.Address;
        }
        public override void VisitColRefNode(ColRefNode n)
        {
            Text += n.Column;
        }
        public override void VisitRowRefNode(RowRefNode n)
        {
            Text += n.Row;
        }
        public override void VisitNameNode(NameNode n)
        {
            Text += n.Name;
        }
        public override void VisitRangeNode(RangeNode n)
        {
            n.From?.Visit(this);
            Text += ":";
            n.To?.Visit(this);
        }
        public override void VisitSheetRefNode(SheetRefNode n)
        {
            Text += n.Sheet + "!";
            n.Cell?.Visit(this);
        }
        public override void VisitCallNode(CallNode n)
        {
            Text += n.Name + "(";
            for (int i = 0; i < n.Args.Count; i++)
            {
                if (i > 0) Text += ", ";
                n.Args[i]?.Visit(this);
            }
            Text += ")";
        }
        public override void VisitBinOpNode(BinOpNode n)
        {
            Text += "(";
            n.Left?.Visit(this);
            Text += " " + n.Op + " ";
            n.Right?.Visit(this);
            Text += ")";
        }
        public override void VisitUnaryOpNode(UnaryOpNode n)
        {
            Text += "(" + n.Op;
            n.Operand?.Visit(this);
            Text += ")";
        }
        public override void VisitLambdaNode(LambdaNode n)
        {
            if (n.Params.Count == 1)
            {
                Text += n.Params[0] + " -> ";
            }
            else
            {
                Text += "(";
                for (int i = 0; i < n.Params.Count; i++)
                {
                    if (i > 0) Text += ", ";
                    Text += n.Params[i];
                }
                Text += ") -> ";
            }
            n.Body?.Visit(this);
        }
    }
}