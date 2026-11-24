using System;
using ProgramTree;

namespace SimpleLang.Visitors
{
    public class ConstantFolder : AutoVisitor
    {
        public bool Fold(ref ExprNode node)
        {
            if (node == null)
                return false;

            var binOp = node as BinOpNode;
            if (binOp != null)
            {
                var left = binOp.Left;
                var right = binOp.Right;
                Fold(ref left);
                Fold(ref right);
                binOp.Left = left;
                binOp.Right = right;

                var leftNumber = left as NumberNode;
                var rightNumber = right as NumberNode;
                double value;
                if (leftNumber != null && rightNumber != null &&
                    TryEvaluate(binOp.Op, leftNumber.Value, rightNumber.Value, out value))
                {
                    node = new NumberNode(value);
                    return true;
                }

                return false;
            }

            var unaryOp = node as UnaryOpNode;
            if (unaryOp != null)
            {
                var operand = unaryOp.Operand;
                Fold(ref operand);
                unaryOp.Operand = operand;

                var operandNumber = operand as NumberNode;
                if (operandNumber != null)
                {
                    if (unaryOp.Op == "-")
                    {
                        node = new NumberNode(-operandNumber.Value);
                        return true;
                    }
                    if (unaryOp.Op == "+")
                    {
                        node = new NumberNode(operandNumber.Value);
                        return true;
                    }
                }

                return false;
            }

            var lambda = node as LambdaNode;
            if (lambda != null)
            {
                var body = lambda.Body;
                Fold(ref body);
                lambda.Body = body;
                return false;
            }

            return node is NumberNode;
        }

        public override void VisitCellDefNode(CellDefNode n)
        {
            var rhs = n.Rhs;
            Fold(ref rhs);
            n.Rhs = rhs;
        }

        private static bool TryEvaluate(string op, double left, double right, out double result)
        {
            switch (op)
            {
                case "+":
                    result = left + right;
                    return true;
                case "-":
                    result = left - right;
                    return true;
                case "*":
                    result = left * right;
                    return true;
                case "/":
                    // Деление на ноль не сворачиваем: результат не является числом.
                    if (right == 0)
                        break;
                    result = left / right;
                    return true;
                case "^":
                    result = Math.Pow(left, right);
                    return true;
            }

            result = 0;
            return false;
        }
    }
}
