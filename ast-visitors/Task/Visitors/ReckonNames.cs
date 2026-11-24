using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ProgramTree;

namespace SimpleLang.Visitors
{
    /// <summary>
    /// Вспомогательные операции над именами, адресами ячеек и диапазонами Reckon.
    /// </summary>
    internal static class ReckonNames
    {
        private static readonly HashSet<string> BuiltinFunctions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "MAP", "FILTER", "REDUCE", "SORT", "UNIQUE", "LAMBDA"
            };

        /// <summary>
        /// Возвращает true для встроенных функций: ключевых слов map/filter/reduce/sort/unique/lambda
        /// и имён, состоящих только из заглавных букв и цифр (лексер относит их к токену FUNCNAME).
        /// </summary>
        public static bool IsBuiltinFunction(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            if (BuiltinFunctions.Contains(name))
                return true;

            foreach (char c in name)
                if (!char.IsUpper(c) && !char.IsDigit(c))
                    return false;
            return true;
        }

        /// <summary>
        /// Убирает из адреса знаки $ и приводит буквы к верхнему регистру: $A$1 и a1 дают A1.
        /// </summary>
        public static string NormalizeAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                return address;
            return address.Replace("$", string.Empty).ToUpperInvariant();
        }

        /// <summary>
        /// Возвращает имя, под которым определение попадает в граф зависимостей.
        /// </summary>
        public static string GetDefinitionName(Node lhs)
        {
            var cellRef = lhs as CellRefNode;
            if (cellRef != null)
                return NormalizeAddress(cellRef.Address);

            var name = lhs as NameNode;
            if (name != null)
                return name.Name;

            var call = lhs as CallNode;
            if (call != null)
                return call.Name;

            return null;
        }

        /// <summary>
        /// Перечисляет адреса всех ячеек диапазона: A1:B2 -> A1, A2, B1, B2.
        /// Если границы не являются корректными адресами, они возвращаются как есть.
        /// </summary>
        public static IEnumerable<string> ExpandRange(string from, string to)
        {
            string fromColumn, toColumn;
            int fromRow, toRow;
            if (!TryParseAddress(from, out fromColumn, out fromRow) ||
                !TryParseAddress(to, out toColumn, out toRow))
            {
                if (!string.IsNullOrEmpty(from))
                    yield return NormalizeAddress(from);
                if (!string.IsNullOrEmpty(to))
                    yield return NormalizeAddress(to);
                yield break;
            }

            int firstColumn = GetColumnIndex(fromColumn);
            int lastColumn = GetColumnIndex(toColumn);
            for (int column = Math.Min(firstColumn, lastColumn); column <= Math.Max(firstColumn, lastColumn); column++)
            {
                string columnName = GetColumnName(column);
                for (int row = Math.Min(fromRow, toRow); row <= Math.Max(fromRow, toRow); row++)
                    yield return columnName + row.ToString(CultureInfo.InvariantCulture);
            }
        }

        private static bool TryParseAddress(string address, out string column, out int row)
        {
            column = null;
            row = 0;

            string normalized = NormalizeAddress(address);
            if (string.IsNullOrEmpty(normalized))
                return false;

            int letters = 0;
            while (letters < normalized.Length && normalized[letters] >= 'A' && normalized[letters] <= 'Z')
                letters++;

            if (letters == 0 || letters == normalized.Length)
                return false;

            column = normalized.Substring(0, letters);
            return int.TryParse(normalized.Substring(letters), NumberStyles.None,
                                CultureInfo.InvariantCulture, out row);
        }

        private static int GetColumnIndex(string column)
        {
            int index = 0;
            foreach (char c in column)
                index = index * 26 + (c - 'A' + 1);
            return index;
        }

        private static string GetColumnName(int index)
        {
            var name = new StringBuilder();
            while (index > 0)
            {
                index--;
                name.Insert(0, (char)('A' + index % 26));
                index /= 26;
            }
            return name.ToString();
        }
    }
}
