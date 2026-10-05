using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 1つのCSV表へ追記するエスケープ前のデータ行一覧を保持する不変値です。
    /// </summary>
    public sealed class CsvTableRows
    {
        /// <summary>
        /// 追記先の表定義を取得します。
        /// </summary>
        public CsvTableDefinition Table { get; }

        /// <summary>
        /// 列順に並んだエスケープ前のフィールド値の行一覧を取得します。
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> Rows { get; }

        /// <summary>
        /// 表定義と行一覧から追記データを生成します。
        /// </summary>
        /// <param name="table">追記先の表定義。</param>
        /// <param name="rows">列順に並んだエスケープ前のフィールド値の行一覧。</param>
        /// <exception cref="ArgumentNullException">表定義または行一覧がnull、または行がnullの場合。</exception>
        /// <exception cref="ArgumentException">行のフィールド数が列数と一致しない場合。</exception>
        public CsvTableRows(CsvTableDefinition table, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            if (rows == null || rows.Any(row => row == null))
            {
                throw new ArgumentNullException(nameof(rows));
            }

            if (rows.Any(row => row.Count != table.Columns.Count))
            {
                throw new ArgumentException(
                    $"Every row of table '{table.FileSuffix}' must contain {table.Columns.Count} fields.",
                    nameof(rows));
            }

            Rows = rows.Select(row => (IReadOnlyList<string>)row.ToArray()).ToArray();
        }
    }
}
