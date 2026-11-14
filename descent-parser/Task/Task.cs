using System;
using SimpleLexer;

namespace SimpleParser
{
    public class ParserException : System.Exception
    {
        public ParserException(string msg) : base(msg) { }
    }

    public class Parser
    {
        private Lexer l;

        public Parser(Lexer lexer)
        {
            l = lexer;
        }

        public void Parse()
        {
            throw new NotImplementedException();
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