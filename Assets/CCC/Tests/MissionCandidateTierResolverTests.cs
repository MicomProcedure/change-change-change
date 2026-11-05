using Ccc.Core.Logic;
using Ccc.Core.Model;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// Kind定義の肯定Tier規則に従ってMissionCandidateの確定Tierを解決することを検証します。
    /// </summary>
    public sealed class MissionCandidateTierResolverTests
    {
        private const float BetweenTierTwoThreshold = 0.25f;

        /// <summary>
        /// 固定Tierの肯定条件は適合率とパラメータ到達可能性に関係なく固定Tierになることを検証します。
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void Resolve_FixedTierKind_ShouldReturnFixedTier(bool isParameterReachable)
        {
            int tier = Resolve(MissionCondition.Modulo(3, 1), 0.5f, isParameterReachable);

            Assert.That(tier, Is.EqualTo(4));
        }

        /// <summary>
        /// Betweenは適合率が閾値以上ならTier2、未満ならTier3になることを検証します。
        /// </summary>
        [TestCase(0.25f, 2)]
        [TestCase(0.5f, 2)]
        [TestCase(0.2f, 3)]
        public void Resolve_Between_ShouldResolveTierByMatchRatioThreshold(float matchRatio, int expectedTier)
        {
            int tier = Resolve(MissionCondition.Between(1, 3), matchRatio, isParameterReachable: false);

            Assert.That(tier, Is.EqualTo(expectedTier));
        }

        /// <summary>
        /// 約数はNが到達可能ならTier3、到達不可ならTier4になることを検証します。
        /// </summary>
        [TestCase(true, 3)]
        [TestCase(false, 4)]
        public void Resolve_Divisor_ShouldResolveTierByParameterReachability(
            bool isParameterReachable,
            int expectedTier)
        {
            int tier = Resolve(MissionCondition.Create(MissionKind.Divisor, 12), 0.4f, isParameterReachable);

            Assert.That(tier, Is.EqualTo(expectedTier));
        }

        /// <summary>
        /// Negated条件は肯定Tier規則に関係なく固定のNegated Tierになることを検証します。
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        public void Resolve_NegatedDivisor_ShouldReturnNegatedTier(bool isParameterReachable)
        {
            int tier = Resolve(MissionCondition.Create(MissionKind.Divisor, 12, true), 0.6f, isParameterReachable);

            Assert.That(tier, Is.EqualTo(4));
        }

        private static int Resolve(MissionCondition condition, float matchRatio, bool isParameterReachable)
        {
            return MissionCandidateTierResolver.Resolve(
                condition,
                Rational.FromSingle(matchRatio),
                Rational.FromSingle(BetweenTierTwoThreshold),
                isParameterReachable);
        }
    }
}
