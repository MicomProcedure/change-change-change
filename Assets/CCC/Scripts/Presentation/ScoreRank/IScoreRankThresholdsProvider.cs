using Ccc.Core.Model;

namespace Ccc.Presentation.ScoreRank
{
    /// <summary>
    /// GameModeとGameDifficultyの組ごとに設定された評価段階の基準を提供するポートです。
    /// </summary>
    public interface IScoreRankThresholdsProvider
    {
        /// <summary>
        /// 指定されたGameModeとGameDifficultyの組に設定された評価段階の基準を取得します。
        /// </summary>
        /// <param name="gameMode">基準を取得するゲームモード。</param>
        /// <param name="difficulty">基準を取得するゲーム難易度。</param>
        /// <returns>評価段階の基準。</returns>
        /// <exception cref="System.InvalidOperationException">指定された組の設定が存在しない場合。</exception>
        /// <exception cref="System.ArgumentException">指定された組の基準の設定が規則に違反している場合。</exception>
        ScoreRankThresholds Get(GameMode gameMode, GameDifficulty difficulty);
    }
}
