using System;
using Ccc.Presentation.ScoreRank;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// 評価段階の基準の生成時検証と、スコアから到達段階数を求める規則を検証します。
    /// </summary>
    public sealed class ScoreRankThresholdsTests
    {
        private static readonly int[] StandardRequiredScores = { 10, 20, 30 };

        /// <summary>
        /// 必要スコア以上で段階に到達し、未満では到達しないことを境界値で検証します。
        /// </summary>
        /// <param name="score">評価するスコア。</param>
        /// <param name="expectedRankCount">期待する到達段階数。</param>
        [TestCase(0, 0)]
        [TestCase(9, 0)]
        [TestCase(10, 1)]
        [TestCase(19, 1)]
        [TestCase(20, 2)]
        [TestCase(29, 2)]
        [TestCase(30, 3)]
        [TestCase(1000, 3)]
        public void CountAchievedRanks_ShouldCountRanksWhoseRequiredScoreIsReached(int score, int expectedRankCount)
        {
            var thresholds = new ScoreRankThresholds(StandardRequiredScores);

            int achievedRankCount = thresholds.CountAchievedRanks(score);

            Assert.That(achievedRankCount, Is.EqualTo(expectedRankCount));
        }

        /// <summary>
        /// 負のスコアを拒否することを検証します。
        /// </summary>
        [Test]
        public void CountAchievedRanks_WhenScoreIsNegative_ShouldThrow()
        {
            var thresholds = new ScoreRankThresholds(StandardRequiredScores);

            Assert.That(
                () => thresholds.CountAchievedRanks(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("score"));
        }

        /// <summary>
        /// 段階数と必要スコアが入力どおりに公開されることを検証します。
        /// </summary>
        [Test]
        public void Constructor_WhenRequiredScoresAreValid_ShouldExposeRanks()
        {
            var thresholds = new ScoreRankThresholds(StandardRequiredScores);

            Assert.That(thresholds.RankCount, Is.EqualTo(3));
            Assert.That(thresholds.RequiredScores, Is.EqualTo(StandardRequiredScores));
        }

        /// <summary>
        /// 生成後に入力元の配列を変更しても基準が変わらないことを検証します。
        /// </summary>
        [Test]
        public void Constructor_WhenSourceIsModifiedAfterCreation_ShouldKeepOriginalRequiredScores()
        {
            int[] source = { 10, 20, 30 };
            var thresholds = new ScoreRankThresholds(source);

            source[0] = 15;

            Assert.That(thresholds.CountAchievedRanks(10), Is.EqualTo(1));
        }

        /// <summary>
        /// nullを拒否することを検証します。
        /// </summary>
        [Test]
        public void Constructor_WhenRequiredScoresIsNull_ShouldThrow()
        {
            Assert.That(
                () => new ScoreRankThresholds(null),
                Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("requiredScores"));
        }

        /// <summary>
        /// 空のリストを拒否することを検証します。
        /// </summary>
        [Test]
        public void Constructor_WhenRequiredScoresIsEmpty_ShouldThrow()
        {
            Assert.That(
                () => new ScoreRankThresholds(Array.Empty<int>()),
                Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("requiredScores"));
        }

        /// <summary>
        /// 0以下の必要スコアを拒否することを検証します。
        /// </summary>
        /// <param name="invalidRequiredScore">不正な必要スコア。</param>
        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WhenRequiredScoreIsNotPositive_ShouldThrow(int invalidRequiredScore)
        {
            Assert.That(
                () => new ScoreRankThresholds(new[] { invalidRequiredScore, 20 }),
                Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("requiredScores"));
        }

        /// <summary>
        /// 厳密に増加していない必要スコアを拒否することを検証します。
        /// </summary>
        /// <param name="first">1段階目の必要スコア。</param>
        /// <param name="second">2段階目の必要スコア。</param>
        [TestCase(20, 20)]
        [TestCase(20, 10)]
        public void Constructor_WhenRequiredScoresAreNotStrictlyIncreasing_ShouldThrow(int first, int second)
        {
            Assert.That(
                () => new ScoreRankThresholds(new[] { first, second }),
                Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("requiredScores"));
        }
    }
}
