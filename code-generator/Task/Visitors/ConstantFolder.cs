using ProgramTree;

namespace SimpleLang.Visitors
{
    public class ConstantFolder : AutoVisitor
    {
        private bool changed;

        public bool Fold(ref ExprNode node)
        {
            changed = false;
            node.Visit(this);
            return changed;
        }

        public override void VisitBinOpNode(BinOpNode n)
        {
            n.Left?.Visit(this);
            n.Right?.Visit(this);

            if (n.Left is NumberNode ln && n.Right is NumberNode rn)
            {
                double result = n.Op switch
                {
                    "+" => ln.Value + rn.Value,
                    "-" => ln.Value - rn.Value,
                    "*" => ln.Value * rn.Value,
                    "/" => ln.Value / rn.Value,
                    "^" => System.Math.Pow(ln.Value, rn.Value),
                    "=" => ln.Value == rn.Value ? 1.0 : 0.0,
                    "<>" => ln.Value != rn.Value ? 1.0 : 0.0,
                    "<" => ln.Value < rn.Value ? 1.0 : 0.0,
                    ">" => ln.Value > rn.Value ? 1.0 : 0.0,
                    "<=" => ln.Value <= rn.Value ? 1.0 : 0.0,
                    ">=" => ln.Value >= rn.Value ? 1.0 : 0.0,
                    _ => double.NaN
                };
            }
        }

        public override void VisitUnaryOpNode(UnaryOpNode n)
        {
            n.Operand?.Visit(this);
        }
    }
}