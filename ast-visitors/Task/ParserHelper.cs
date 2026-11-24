using System;
using System.Linq;
using System.Text;

namespace SimpleParser
{
    public class LexException : Exception
    {
        public LexException(string msg) : base(msg) { }
    }
    public class SyntaxException : Exception
    {
        public SyntaxException(string msg) : base(msg) { }
    }
    public static class ParserHelper
    {
        /// <summary>
        /// Преобразует текстовое представление строкового литерала
        /// (вместе с окружающими кавычками) в значение строки.
        /// </summary>
        public static string UnescapeString(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 2 ||
                text[0] != '\"' || text[text.Length - 1] != '\"')
                return text;

            string body = text.Substring(1, text.Length - 2);
            StringBuilder result = new StringBuilder(body.Length);

            for (int i = 0; i < body.Length; i++)
            {
                char c = body[i];
                if (c == '\\' && i + 1 < body.Length)
                {
                    char next = body[++i];
                    switch (next)
                    {
                        case '\"': result.Append('\"'); break;
                        case '\\': result.Append('\\'); break;
                        case 'n': result.Append('\n'); break;
                        case 'r': result.Append('\r'); break;
                        case 't': result.Append('\t'); break;
                        default:
                            result.Append('\\');
                            result.Append(next);
                            break;
                    }
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}