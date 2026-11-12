using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// スコアを記録した結果を表す不変データです。
    /// </summary>
    public sealed class HighScoreUpdateResult
    {
        /// <summary>
        /// 記録を試みたスコアを取得します。
        /// </summary>
        public int Score { get; }

        /// <summary>
        /// 記録後のハイスコアを取得します。
        /// </summary>
        public int HighScore { get; }

        /// <summary>
        /// ハイスコアを更新したかどうかを取得します。
        /// </summary>
        public bool IsNewHighScore { get; }

        /// <summary>
        /// HighScoreUpdateResultを生成します。
        /// </summary>
        /// <param name="score">記録を試みたスコア。</param>
        /// <param name="highScore">記録後のハイスコア。</param>
        /// <param name="isNewHighScore">ハイスコアを更新したかどうか。</param>
        /// <exception cref="ArgumentOutOfRangeException">scoreが負、またはhighScoreがscore未満の場合。</exception>
        public HighScoreUpdateResult(int score, int highScore, bool isNewHighScore)
        {
            if (score < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score), score, null);
            }

            if (highScore < score)
            {
                throw new ArgumentOutOfRangeException(nameof(highScore), highScore, null);
            }

            Score = score;
            HighScore = highScore;
            IsNewHighScore = isNewHighScore;
        }
    }
}
