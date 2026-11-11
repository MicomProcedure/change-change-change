using System;

namespace Ccc.Presentation
{
    /// <summary>
    /// Result画面の描画に必要な変換済み表示状態です。
    /// </summary>
    public sealed class GameplayResultViewStateDto
    {
        /// <summary>
        /// 今回のSessionのスコアを、接頭辞を含まない表示用文字列で取得します。
        /// </summary>
        public string ScoreText { get; }

        /// <summary>
        /// 今回のSessionのスコアで到達した評価段階の数を取得します。
        /// </summary>
        public int AchievedRankCount { get; }

        /// <summary>
        /// 今回のSessionでハイスコアを更新したかどうかを取得します。
        /// </summary>
        public bool IsNewHighScore { get; }

        /// <summary>
        /// Result画面の表示状態を生成します。
        /// </summary>
        /// <param name="scoreText">接頭辞を含まないスコアの表示用文字列。</param>
        /// <param name="achievedRankCount">今回のSessionのスコアで到達した評価段階の数。0以上。</param>
        /// <param name="isNewHighScore">今回のSessionでハイスコアを更新したかどうか。</param>
        /// <exception cref="ArgumentNullException">scoreTextがnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">achievedRankCountが負の値の場合。</exception>
        public GameplayResultViewStateDto(
            string scoreText,
            int achievedRankCount,
            bool isNewHighScore)
        {
            if (achievedRankCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(achievedRankCount),
                    achievedRankCount,
                    "Achieved rank count must be zero or positive.");
            }

            ScoreText = scoreText ?? throw new ArgumentNullException(nameof(scoreText));
            AchievedRankCount = achievedRankCount;
            IsNewHighScore = isNewHighScore;
        }
    }
}
