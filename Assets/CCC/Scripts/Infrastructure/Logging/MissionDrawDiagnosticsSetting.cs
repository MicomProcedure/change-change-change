using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// Mission抽選診断の出力設定をInspectorで編集するSerializable設定です。
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
        /// Inspector設定を検証し、検出したエラーを追加します。
        /// </summary>
        /// <param name="errors">エラーの追加先。</param>
        public void Validate(List<string> errors)
        {
            if (logLevel == LogLevel.None)
            {
                errors.Add("Mission draw diagnostics log level must not be None.");
            }

            if (logCandidateCount < 0)
            {
                errors.Add("Mission draw diagnostics log candidate count must not be negative.");
            }

            if (csvCandidateCount < 0)
            {
                errors.Add("Mission draw diagnostics CSV candidate count must not be negative.");
            }

            LogSettingsValidation.ValidateDirectoryName(
                errors,
                csvDirectoryName,
                "Mission draw diagnostics CSV directory name");
        }

        /// <summary>
        /// Inspector設定から実行時のMission抽選診断設定を生成します。
        /// </summary>
        /// <returns>Mission抽選診断設定。</returns>
        public MissionDrawDiagnosticsOptions CreateOptions()
        {
            return new MissionDrawDiagnosticsOptions(
                logLevel,
                logCandidateCount,
                isCsvEnabled,
                csvCandidateCount,
                csvDirectoryName);
        }
    }
}
