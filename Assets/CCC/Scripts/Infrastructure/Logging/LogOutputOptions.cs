using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// アプリケーション全体のLog出力先と出力レベルを保持する不変の設定値です。
    /// </summary>
    public sealed class LogOutputOptions
    {
        /// <summary>
        /// 全カテゴリへ適用する最低LogLevelを取得します。
        /// </summary>
        public LogLevel MinimumLevel { get; }

        /// <summary>
        /// カテゴリ別の最低LogLevel上書き一覧を取得します。
        /// </summary>
        public IReadOnlyList<LogCategoryLevelOverride> CategoryLevelOverrides { get; }

        /// <summary>
        /// Unity Consoleへの出力を有効にするかどうかを取得します。
        /// </summary>
        public bool IsUnityConsoleEnabled { get; }

        /// <summary>
        /// 通常Logのファイル出力を有効にするかどうかを取得します。
        /// </summary>
        public bool IsGeneralFileEnabled { get; }

        /// <summary>
        /// 通常Logファイルを配置する種類別ディレクトリ名を取得します。
        /// </summary>
        public string GeneralDirectoryName { get; }

        /// <summary>
        /// Log出力設定を生成します。
        /// </summary>
        /// <param name="minimumLevel">全カテゴリへ適用する最低LogLevel。</param>
        /// <param name="categoryLevelOverrides">カテゴリ別の最低LogLevel上書き一覧。</param>
        /// <param name="isUnityConsoleEnabled">Unity Consoleへの出力を有効にするかどうか。</param>
        /// <param name="isGeneralFileEnabled">通常Logのファイル出力を有効にするかどうか。</param>
        /// <param name="generalDirectoryName">通常Logファイルを配置する種類別ディレクトリ名。</param>
        /// <exception cref="ArgumentNullException">上書き一覧がnull、またはnull要素を含む場合。</exception>
        /// <exception cref="ArgumentException">ディレクトリ名が空白の場合。</exception>
        public LogOutputOptions(
            LogLevel minimumLevel,
            IReadOnlyList<LogCategoryLevelOverride> categoryLevelOverrides,
            bool isUnityConsoleEnabled,
            bool isGeneralFileEnabled,
            string generalDirectoryName)
        {
            if (categoryLevelOverrides == null || categoryLevelOverrides.Any(entry => entry == null))
            {
                throw new ArgumentNullException(nameof(categoryLevelOverrides));
            }

            if (string.IsNullOrWhiteSpace(generalDirectoryName))
            {
                throw new ArgumentException("Directory name must not be blank.", nameof(generalDirectoryName));
            }

            MinimumLevel = minimumLevel;
            CategoryLevelOverrides = categoryLevelOverrides.ToArray();
            IsUnityConsoleEnabled = isUnityConsoleEnabled;
            IsGeneralFileEnabled = isGeneralFileEnabled;
            GeneralDirectoryName = generalDirectoryName;
        }
    }
}
