using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 保存済みのハイスコア記録を、実行中のバージョンでどう扱うかを決定します。
    /// </summary>
    public interface IHighScoreCompatibilityPolicy
    {
        /// <summary>
        /// 保存済みの記録に対する扱いを決定します。
        /// </summary>
        /// <param name="gameMode">記録のゲームモード。</param>
        /// <param name="difficulty">記録のゲーム難易度。</param>
        /// <param name="record">保存済みの記録。</param>
        /// <param name="currentAppVersion">実行中のアプリケーションバージョン。</param>
        /// <returns>記録の扱い。</returns>
        HighScoreCompatibilityDecision Decide(
            GameMode gameMode,
            GameDifficulty difficulty,
            HighScoreRecord record,
            string currentAppVersion);
    }
}
