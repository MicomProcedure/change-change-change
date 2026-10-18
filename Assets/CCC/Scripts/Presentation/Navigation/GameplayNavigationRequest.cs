using System;
using Ccc.Core.Model;

namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// GameScene開始時に必要な設定を保持する不変Requestです。
    /// </summary>
    public sealed class GameplayNavigationRequest
    {
        /// <summary>ゲームモードを取得します。</summary>
        public GameMode GameMode { get; }

        /// <summary>ゲーム難易度を取得します。</summary>
        public GameDifficulty Difficulty { get; }

        /// <summary>OptionMenuの設定Snapshotを取得します。</summary>
        public OptionSettingsSnapshot OptionSettings { get; }

        /// <summary>
        /// GameplayNavigationRequestを生成します。
        /// </summary>
        /// <param name="gameMode">ゲームモードです。</param>
        /// <param name="difficulty">ゲーム難易度です。</param>
        /// <param name="optionSettings">設定Snapshotです。</param>
        public GameplayNavigationRequest(
            GameMode gameMode,
            GameDifficulty difficulty,
            OptionSettingsSnapshot optionSettings)
        {
            if (!Enum.IsDefined(typeof(GameMode), gameMode))
            {
                throw new ArgumentOutOfRangeException(nameof(gameMode));
            }

            if (!Enum.IsDefined(typeof(GameDifficulty), difficulty))
            {
                throw new ArgumentOutOfRangeException(nameof(difficulty));
            }

            if (optionSettings == null)
            {
                throw new ArgumentNullException(nameof(optionSettings));
            }

            GameMode = gameMode;
            Difficulty = difficulty;
            OptionSettings = optionSettings;
        }
    }
}