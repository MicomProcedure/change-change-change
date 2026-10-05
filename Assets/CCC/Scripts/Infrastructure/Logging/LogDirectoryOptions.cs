using System;
using System.Globalization;
using System.IO;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// Logファイルの出力ルートとファイル名に使う日時書式を保持し、種類別の出力パスを解決する不変の設定値です。
    /// </summary>
    public sealed class LogDirectoryOptions
    {
        /// <summary>
        /// Logファイルを出力するルートディレクトリの絶対パスを取得します。
        /// </summary>
        public string RootDirectoryPath { get; }

        /// <summary>
        /// ファイル名とSessionIdに使う日時書式を取得します。
        /// </summary>
        public string FileTimestampFormat { get; }

        /// <summary>
        /// 出力ルートと日時書式からディレクトリ設定を生成します。
        /// </summary>
        /// <param name="rootDirectoryPath">Logファイルを出力するルートディレクトリの絶対パス。</param>
        /// <param name="fileTimestampFormat">ファイル名とSessionIdに使う日時書式。</param>
        /// <exception cref="ArgumentException">ルートが空白または相対パスの場合、または日時書式が空白の場合。</exception>
        public LogDirectoryOptions(string rootDirectoryPath, string fileTimestampFormat)
        {
            if (string.IsNullOrWhiteSpace(rootDirectoryPath) || !Path.IsPathRooted(rootDirectoryPath))
            {
                throw new ArgumentException(
                    "Root directory path must be an absolute path.",
                    nameof(rootDirectoryPath));
            }

            if (string.IsNullOrWhiteSpace(fileTimestampFormat))
            {
                throw new ArgumentException("Timestamp format must not be blank.", nameof(fileTimestampFormat));
            }

            RootDirectoryPath = rootDirectoryPath;
            FileTimestampFormat = fileTimestampFormat;
        }

        /// <summary>
        /// 種類別ディレクトリの絶対パスを解決します。
        /// </summary>
        /// <param name="categoryDirectoryName">種類別ディレクトリ名。</param>
        /// <returns>種類別ディレクトリの絶対パス。</returns>
        /// <exception cref="ArgumentException">ディレクトリ名が空白の場合。</exception>
        public string ResolveCategoryDirectoryPath(string categoryDirectoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryDirectoryName))
            {
                throw new ArgumentException("Directory name must not be blank.", nameof(categoryDirectoryName));
            }

            return Path.Combine(RootDirectoryPath, categoryDirectoryName);
        }

        /// <summary>
        /// 日時をファイル名用の書式で文字列化します。
        /// </summary>
        /// <param name="timestamp">文字列化する日時。</param>
        /// <returns>ファイル名用の日時文字列。</returns>
        public string FormatTimestamp(DateTime timestamp)
        {
            return timestamp.ToString(FileTimestampFormat, CultureInfo.InvariantCulture);
        }
    }
}
