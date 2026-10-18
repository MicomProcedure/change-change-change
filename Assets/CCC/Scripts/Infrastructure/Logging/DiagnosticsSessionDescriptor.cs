using System;
using Ccc.Core.Model;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 診断出力で複数Sessionを識別・比較するためのSession識別情報を保持する不変値です。
    /// </summary>
    public sealed class DiagnosticsSessionDescriptor
    {
        /// <summary>
        /// Sessionを一意に識別する文字列を取得します。
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Sessionで使用したGameplay設定の表示名を取得します。
        /// </summary>
        public string ConfigDisplayName { get; }

        /// <summary>
        /// Sessionで選択されたGameplay設定のGameModeを取得します。
        /// </summary>
        public GameMode GameMode { get; }

        /// <summary>
        /// Sessionで選択されたGameplay設定のDifficultyを取得します。
        /// </summary>
        public GameDifficulty Difficulty { get; }

        /// <summary>
        /// Session識別情報を生成します。
        /// </summary>
        /// <param name="sessionId">Sessionを一意に識別する文字列。</param>
        /// <param name="configDisplayName">Gameplay設定の表示名。</param>
        /// <param name="gameMode">SessionのGameMode。</param>
        /// <param name="difficulty">SessionのDifficulty。</param>
        /// <exception cref="ArgumentException">SessionIdが空白の場合。</exception>
        /// <exception cref="ArgumentNullException">設定表示名がnullの場合。</exception>
        public DiagnosticsSessionDescriptor(
            string sessionId,
            string configDisplayName,
            GameMode gameMode,
            GameDifficulty difficulty)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("Session id must not be blank.", nameof(sessionId));
            }

            SessionId = sessionId;
            ConfigDisplayName = configDisplayName ?? throw new ArgumentNullException(nameof(configDisplayName));
            GameMode = gameMode;
            Difficulty = difficulty;
        }
    }
}
