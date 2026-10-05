using System;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// CSV列名と、行の元データからその列のフィールド値を取り出す関数の組を保持します。
    /// </summary>
    /// <typeparam name="TRow">行の元データの型。</typeparam>
    internal sealed class CsvColumn<TRow>
    {
        private readonly Func<TRow, string> _selectField;

        /// <summary>
        /// 列名とフィールド値の取り出し関数から列定義を生成します。
        /// </summary>
        /// <param name="name">ヘッダー行に書き出す列名。</param>
        /// <param name="selectField">行の元データからフィールド値を取り出す関数。</param>
        /// <exception cref="ArgumentException">列名が空白の場合。</exception>
        /// <exception cref="ArgumentNullException">取り出し関数がnullの場合。</exception>
        internal CsvColumn(string name, Func<TRow, string> selectField)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Column name must not be blank.", nameof(name));
            }

            Name = name;
            _selectField = selectField ?? throw new ArgumentNullException(nameof(selectField));
        }

        /// <summary>
        /// ヘッダー行に書き出す列名を取得します。
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// 行の元データからこの列のフィールド値を取り出します。
        /// </summary>
        /// <param name="row">行の元データ。</param>
        /// <returns>エスケープ前のフィールド値。</returns>
        internal string SelectField(TRow row)
        {
            return _selectField(row);
        }
    }
}
