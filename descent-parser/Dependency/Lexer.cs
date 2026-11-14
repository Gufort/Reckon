using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleLexer
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

        public Lexer(TextReader input)
        {
            CurrentLineText = "";
            inputReader = input;
            keywordsMap = new Dictionary<string, Tok>();
            InitKeywords();
            row = 1; col = 0;
            NextChRaw();
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
                NextChRaw();
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
                errorDescription.Append(message);
            throw new LexerException(errorDescription.ToString());
        }

        private void ReadNext()
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

        private void NextChRaw()
        {
            ReadNext();
        }

        private void NextCh()
        {
            LexText += currentCh;
            ReadNext();
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

        private void SkipBlockComment()
        {
            int depth = 1;
            while (depth > 0)
            {
                if ((int)currentCh == 0)
                {
                    LexError("Unclosed block comment");
                    return;
                }
                if (currentCh == '{' && inputReader.Peek() == '-')
                {
                    depth++;
                    NextChRaw();
                    NextChRaw();
                }
                else if (currentCh == '-' && inputReader.Peek() == '}')
                {
                    depth--;
                    NextChRaw();
                    NextChRaw();
                }
                else
                {
                    NextChRaw();
                }
            }
        }

        private void ReadString()
        {
            while (true)
            {
                if ((int)currentCh == 0)
                {
                    LexError("Unclosed string literal");
                    return;
                }
                if (currentCh == '\\')
                {
                    NextCh();
                    if ((int)currentCh == 0)
                    {
                        LexError("Unclosed string literal");
                        return;
                    }
                    NextCh();
                }
                else if (currentCh == '"')
                {
                    NextCh();
                    break;
                }
                else
                {
                    NextCh();
                }
            }
            LexKind = Tok.STRING;
        }

        public void NextLexem()
        {
            PassSpaces();
            LexText = "";
            LexRow = row;
            LexCol = col;

            if (currentCh == '-' && inputReader.Peek() == '-')
            {
                NextCh();
                NextCh();
                while ((int)currentCh != 0 && currentCh != '\n')
                    NextChRaw();
                NextLexem();
                return;
            }

            if (currentCh == '{' && inputReader.Peek() == '-')
            {
                NextCh();
                SkipBlockComment();
                NextLexem();
                return;
            }

            if (currentCh == '"')
            {
                NextCh();
                ReadString();
                return;
            }

            if (currentCh == '#')
            {
                NextCh();
                while (char.IsLetterOrDigit(currentCh) || currentCh == '/' || currentCh == '!' || currentCh == '?')
                    NextCh();
                LexKind = Tok.ERROR;
                return;
            }

            if (currentCh == '$')
            {
                NextCh();
                if (!char.IsUpper(currentCh))
                {
                    LexError("Expected column letter after $");
                    return;
                }
                while (char.IsUpper(currentCh))
                    NextCh();
                if (currentCh == '$')
                    NextCh();
                if (!char.IsDigit(currentCh))
                {
                    LexError("Expected row number");
                    return;
                }
                while (char.IsDigit(currentCh))
                    NextCh();
                LexKind = Tok.CELLREF;
                return;
            }

            if (char.IsLetter(currentCh))
            {
                while (char.IsLetterOrDigit(currentCh) || currentCh == '_')
                    NextCh();

                if (currentCh == '$' && IsAllLetterUpper(LexText))
                {
                    NextCh();
                    if (char.IsDigit(currentCh))
                    {
                        while (char.IsDigit(currentCh))
                            NextCh();
                        LexKind = Tok.CELLREF;
                        return;
                    }
                    else
                    {
                        LexError("Expected row number after $");
                        return;
                    }
                }

                if (keywordsMap.ContainsKey(LexText))
                {
                    LexKind = Tok.BOOL;
                    return;
                }

                if (IsAllLetterUpper(LexText))
                {
                    LexKind = Tok.FUNCNAME;
                    return;
                }

                if (IsCellRefUpperDig(LexText))
                {
                    LexKind = Tok.CELLREF;
                    return;
                }

                LexKind = Tok.NAME;
                return;
            }

            if (char.IsDigit(currentCh))
            {
                while (char.IsDigit(currentCh))
                    NextCh();
                if (currentCh == '.')
                {
                    NextCh();
                    while (char.IsDigit(currentCh))
                        NextCh();
                }
                LexKind = Tok.NUMBER;
                return;
            }

            if (currentCh == '@')
            {
                NextCh();
                if (char.IsUpper(currentCh))
                {
                    while (char.IsUpper(currentCh))
                        NextCh();
                    LexKind = Tok.COLREF;
                }
                else if (char.IsDigit(currentCh))
                {
                    while (char.IsDigit(currentCh))
                        NextCh();
                    LexKind = Tok.ROWREF;
                }
                else
                {
                    LexError("Invalid @ reference");
                }
                return;
            }

            if (currentCh == ':')
            {
                NextCh();
                if (currentCh == '=')
                {
                    NextCh();
                    LexKind = Tok.ASSIGN;
                }
                else
                {
                    LexKind = Tok.COLON;
                }
                return;
            }

            if (currentCh == '-')
            {
                NextCh();
                if (currentCh == '>')
                {
                    NextCh();
                    LexKind = Tok.ARROW;
                }
                else
                {
                    LexKind = Tok.MINUS;
                }
                return;
            }

            if (currentCh == '+') { NextCh(); LexKind = Tok.PLUS; return; }
            if (currentCh == '*') { NextCh(); LexKind = Tok.MULT; return; }
            if (currentCh == '/') { NextCh(); LexKind = Tok.DIV; return; }
            if (currentCh == '^') { NextCh(); LexKind = Tok.POW; return; }
            if (currentCh == '&') { NextCh(); LexKind = Tok.CONCAT; return; }
            if (currentCh == '=') { NextCh(); LexKind = Tok.EQ; return; }

            if (currentCh == '<')
            {
                NextCh();
                if (currentCh == '>') { NextCh(); LexKind = Tok.NEQ; }
                else if (currentCh == '=') { NextCh(); LexKind = Tok.LEQ; }
                else { LexKind = Tok.LT; }
                return;
            }

            if (currentCh == '>')
            {
                NextCh();
                if (currentCh == '=') { NextCh(); LexKind = Tok.GEQ; }
                else { LexKind = Tok.GT; }
                return;
            }

            if (currentCh == '(') { NextCh(); LexKind = Tok.LPAREN; return; }
            if (currentCh == ')') { NextCh(); LexKind = Tok.RPAREN; return; }
            if (currentCh == ',') { NextCh(); LexKind = Tok.COMMA; return; }
            if (currentCh == '!') { NextCh(); LexKind = Tok.BANG; return; }

            if ((int)currentCh == 0)
            {
                LexKind = Tok.EOF;
                return;
            }

            LexError("Incorrect symbol " + currentCh);
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
                    result += " " + LexText;
                    break;
                case Tok.STRING:
                    result += " " + LexText;
                    break;
                case Tok.BOOL:
                    result += " " + LexText;
                    break;
                case Tok.ERROR:
                    result += " " + LexText;
                    break;
                case Tok.CELLREF:
                case Tok.COLREF:
                case Tok.ROWREF:
                case Tok.FUNCNAME:
                case Tok.NAME:
                    result += " " + LexText;
                    break;
            }
            return result;
        }
    }
}