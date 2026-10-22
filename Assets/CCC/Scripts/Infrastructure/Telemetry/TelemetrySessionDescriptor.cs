using System;
using Ccc.Core.Model;

namespace Ccc.Infrastructure.Telemetry
{
    /// <summary>
    /// 1回のGameplay Sessionに共通するプレイデータ送信用の識別情報です。
    /// </summary>
    public sealed class TelemetrySessionDescriptor
    {
        /// <summary>
        /// Sessionごとに生成した匿名の識別子を取得します。
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// ゲーム内のPlayPlatform名を取得します。
        /// </summary>
        public string Platform { get; }

        /// <summary>
        /// UnityのRuntimePlatform名を取得します。
        /// </summary>
        public string RuntimePlatform { get; }

        /// <summary>
        /// 端末モデル名を取得します。iPadとiPhoneの区別に使います。
        /// </summary>
        public string DeviceModel { get; }

        /// <summary>
        /// アプリのバージョンを取得します。
        /// </summary>
        public string AppVersion { get; }

        /// <summary>
        /// 難易度を取得します。
        /// </summary>
        public GameDifficulty Difficulty { get; }

        /// <summary>
        /// 選択されたConfig Entryの表示名を取得します。
        /// </summary>
        public string ConfigName { get; }

        /// <summary>
        /// TelemetrySessionDescriptorを生成します。
        /// </summary>
        /// <param name="sessionId">匿名のSession識別子。</param>
        /// <param name="platform">ゲーム内のPlayPlatform名。</param>
        /// <param name="runtimePlatform">UnityのRuntimePlatform名。</param>
        /// <param name="deviceModel">端末モデル名。</param>
        /// <param name="appVersion">アプリのバージョン。</param>
        /// <param name="difficulty">難易度。</param>
        /// <param name="configName">Config Entryの表示名。</param>
        /// <exception cref="ArgumentException"><paramref name="sessionId"/> が空の場合。</exception>
        public TelemetrySessionDescriptor(
            string sessionId,
            string platform,
            string runtimePlatform,
            string deviceModel,
            string appVersion,
            GameDifficulty difficulty,
            string configName)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("Session id must not be blank.", nameof(sessionId));
            }

            SessionId = sessionId;
            Platform = platform;
            RuntimePlatform = runtimePlatform;
            DeviceModel = deviceModel;
            AppVersion = appVersion;
            Difficulty = difficulty;
            ConfigName = configName;
        }
    }
}
