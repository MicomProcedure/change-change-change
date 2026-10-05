using System;
using Microsoft.Extensions.Logging;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// Mission抽選診断のLog出力とCSV出力の設定を保持する不変の設定値です。
    /// </summary>
    public sealed class MissionDrawDiagnosticsOptions
    {
        /// <summary>
        /// Log出力に使うLogLevelを取得します。
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// Log出力に含める順位上位の候補数を取得します。
        /// </summary>
        public int LogCandidateCount { get; }

        /// <summary>
        /// CSV出力を有効にするかどうかを取得します。
        /// </summary>
        public bool IsCsvEnabled { get; }

        /// <summary>
        /// CSV出力に含める順位上位の候補数を取得します。
        /// </summary>
        public int CsvCandidateCount { get; }

        /// <summary>
        /// CSVファイルを配置する種類別ディレクトリ名を取得します。
        /// </summary>
        public string CsvDirectoryName { get; }

        /// <summary>
        /// Mission抽選診断の設定を生成します。
        /// </summary>
        /// <param name="logLevel">Log出力に使うLogLevel。</param>
        /// <param name="logCandidateCount">Log出力に含める順位上位の候補数。</param>
        /// <param name="isCsvEnabled">CSV出力を有効にするかどうか。</param>
        /// <param name="csvCandidateCount">CSV出力に含める順位上位の候補数。</param>
        /// <param name="csvDirectoryName">CSVファイルを配置する種類別ディレクトリ名。</param>
        /// <exception cref="ArgumentOutOfRangeException">候補数が負の場合、またはLogLevelがNoneの場合。</exception>
        /// <exception cref="ArgumentException">ディレクトリ名が空白の場合。</exception>
        public MissionDrawDiagnosticsOptions(
            LogLevel logLevel,
            int logCandidateCount,
            bool isCsvEnabled,
            int csvCandidateCount,
            string csvDirectoryName)
        {
            if (logLevel == LogLevel.None)
            {
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "Log level must not be None.");
            }

            if (logCandidateCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(logCandidateCount),
                    logCandidateCount,
                    "Candidate count must not be negative.");
            }

            if (csvCandidateCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(csvCandidateCount),
                    csvCandidateCount,
                    "Candidate count must not be negative.");
            }

            if (string.IsNullOrWhiteSpace(csvDirectoryName))
            {
                throw new ArgumentException("Directory name must not be blank.", nameof(csvDirectoryName));
            }

            LogLevel = logLevel;
            LogCandidateCount = logCandidateCount;
            IsCsvEnabled = isCsvEnabled;
            CsvCandidateCount = csvCandidateCount;
            CsvDirectoryName = csvDirectoryName;
        }
    }
}
