using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SimpleLexer;

namespace SimpleParser
{
    public class ParserException : System.Exception
    {
        public ParserException(string msg) : base(msg) { }
    }

    public enum DefinitionKind
    {
        Cell,
        Name,
        Formula
    }

    public class Definition
    {
        public DefinitionKind Kind { get; private set; }
        public string Target { get; private set; }
        public List<string> Parameters { get; private set; }
        public Expression Body { get; private set; }

        public Definition(DefinitionKind kind, string target, Expression body)
        {
            Kind = kind;
            Target = target;
            Parameters = new List<string>();
            Body = body;
        }
    }

    public abstract class Expression
    {
    }

    public class NumberExpression : Expression
    {
        public double Value { get; private set; }
        public string Text { get; private set; }

        public NumberExpression(double value, string text)
        {
            Value = value;
            Text = text;
        }
    }

    public class StringExpression : Expression
    {
        public string Value { get; private set; }

        public StringExpression(string value)
        {
            Value = value;
        }
    }

    public class BoolExpression : Expression
    {
        public bool Value { get; private set; }

        public BoolExpression(bool value)
        {
            Value = value;
        }
    }

    public class ErrorExpression : Expression
    {
        public string Text { get; private set; }

        public ErrorExpression(string text)
        {
            Text = text;
        }
    }

    public class CellRefExpression : Expression
    {
        public string Reference { get; private set; }

        public CellRefExpression(string reference)
        {
            Reference = reference;
        }
    }

    public class ColumnRefExpression : Expression
    {
        public string Reference { get; private set; }

        public ColumnRefExpression(string reference)
        {
            Reference = reference;
        }
    }

    public class RowRefExpression : Expression
    {
        public string Reference { get; private set; }

        public RowRefExpression(string reference)
        {
            Reference = reference;
        }
    }

    public class NameExpression : Expression
    {
        public string Name { get; private set; }

        public NameExpression(string name)
        {
            Name = name;
        }
    }

    public class UnaryExpression : Expression
    {
        public Tok Operator { get; private set; }
        public Expression Operand { get; private set; }

        public UnaryExpression(Tok op, Expression operand)
        {
            Operator = op;
            Operand = operand;
        }
    }

    public class BinaryExpression : Expression
    {
        public Tok Operator { get; private set; }
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }

