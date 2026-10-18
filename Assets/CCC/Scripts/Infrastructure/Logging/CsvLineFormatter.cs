using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// フィールド値の並びをRFC 4180形式のCSV 1行へ変換します。
    /// </summary>
    public static class CsvLineFormatter
    {
        private const char Delimiter = ',';
        private const char Quote = '"';
        private static readonly char[] CharactersRequiringQuote = { Delimiter, Quote, '\r', '\n' };

        /// <summary>
        /// フィールド値の並びをエスケープしてCSV 1行へ連結します。
        /// </summary>
        /// <param name="fields">列順に並んだフィールド値。nullは空文字として扱います。</param>
        /// <returns>改行を含まないCSV 1行。</returns>
        /// <exception cref="ArgumentNullException">フィールド値の並びがnullの場合。</exception>
        public static string FormatLine(IReadOnlyList<string> fields)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            return string.Join(Delimiter.ToString(), fields.Select(EscapeField));
        }

        private static string EscapeField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            if (field.IndexOfAny(CharactersRequiringQuote) < 0)
            {
                return field;
            }

            string doubledQuote = new string(Quote, 2);
            return Quote + field.Replace(Quote.ToString(), doubledQuote) + Quote;
        }
    }
}
