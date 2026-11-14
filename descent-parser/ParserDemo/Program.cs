using System;
using System.IO;
using SimpleParser;
using SimpleLexer;

namespace ParserDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileContents = @"price := 100
tax(rate, amount) := amount * rate
total := price + tax(0.2, price)
A1 := Sheet1!B1 + SUM(A1:A10)
result := IF(A1 > 0, TRUE, #DIV/0!)";

            TextReader inputReader = new StringReader(fileContents);
            Lexer l = new Lexer(inputReader);
            Parser p = new Parser(l);

            try
            {
                p.Parse();
                if (l.LexKind == Tok.EOF)
                {
                    Console.WriteLine("Program successfully recognized");
                }
                else
                {
                    p.SyntaxError("end of file was expected");
                }
            }
            catch (ParserException e)
            {
                Console.WriteLine("parser error: " + e.Message);
            }
            catch (LexerException le)
            {
                Console.WriteLine("lexer error: " + le.Message);
            }
        }
    }
}