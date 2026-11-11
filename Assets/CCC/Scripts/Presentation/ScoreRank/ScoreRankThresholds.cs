using System;
using System.Collections.Generic;

namespace Ccc.Presentation.ScoreRank
{
    /// <summary>
    /// スコアの評価段階（ScoreRank）を、段階ごとに必要なスコアの昇順リストで表す不変の基準です。
    /// 評価段階を画面上でどう見せるか（星など）は扱いません。
    /// </summary>
    public sealed class ScoreRankThresholds
    {
        private readonly int[] _requiredScores;

        /// <summary>
        /// 評価段階の数を取得します。
        /// </summary>
        public int RankCount
        {
            get { return _requiredScores.Length; }
        }

        /// <summary>
        /// 各評価段階に到達するために必要なスコアを、低い段階から順に取得します。
        /// </summary>
        public IReadOnlyList<int> RequiredScores
        {
            get { return _requiredScores; }
        }

        /// <summary>
        /// 評価段階の基準を生成します。
        /// </summary>
        /// <param name="requiredScores">各評価段階に必要なスコア。1以上で、低い段階から順に厳密に増加する値。</param>
        /// <exception cref="ArgumentNullException"><paramref name="requiredScores"/>がnullの場合。</exception>
        /// <exception cref="ArgumentException"><paramref name="requiredScores"/>が空、または厳密に増加していない場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="requiredScores"/>に0以下の値が含まれる場合。</exception>
        public ScoreRankThresholds(IReadOnlyList<int> requiredScores)
        {
            if (requiredScores == null)
            {
                throw new ArgumentNullException(nameof(requiredScores));
            }

            if (requiredScores.Count == 0)
            {
                throw new ArgumentException(
                    "Score rank thresholds must contain at least one required score.",
                    nameof(requiredScores));
            }

            _requiredScores = new int[requiredScores.Count];
            for (int index = 0; index < requiredScores.Count; index++)
            {
                int requiredScore = requiredScores[index];
                if (requiredScore <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(requiredScores),
                        requiredScore,
                        $"Required score at index {index} must be greater than zero.");
                }

                if (index > 0 && requiredScore <= _requiredScores[index - 1])
                {
                    throw new ArgumentException(
                        $"Required score at index {index} must be greater than the previous required score.",
                        nameof(requiredScores));
                }

                _requiredScores[index] = requiredScore;
            }
        }

        /// <summary>
        /// 指定スコアで到達した評価段階の数を取得します。必要スコア以上のスコアでその段階に到達したとみなします。
        /// </summary>
        /// <param name="score">評価するスコア。0以上。</param>
        /// <returns>到達した評価段階の数。0以上<see cref="RankCount"/>以下。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="score"/>が負の値の場合。</exception>
        public int CountAchievedRanks(int score)
        {
            if (score < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be zero or positive.");
            }

            int achievedRankCount = 0;
            while (achievedRankCount < _requiredScores.Length &&
                   score >= _requiredScores[achievedRankCount])
            {
                achievedRankCount++;
            }

            return achievedRankCount;
        }
    }
}
