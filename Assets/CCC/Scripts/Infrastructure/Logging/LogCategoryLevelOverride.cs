using System;
using Microsoft.Extensions.Logging;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 指定カテゴリ接頭辞に適用する最低LogLevelを保持する不変の設定値です。
    /// </summary>
    public sealed class LogCategoryLevelOverride
    {
        /// <summary>
        /// 最低LogLevelを上書きするカテゴリ名の接頭辞を取得します。
        /// </summary>
        public string CategoryPrefix { get; }

        /// <summary>
        /// 該当カテゴリへ適用する最低LogLevelを取得します。
        /// </summary>
        public LogLevel MinimumLevel { get; }

        /// <summary>
        /// カテゴリ接頭辞と最低LogLevelから上書き設定を生成します。
        /// </summary>
        /// <param name="categoryPrefix">カテゴリ名の接頭辞。</param>
        /// <param name="minimumLevel">該当カテゴリへ適用する最低LogLevel。</param>
        /// <exception cref="ArgumentException">カテゴリ接頭辞が空白の場合。</exception>
        public LogCategoryLevelOverride(string categoryPrefix, LogLevel minimumLevel)
        {
            if (string.IsNullOrWhiteSpace(categoryPrefix))
            {
                throw new ArgumentException("Category prefix must not be blank.", nameof(categoryPrefix));
            }

            CategoryPrefix = categoryPrefix;
            MinimumLevel = minimumLevel;
        }
    }
}
