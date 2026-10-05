using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        /// Inspector設定を検証し、検出されたエラー一覧を返します。
        /// </summary>
        /// <returns>技術的な設定エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (categoryLevelOverrides == null)
            {
                errors.Add("Category level overrides must not be null.");
            }
            else
            {
                for (int index = 0; index < categoryLevelOverrides.Count; index++)
                {
                    LogCategoryLevelSetting setting = categoryLevelOverrides[index];
                    if (setting == null)
                    {
                        errors.Add($"Category level override #{index} must not be null.");
                        continue;
                    }

                    setting.Validate(errors, index);
                }
            }

            if (string.IsNullOrWhiteSpace(rootDirectoryRelativePath) || Path.IsPathRooted(rootDirectoryRelativePath))
            {
                errors.Add("Root directory path must be a non-blank path relative to the project root.");
            }

            ValidateTimestampFormat(errors);
            LogSettingsValidation.ValidateDirectoryName(errors, generalDirectoryName, "General directory name");
            if (missionDrawDiagnostics == null)
            {
                errors.Add("Mission draw diagnostics setting must not be null.");
            }
            else
            {
                missionDrawDiagnostics.Validate(errors);
            }

            return errors;
        }

        /// <summary>
        /// Inspector設定からアプリケーション全体のLog出力設定を生成します。
        /// </summary>
        /// <returns>Log出力設定。</returns>
        /// <exception cref="InvalidOperationException">Inspector設定が不正な場合。</exception>
        public LogOutputOptions CreateLogOutputOptions()
        {
            ThrowIfInvalid();
            return new LogOutputOptions(
                minimumLevel,
                categoryLevelOverrides.Select(setting => setting.CreateOverride()).ToArray(),
                isUnityConsoleEnabled,
                isGeneralFileEnabled,
                generalDirectoryName);
        }

        /// <summary>
        /// Inspector設定とプロジェクトルートからLogファイルのディレクトリ設定を生成します。
        /// </summary>
        /// <param name="projectRootPath">相対ルートの基準にするプロジェクトルートの絶対パス。</param>
        /// <returns>Logファイルのディレクトリ設定。</returns>
        /// <exception cref="InvalidOperationException">Inspector設定が不正な場合。</exception>
        /// <exception cref="ArgumentException">プロジェクトルートが絶対パスでない場合。</exception>
        public LogDirectoryOptions CreateLogDirectoryOptions(string projectRootPath)
        {
            ThrowIfInvalid();
            if (string.IsNullOrWhiteSpace(projectRootPath) || !Path.IsPathRooted(projectRootPath))
            {
                throw new ArgumentException("Project root path must be an absolute path.", nameof(projectRootPath));
            }

            return new LogDirectoryOptions(
                Path.GetFullPath(Path.Combine(projectRootPath, rootDirectoryRelativePath)),
                fileTimestampFormat);
        }

        /// <summary>
        /// Inspector設定からMission抽選診断設定を生成します。
        /// </summary>
        /// <returns>Mission抽選診断設定。</returns>
        /// <exception cref="InvalidOperationException">Inspector設定が不正な場合。</exception>
        public MissionDrawDiagnosticsOptions CreateMissionDrawDiagnosticsOptions()
        {
            ThrowIfInvalid();
            return missionDrawDiagnostics.CreateOptions();
        }

        private void ThrowIfInvalid()
        {
            IReadOnlyList<string> errors = Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Log settings validation failed: " + string.Join(" | ", errors));
            }
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
    }
}
