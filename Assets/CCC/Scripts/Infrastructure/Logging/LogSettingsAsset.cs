using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// Log出力先、出力レベル、ファイル配置、および診断種類別の出力設定を一元的にInspectorで編集するScriptableObjectです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Log Settings")]
    public sealed class LogSettingsAsset : ScriptableObject
    {
        private const string DefaultRootDirectoryRelativePath = "Logs/Ccc";
        private const string DefaultFileTimestampFormat = "yyyyMMdd_HHmmss";
        private const string DefaultGeneralDirectoryName = "General";

        [Header("Output")]
        [SerializeField]
        private LogLevel minimumLevel = LogLevel.Debug;

        [SerializeField]
        private List<LogCategoryLevelSetting> categoryLevelOverrides = new List<LogCategoryLevelSetting>();

        [SerializeField]
        private bool isUnityConsoleEnabled = true;

        [SerializeField]
        private bool isGeneralFileEnabled = true;

        [Header("File")]
        [SerializeField]
        private string rootDirectoryRelativePath = DefaultRootDirectoryRelativePath;

        [SerializeField]
        private string fileTimestampFormat = DefaultFileTimestampFormat;

        [SerializeField]
        private string generalDirectoryName = DefaultGeneralDirectoryName;

        [Header("Diagnostics")]
        [SerializeField]
        private MissionDrawDiagnosticsSetting missionDrawDiagnostics = new MissionDrawDiagnosticsSetting();

        /// <summary>
        /// 全カテゴリへ適用する最低LogLevelを取得します。
        /// </summary>
        public LogLevel MinimumLevel => minimumLevel;

        /// <summary>
        /// カテゴリ別の最低LogLevel上書き一覧を取得します。
        /// </summary>
        public IReadOnlyList<LogCategoryLevelSetting> CategoryLevelOverrides => categoryLevelOverrides;

        /// <summary>
        /// Unity Consoleへの出力を有効にするかどうかを取得します。
        /// </summary>
        public bool IsUnityConsoleEnabled => isUnityConsoleEnabled;

        /// <summary>
        /// 通常Logのファイル出力を有効にするかどうかを取得します。
        /// </summary>
        public bool IsGeneralFileEnabled => isGeneralFileEnabled;

        /// <summary>
        /// 通常Logファイルを配置する種類別ディレクトリ名を取得します。
        /// </summary>
        public string GeneralDirectoryName => generalDirectoryName;

        /// <summary>
        /// Mission抽選診断の出力設定を取得します。
        /// </summary>
        public MissionDrawDiagnosticsSetting MissionDrawDiagnostics => missionDrawDiagnostics;

        /// <summary>
        /// Inspector設定を検証し、検出されたエラー一覧を返します。
        /// </summary>
        /// <returns>技術的な設定エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            ValidateCategoryLevelOverrides(errors);
            if (string.IsNullOrWhiteSpace(rootDirectoryRelativePath) || Path.IsPathRooted(rootDirectoryRelativePath))
            {
                errors.Add("Root directory path must be a non-blank path relative to the project root.");
            }

            ValidateTimestampFormat(errors);
            ValidateDirectoryName(errors, generalDirectoryName, "General directory name");
            if (missionDrawDiagnostics == null)
            {
                errors.Add("Mission draw diagnostics setting must not be null.");
            }
            else
            {
                ValidateMissionDrawDiagnostics(errors);
            }

            return errors;
        }

        /// <summary>
        /// プロジェクトルート基準で種類別ディレクトリの絶対パスを解決します。
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

            string projectRootPath = Path.GetDirectoryName(Application.dataPath);
            return Path.GetFullPath(Path.Combine(projectRootPath, rootDirectoryRelativePath, categoryDirectoryName));
        }

        /// <summary>
        /// 日時をファイル名とSessionId用の書式で文字列化します。
        /// </summary>
        /// <param name="timestamp">文字列化する日時。</param>
        /// <returns>ファイル名用の日時文字列。</returns>
        public string FormatTimestamp(DateTime timestamp)
        {
            return timestamp.ToString(fileTimestampFormat, CultureInfo.InvariantCulture);
        }

        private void ValidateCategoryLevelOverrides(List<string> errors)
        {
            if (categoryLevelOverrides == null)
            {
                errors.Add("Category level overrides must not be null.");
                return;
            }

            for (int index = 0; index < categoryLevelOverrides.Count; index++)
            {
                LogCategoryLevelSetting setting = categoryLevelOverrides[index];
                if (setting == null || string.IsNullOrWhiteSpace(setting.CategoryPrefix))
                {
                    errors.Add($"Category level override #{index} must have a category prefix.");
                }
            }
        }

        private void ValidateMissionDrawDiagnostics(List<string> errors)
        {
            if (missionDrawDiagnostics.LogLevel == LogLevel.None)
            {
                errors.Add("Mission draw diagnostics log level must not be None.");
            }

            if (missionDrawDiagnostics.LogCandidateCount < 0)
            {
                errors.Add("Mission draw diagnostics log candidate count must not be negative.");
            }

            if (missionDrawDiagnostics.CsvCandidateCount < 0)
            {
                errors.Add("Mission draw diagnostics CSV candidate count must not be negative.");
            }

            ValidateDirectoryName(
                errors,
                missionDrawDiagnostics.CsvDirectoryName,
                "Mission draw diagnostics CSV directory name");
        }

        private void ValidateTimestampFormat(List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(fileTimestampFormat))
            {
                errors.Add("File timestamp format must not be blank.");
                return;
            }

            try
            {
                string formattedTimestamp = DateTime.MinValue.ToString(
                    fileTimestampFormat,
                    CultureInfo.InvariantCulture);
                if (formattedTimestamp.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    errors.Add($"File timestamp format produces invalid file name characters: {fileTimestampFormat}.");
                }
            }
            catch (FormatException)
            {
                errors.Add($"File timestamp format is invalid: {fileTimestampFormat}.");
            }
        }

        private static void ValidateDirectoryName(List<string> errors, string directoryName, string settingName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
            {
                errors.Add($"{settingName} must not be blank.");
                return;
            }

            if (directoryName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errors.Add($"{settingName} contains invalid characters: {directoryName}.");
            }
        }
    }

    /// <summary>
    /// カテゴリ別の最低LogLevel上書きをInspectorで編集するSerializable設定です。
    /// </summary>
    [Serializable]
    public sealed class LogCategoryLevelSetting
    {
        [SerializeField]
        private string categoryPrefix;

        [SerializeField]
        private LogLevel minimumLevel = LogLevel.Information;

        /// <summary>
        /// 最低LogLevelを上書きするカテゴリ名の接頭辞を取得します。
        /// </summary>
        public string CategoryPrefix => categoryPrefix;

        /// <summary>
        /// 該当カテゴリへ適用する最低LogLevelを取得します。
        /// </summary>
        public LogLevel MinimumLevel => minimumLevel;
    }

    /// <summary>
    /// Mission抽選診断のLog出力とCSV出力の設定をInspectorで編集するSerializable設定です。
    /// </summary>
    [Serializable]
    public sealed class MissionDrawDiagnosticsSetting
    {
        private const int DefaultLogCandidateCount = 10;
        private const int DefaultCsvCandidateCount = 10;
        private const string DefaultCsvDirectoryName = "MissionDraw";

        [SerializeField]
        private LogLevel logLevel = LogLevel.Debug;

        [SerializeField]
        private int logCandidateCount = DefaultLogCandidateCount;

        [SerializeField]
        private bool isCsvEnabled = true;

        [SerializeField]
        private int csvCandidateCount = DefaultCsvCandidateCount;

        [SerializeField]
        private string csvDirectoryName = DefaultCsvDirectoryName;

        /// <summary>
        /// Log出力に使うLogLevelを取得します。
        /// </summary>
        public LogLevel LogLevel => logLevel;

        /// <summary>
        /// Log出力に含める順位上位の候補数を取得します。
        /// </summary>
        public int LogCandidateCount => logCandidateCount;

        /// <summary>
        /// CSV出力を有効にするかどうかを取得します。
        /// </summary>
        public bool IsCsvEnabled => isCsvEnabled;

        /// <summary>
        /// CSV出力に含める順位上位の候補数を取得します。
        /// </summary>
        public int CsvCandidateCount => csvCandidateCount;

        /// <summary>
        /// CSVファイルを配置する種類別ディレクトリ名を取得します。
        /// </summary>
        public string CsvDirectoryName => csvDirectoryName;
    }
}
