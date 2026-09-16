using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 固定Kind定義と分析済み適合率からMissionCandidateの確定Tierを解決します。
    /// </summary>
    public static class MissionCandidateTierResolver
    {
        private const int BetweenWideTier = 2;
        private const int BetweenNarrowTier = 3;

        /// <summary>
        /// 候補条件の確定Tierを解決します。
        /// </summary>
        /// <param name="condition">Tierを解決する候補条件。</param>
        /// <param name="matchRatio">分析済みの正確な適合率。</param>
        /// <param name="betweenTierTwoThreshold">BetweenをTier 2とする包含閾値。</param>
        /// <returns>確定Tier。</returns>
        public static int Resolve(
            MissionCondition condition,
            Rational matchRatio,
            Rational betweenTierTwoThreshold)
        {
            ValidateRatio(matchRatio, nameof(matchRatio));
            ValidateRatio(betweenTierTwoThreshold, nameof(betweenTierTwoThreshold));
            MissionKindDefinition definition =
                MissionKindDefinitionCatalog.Get(condition.Kind);

            if (condition.IsNegated)
            {
                if (!definition.CanGenerateNegated)
                {
                    throw new ArgumentException(
                        $"MissionKind.{condition.Kind} does not allow Negated candidates.",
                        nameof(condition));
                }

                return definition.NegatedTier;
            }

            if (definition.HasFixedPositiveTier)
            {
                return definition.FixedPositiveTier;
            }

            return matchRatio >= betweenTierTwoThreshold
                ? BetweenWideTier
                : BetweenNarrowTier;
        }

        private static void ValidateRatio(Rational ratio, string parameterName)
        {
            if (ratio.Denominator <= 0 ||
                ratio < Rational.Create(0, 1) ||
                ratio > Rational.Create(1, 1))
            {
                throw new ArgumentOutOfRangeException(parameterName, ratio, null);
            }
        }
    }
}
