using System;
using UnityEngine;

namespace Ccc.Infrastructure.Telemetry
{
    /// <summary>
    /// 1回のSession結果を表す送信用Dtoです。フィールド名がそのままJSONのキーになります。
    /// </summary>
    [Serializable]
    public sealed class SessionRecordDto
    {
        [SerializeField] private string sessionId;
        [SerializeField] private string platform;
        [SerializeField] private string runtimePlatform;
        [SerializeField] private string deviceModel;
        [SerializeField] private string appVersion;
        [SerializeField] private string difficulty;
        [SerializeField] private string configName;
        [SerializeField] private int finalSection;
        [SerializeField] private int correctAnswerCount;
        [SerializeField] private string terminationReason;
        [SerializeField] private string startedAtUtc;
        [SerializeField] private string endedAtUtc;

        /// <summary>
        /// SessionRecordDtoを生成します。
        /// </summary>
        /// <param name="descriptor">Sessionの識別情報。</param>
        /// <param name="finalSection">終了時のSection。</param>
        /// <param name="correctAnswerCount">正解数。</param>
        /// <param name="terminationReason">終了理由名。</param>
        /// <param name="startedAtUtc">開始時刻（ISO 8601, UTC）。</param>
        /// <param name="endedAtUtc">終了時刻（ISO 8601, UTC）。</param>
        /// <exception cref="ArgumentNullException"><paramref name="descriptor"/> がnullの場合。</exception>
        public SessionRecordDto(
            TelemetrySessionDescriptor descriptor,
            int finalSection,
            int correctAnswerCount,
            string terminationReason,
            string startedAtUtc,
            string endedAtUtc)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            sessionId = descriptor.SessionId;
            platform = descriptor.Platform;
            runtimePlatform = descriptor.RuntimePlatform;
            deviceModel = descriptor.DeviceModel;
            appVersion = descriptor.AppVersion;
            difficulty = descriptor.Difficulty.ToString();
            configName = descriptor.ConfigName;
            this.finalSection = finalSection;
            this.correctAnswerCount = correctAnswerCount;
            this.terminationReason = terminationReason;
            this.startedAtUtc = startedAtUtc;
            this.endedAtUtc = endedAtUtc;
        }
    }
}