        public BinaryExpression(Tok op, Expression left, Expression right)
        {
            Operator = op;
            Left = left;
            Right = right;
        }
    }

    public class CallExpression : Expression
    {
        public string Name { get; private set; }
        public List<Expression> Arguments { get; private set; }

        public CallExpression(string name, List<Expression> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }

    public class RangeExpression : Expression
    {
        public string Start { get; private set; }
        public string End { get; private set; }

        public RangeExpression(string start, string end)
        {
            Start = start;
            End = end;
        }
    }

    public class SheetRefExpression : Expression
    {
        public string Sheet { get; private set; }
        public string Reference { get; private set; }

        public SheetRefExpression(string sheet, string reference)
        {
            Sheet = sheet;
            Reference = reference;
        }
    }

    public class Parser
    {
        private Lexer l;

        public Parser(Lexer lexer)
        {
            l = lexer;
        }

        public List<Definition> Parse()
        {
            List<Definition> definitions = new List<Definition>();
            while (l.LexKind != Tok.EOF)
                definitions.Add(ParseDefinition());
            return definitions;
        }

        private Definition ParseDefinition()
        {
            if (l.LexKind == Tok.CELLREF)
            {
                string target = l.LexText;
                Next();
                Expect(Tok.ASSIGN, "':=' was expected");
                return new Definition(DefinitionKind.Cell, target, ParseExpr());
            }

            if (l.LexKind == Tok.NAME)
            {
                string target = l.LexText;
                Next();
                if (l.LexKind == Tok.LPAREN)
                {
                    List<string> parameters = ParseParams();
                    Expect(Tok.ASSIGN, "':=' was expected");
                    Definition definition = new Definition(DefinitionKind.Formula, target, ParseExpr());
                    definition.Parameters.AddRange(parameters);
                    return definition;
                }
                Expect(Tok.ASSIGN, "':=' was expected");
                return new Definition(DefinitionKind.Name, target, ParseExpr());
            }

            SyntaxError("a definition was expected");
            return null;
        }

        private List<string> ParseParams()
        {
            List<string> parameters = new List<string>();
            Expect(Tok.LPAREN, "'(' was expected");
            parameters.Add(Expect(Tok.NAME, "a parameter name was expected"));
            while (l.LexKind == Tok.COMMA)
            {
                Next();
                parameters.Add(Expect(Tok.NAME, "a parameter name was expected"));
            }
            Expect(Tok.RPAREN, "')' was expected");
            return parameters;
        }

        private Expression ParseExpr()
        {
            Expression left = ParseAdditive();
            while (IsComparison(l.LexKind))
            {
                Tok op = l.LexKind;
                Next();
                left = new BinaryExpression(op, left, ParseAdditive());
            }
            return left;
        }

        private Expression ParseAdditive()
        {
            Expression left = ParseMultiplicative();
            while (l.LexKind == Tok.PLUS || l.LexKind == Tok.MINUS || l.LexKind == Tok.CONCAT)
            {
                Tok op = l.LexKind;
                Next();
                left = new BinaryExpression(op, left, ParseMultiplicative());
            }
            return left;
        }

        private Expression ParseMultiplicative()
        {
            Expression left = ParsePower();
            while (l.LexKind == Tok.MULT || l.LexKind == Tok.DIV)
            {
                Tok op = l.LexKind;
                Next();
                left = new BinaryExpression(op, left, ParsePower());
            }
            return left;
        }

        private Expression ParsePower()
        {
            Expression left = ParsePrimary();
            while (l.LexKind == Tok.POW)
            {
                Tok op = l.LexKind;
                Next();
                left = new BinaryExpression(op, left, ParsePrimary());
            }
            return left;
        }

        private Expression ParsePrimary()
        {
            if (l.LexKind == Tok.MINUS || l.LexKind == Tok.PLUS)
            {
                Tok op = l.LexKind;
                Next();
                return new UnaryExpression(op, ParsePrimary());
            }
            return ParseAtom();
        }

        private Expression ParseAtom()
        {
            switch (l.LexKind)
            {
                case Tok.NUMBER:
                    {
                        string text = l.LexText;
                        Next();
                        return new NumberExpression(double.Parse(text, CultureInfo.InvariantCulture), text);
                    }
                case Tok.STRING:
                    {
                        string text = l.LexText;
                        Next();
                        return new StringExpression(Unquote(text));
                    }
                case Tok.BOOL:
                    {
                        bool value = l.LexText == "TRUE";
                        Next();
                        return new BoolExpression(value);
                    }
                case Tok.ERROR:
                    {
                        string text = l.LexText;
                        Next();
                        return new ErrorExpression(text);
                    }
                case Tok.CELLREF:
                    {
                        string text = l.LexText;
                        Next();
                        if (l.LexKind == Tok.COLON)
                        {
                            Next();
                            string end = Expect(Tok.CELLREF, "a cell reference was expected");
                            return new RangeExpression(text, end);
                        }
                        return new CellRefExpression(text);
                    }
                case Tok.COLREF:
                    {
                        string text = l.LexText;
                        Next();
                        return new ColumnRefExpression(text);
                    }
                case Tok.ROWREF:
                    {
                        string text = l.LexText;
                        Next();
                        return new RowRefExpression(text);
                    }
                case Tok.FUNCNAME:
                    return ParseCall();
                case Tok.NAME:
                    return ParseNameAtom();
                case Tok.LPAREN:
                    {
                        Next();
                        Expression expression = ParseExpr();
                        Expect(Tok.RPAREN, "')' was expected");
                        return expression;
                    }
                default:
                    SyntaxError("an expression was expected");
                    return null;
            }
        }

        private Expression ParseNameAtom()
        {
            string name = l.LexText;
            Next();
            if (l.LexKind == Tok.LPAREN)
                return ParseCall(name);
            if (l.LexKind == Tok.BANG)
            {
                Next();
                string reference = Expect(Tok.CELLREF, "a cell reference was expected");
                return new SheetRefExpression(name, reference);
            }
            return new NameExpression(name);
        }

        private Expression ParseCall()
        {
            string name = l.LexText;
            Next();
            return ParseCall(name);
        }

        private Expression ParseCall(string name)
        {
            Expect(Tok.LPAREN, "'(' was expected");
            List<Expression> arguments = new List<Expression>();
            if (l.LexKind != Tok.RPAREN)
            {
                arguments.Add(ParseExpr());
                while (l.LexKind == Tok.COMMA)
                {
                    Next();
                    arguments.Add(ParseExpr());
                }
            }
            Expect(Tok.RPAREN, "')' was expected");
            return new CallExpression(name, arguments);
        }

        private bool IsComparison(Tok kind)
        {
            return kind == Tok.EQ || kind == Tok.NEQ || kind == Tok.LT ||
                   kind == Tok.GT || kind == Tok.LEQ || kind == Tok.GEQ;
        }

        private void Next()
        {
            l.NextLexem();
        }

        private string Expect(Tok kind, string message)
        {
            if (l.LexKind != kind)
                SyntaxError(message);
            string text = l.LexText;
            Next();
            return text;
        }

        private string Unquote(string text)
        {
            if (text.Length < 2)
                return text;
            string inner = text.Substring(1, text.Length - 2);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < inner.Length; i++)
            {
                if (inner[i] == '\\' && i + 1 < inner.Length)
                    i++;
                result.Append(inner[i]);
            }
            return result.ToString();
        }

        public void SyntaxError(string message)
        {
            var errorMessage = "Syntax error in line " + l.LexRow.ToString() + ":\n";
            errorMessage += l.FinishCurrentLine() + "\n";
            errorMessage += new string(' ', l.LexCol - 1) + "^\n";
            if (message != "")
                errorMessage += message;
            throw new ParserException(errorMessage);
        }
    }
}