using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 1つのCSVファイルに対応する表のファイル名接尾辞と列名一覧を保持する不変値です。
    /// </summary>
    public sealed class CsvTableDefinition
    {
        /// <summary>
        /// SessionIdの後ろに付けるファイル名接尾辞を取得します。
        /// </summary>
        public string FileSuffix { get; }

        /// <summary>
        /// ヘッダー行に書き出す列名一覧を取得します。
        /// </summary>
        public IReadOnlyList<string> Columns { get; }

        /// <summary>
        /// ファイル名接尾辞と列名一覧から表定義を生成します。
        /// </summary>
        /// <param name="fileSuffix">SessionIdの後ろに付けるファイル名接尾辞。</param>
        /// <param name="columns">ヘッダー行に書き出す列名一覧。</param>
        /// <exception cref="ArgumentException">接尾辞が空白、または列名一覧が空か空白要素を含む場合。</exception>
        /// <exception cref="ArgumentNullException">列名一覧がnullの場合。</exception>
        public CsvTableDefinition(string fileSuffix, IReadOnlyList<string> columns)
        {
            if (string.IsNullOrWhiteSpace(fileSuffix))
            {
                throw new ArgumentException("File suffix must not be blank.", nameof(fileSuffix));
            }

            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (columns.Count == 0 || columns.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Columns must contain at least one non-blank name.", nameof(columns));
            }

            FileSuffix = fileSuffix;
            Columns = columns.ToArray();
        }
    }
}
