using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 全ての保存済みハイスコア記録を、バージョンに関係なく使い続ける互換方針です。
    /// </summary>
    public sealed class KeepAllHighScoreCompatibilityPolicy : IHighScoreCompatibilityPolicy
    {
        /// <summary>
        /// 保存済みの記録に対する扱いを決定します。常にKeepを返します。
        /// </summary>
        /// <param name="gameMode">記録のゲームモード。</param>
        /// <param name="difficulty">記録のゲーム難易度。</param>
        /// <param name="record">保存済みの記録。</param>
        /// <param name="currentAppVersion">実行中のアプリケーションバージョン。</param>
        /// <returns>Keepの決定。</returns>
        public HighScoreCompatibilityDecision Decide(
            GameMode gameMode,
            GameDifficulty difficulty,
            HighScoreRecord record,
            string currentAppVersion)
        {
            return HighScoreCompatibilityDecision.Keep();
        }
    }
}
