using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Reckon;

namespace LexerDemo
{
    class Program
    {
        public static void Main()
        {
            string fileContents = @"A1 := 10
A2 := $A$1 + SUM(A1:A2) -- cell sum
result := IF(A1 > 0, ""ok"", #DIV/0!)
{- block {- nested -} comment -}";
            TextReader inputReader = new StringReader(fileContents);
            Lexer l = new Lexer(inputReader);
            try
            {
                do
                {
                    Console.WriteLine(l.TokToString(l.LexKind));
                    l.NextLexem();
                } while (l.LexKind != Tok.EOF);
            }
            catch (LexerException e)
            {
                Console.WriteLine("lexer error: " + e.Message);
            }
        }
    }
}