using System;
using Ccc.Core.Model;

namespace Ccc.Presentation
{
    /// <summary>
    /// GameModeとGameDifficultyの組ごとに決まる、Gameplay画面の表示設定です。
    /// </summary>
    public sealed class GameplayDisplaySettingsDto
    {
        /// <summary>
        /// 表示するSessionの難易度を取得します。
        /// </summary>
        public GameDifficulty Difficulty { get; }

        /// <summary>
        /// 式を表示するスタイルを取得します。
        /// </summary>
        public ExpressionDisplayStyle ExpressionDisplayStyle { get; }

        /// <summary>
        /// 指定された値から表示設定を生成します。
        /// </summary>
        /// <param name="expressionDisplayStyle">式を表示するスタイル。</param>
        /// <param name="difficulty">表示するSessionの難易度。</param>
        /// <exception cref="ArgumentOutOfRangeException">難易度が未定義の場合。</exception>
        public GameplayDisplaySettingsDto(
            ExpressionDisplayStyle expressionDisplayStyle,
            GameDifficulty difficulty)
        {
            if (!Enum.IsDefined(typeof(GameDifficulty), difficulty))
            {
                throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null);
            }

            ExpressionDisplayStyle = expressionDisplayStyle;
            Difficulty = difficulty;
        }
    }
}
