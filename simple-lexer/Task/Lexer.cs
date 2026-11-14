using System;
using System.Collections.Generic;
using System.Globalization;
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

        private char Peek()
        {
            int next = inputReader.Peek();
            return next == -1 ? (char)0 : (char)next;
        }

        private void SkipWhitespaceAndComments()
        {
            while (true)
            {
                while (char.IsWhiteSpace(currentCh))
                {
                    NextChRaw();
                }

                if (currentCh == '-' && Peek() == '-')
                {
                    NextChRaw();
                    NextChRaw();
                    while (currentCh != '\n' && currentCh != '\0')
                    {
                        NextChRaw();
                    }
                }

                else if (currentCh == '{' && Peek() == '-')
                {
                    NextChRaw();
                    NextChRaw();
                    int depth = 1;
                    while (depth > 0 && currentCh != '\0')
                    {
                        if (currentCh == '{' && Peek() == '-')
                        {
                            depth++;
                            NextChRaw();
                            NextChRaw();
                        }
                        else if (currentCh == '-' && Peek() == '}')
                        {
                            depth--;
                            NextChRaw();
                            NextChRaw();
                        }
                        else NextChRaw();
                    }
                    if (depth > 0) LexError("Unclosed block comment");
                }
                else break;
            }
        }

        private void ReadNumber()
        {
            while (char.IsDigit(currentCh))
            {
                NextCh();
            }

            if (currentCh == '.')
            {
                NextCh();
                while (char.IsDigit(currentCh))
                {
                    NextCh();
                }
            }

            string text = LexText;

            if (text.Contains("."))
                LexValue = double.Parse(text, CultureInfo.InvariantCulture);
            else
                LexValue = int.Parse(text);

            LexKind = Tok.NUMBER;
        }

        private void ReadString()
        {
            NextCh();
            StringBuilder sb = new StringBuilder();
            while (currentCh != '"' && currentCh != '\0')
            {
                if (currentCh == '\\')
                {
                    NextCh();
                    switch (currentCh)
                    {
                        case '"': sb.Append('"'); NextCh(); break;
                        case '\\': sb.Append('\\'); NextCh(); break;
                        case 'n': sb.Append('\n'); NextCh(); break;
                        case 't': sb.Append('\t'); NextCh(); break;
                        default: LexError($"Invalid escape sequence '\\{currentCh}'"); break;
                    }
                }
                else
                {
                    sb.Append(currentCh);
                    NextCh();
                }
            }
            if (currentCh == '\0')
            {
                LexError("Unterminated string literal");
            }
            NextCh();
            LexValue = sb.ToString();
            LexKind = Tok.STRING;
        }

        private void ReadError()
        {
            NextCh();
            while (char.IsLetterOrDigit(currentCh) || currentCh == '/' || currentCh == '!' || currentCh == '?' || currentCh == '#')
            {
                NextCh();
            }
            LexKind = Tok.ERROR;
            LexValue = LexText;
        }

        private void ReadAtRef()
        {
            NextCh();
            if (char.IsLetter(currentCh))
            {
                while (char.IsLetter(currentCh))
                {
                    NextCh();
                }
                LexKind = Tok.COLREF;
            }
            else if (char.IsDigit(currentCh))
            {
                while (char.IsDigit(currentCh))
                {
                    NextCh();
                }
                LexKind = Tok.ROWREF;
            }
            else
            {
                LexError("Expected letter or digit after '@'");
            }
            LexValue = LexText;
        }

        private bool IsCellRef(string s)
        {
            int i = 0;
            if (i < s.Length && s[i] == '$') i++;
            int startLetters = i;
            while (i < s.Length && char.IsUpper(s[i])) i++;
            if (i == startLetters) return false;
            if (i < s.Length && s[i] == '$') i++;
            int startDigits = i;
            while (i < s.Length && char.IsDigit(s[i])) i++;
            if (i == startDigits) return false;
            return i == s.Length;
        }

        private static bool IsIdentChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '$' || c == '_';
        }

        private void ReadIdentifierOrCellRef()
        {
            while (IsIdentChar(currentCh))
            {
                NextCh();
            }

            string text = LexText;
            if (text == "TRUE")
            {
                LexKind = Tok.BOOL;
                LexValue = true;
            }

            else if (text == "FALSE")
            {
                LexKind = Tok.BOOL;
                LexValue = false;
            }

            else if (IsCellRef(text))
            {
                LexKind = Tok.CELLREF;
                LexValue = text;
            }

            else if (IsAllLetterUpper(text))
            {
                LexKind = Tok.FUNCNAME;
                LexValue = text;
            }

            else
            {
                LexKind = Tok.NAME;
                LexValue = text;
            }
        }

        public void NextLexem()
        {
            LexText = "";
            LexValue = null;
            SkipWhitespaceAndComments();

            if (currentCh == '\0')
            {
                LexKind = Tok.EOF;
                return;
            }

            LexRow = row;
            LexCol = col;

            if (char.IsDigit(currentCh))
                ReadNumber();
            else if (currentCh == '"')
                ReadString();
            else if (currentCh == '#')
                ReadError();
            else if (currentCh == '@')
                ReadAtRef();
            else if (char.IsLetter(currentCh) || currentCh == '$' || currentCh == '_')
                ReadIdentifierOrCellRef();
            else
            {
                switch (currentCh)
                {
                    case '+':
                        NextCh();
                        LexKind = Tok.PLUS;
                        break;
                    case '-':
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
                        break;
                    case '*':
                        NextCh();
                        LexKind = Tok.MULT;
                        break;
                    case '/':
                        NextCh();
                        LexKind = Tok.DIV;
                        break;
                    case '^':
                        NextCh();
                        LexKind = Tok.POW;
                        break;
                    case '&':
                        NextCh();
                        LexKind = Tok.CONCAT;
                        break;
                    case '=':
                        NextCh();
                        LexKind = Tok.EQ;
                        break;
                    case '<':
                        NextCh();
                        if (currentCh == '>')
                        {
                            NextCh();
                            LexKind = Tok.NEQ;
                        }
                        else if (currentCh == '=')
                        {
                            NextCh();
                            LexKind = Tok.LEQ;
                        }
                        else
                        {
                            LexKind = Tok.LT;
                        }
                        break;
                    case '>':
                        NextCh();
                        if (currentCh == '=')
                        {
                            NextCh();
                            LexKind = Tok.GEQ;
                        }
                        else
                        {
                            LexKind = Tok.GT;
                        }
                        break;
                    case '(':
                        NextCh();
                        LexKind = Tok.LPAREN;
                        break;
                    case ')':
                        NextCh();
                        LexKind = Tok.RPAREN;
                        break;
                    case ',':
                        NextCh();
                        LexKind = Tok.COMMA;
                        break;
                    case ':':
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
                        break;
                    case '!':
                        NextCh();
                        LexKind = Tok.BANG;
                        break;
                    default:
                        LexError($"Unexpected character '{currentCh}'");
                        break;
                }
            }
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