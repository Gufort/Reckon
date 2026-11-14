using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Reckon
{
    public class LexerException : System.Exception
    {
        public LexerException(string msg) : base(msg) { }
    }

    public enum Tok
    {
        EOF, NUMBER, STRING, BOOL, ERROR,
        CELLREF, COLREF, ROWREF, FUNCNAME, NAME,
        ASSIGN, PLUS, MINUS, MULT, DIV, POW, CONCAT,
        EQ, NEQ, LT, GT, LEQ, GEQ,
        LPAREN, RPAREN, COMMA, COLON, BANG, ARROW
    }

    public class Lexer
    {
        private char currentCh;
        public int LexRow, LexCol;
        private int row, col;
        private TextReader inputReader;
        private Dictionary<string, Tok> keywordsMap;
        public Tok LexKind;
        public string LexText;
        public object LexValue;

        private string CurrentLineText;
        private int commentDepth;

        public Lexer(TextReader input)
        {
            CurrentLineText = "";
            inputReader = input;
            keywordsMap = new Dictionary<string, Tok>();
            InitKeywords();
            row = 1; col = 0;
            NextCh();
            NextLexem();
        }

        private void InitKeywords()
        {
            keywordsMap["TRUE"] = Tok.BOOL;
            keywordsMap["FALSE"] = Tok.BOOL;
        }

        private void PassSpaces()
        {
            while (char.IsWhiteSpace(currentCh))
            {
                NextChRaw();
            }
        }

        public string FinishCurrentLine()
        {
            return CurrentLineText + inputReader.ReadLine();
        }

        private void LexError(string message)
        {
            StringBuilder errorDescription = new StringBuilder();
            errorDescription.AppendFormat("Lexical error in line {0}:", row);
            errorDescription.Append("\n");
            errorDescription.Append(FinishCurrentLine());
            errorDescription.Append("\n");
            errorDescription.Append(new string(' ', col - 1) + '^');
            errorDescription.Append('\n');
            if (message != "")
            {
                errorDescription.Append(message);
            }
            throw new LexerException(errorDescription.ToString());
        }

        private void NextChRaw()
        {
            var nextChar = inputReader.Read();
            if (nextChar != -1)
            {
                currentCh = (char)nextChar;
                if (currentCh != '\n')
                {
                    col += 1;
                    CurrentLineText += currentCh;
                }
                else
                {
                    row += 1;
                    col = 0;
                    CurrentLineText = "";
                }
            }
            else
            {
                currentCh = (char)0;
            }
        }

        private void NextCh()
        {
            LexText += currentCh;
            var nextChar = inputReader.Read();
            if (nextChar != -1)
            {
                currentCh = (char)nextChar;
                if (currentCh != '\n')
                {
                    col += 1;
                    CurrentLineText += currentCh;
                }
                else
                {
                    row += 1;
                    col = 0;
                    CurrentLineText = "";
                }
            }
            else
            {
                currentCh = (char)0;
            }
        }

        private bool IsAllLetterUpper(string s)
        {
            foreach (char c in s)
                if (!char.IsLetter(c) || !char.IsUpper(c))
                    return false;
            return s.Length > 0;
        }

        private bool IsCellRefUpperDig(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            int i = 0;
            while (i < s.Length && char.IsUpper(s[i])) i++;
            if (i == 0 || i == s.Length) return false;
            while (i < s.Length && char.IsDigit(s[i])) i++;
            return i == s.Length;
        }

        public void NextLexem()
        {
            throw new NotImplementedException();
        }

        public virtual void ParseToConsole()
        {
            do
            {
                Console.WriteLine(TokToString(LexKind));
                NextLexem();
            } while (LexKind != Tok.EOF);
        }

        public string TokToString(Tok t)
        {
            var result = t.ToString();
            switch (t)
            {
                case Tok.NUMBER:
                    result += " " + (LexValue != null ? LexValue.ToString() : LexText);
                    break;
                case Tok.STRING:
                    result += " " + LexValue;
                    break;
                case Tok.BOOL:
                    result += " " + LexValue.ToString();
                    break;
                case Tok.ERROR:
                    result += " " + LexValue;
                    break;
                case Tok.CELLREF:
                case Tok.COLREF:
                case Tok.ROWREF:
                case Tok.FUNCNAME:
                case Tok.NAME:
                    result += " " + (LexValue?.ToString() ?? LexText);
                    break;
            }
            return result;
        }
    }
}