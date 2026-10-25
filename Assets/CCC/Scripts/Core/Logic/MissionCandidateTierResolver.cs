using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// Kind定義の肯定Tier規則と分析結果からMissionCandidateの確定Tierを解決します。
    /// </summary>
    public static class MissionCandidateTierResolver
    {
        /// <summary>
        /// 候補条件の確定Tierを解決します。
        /// </summary>
        /// <param name="condition">Tierを解決する候補条件。</param>
        /// <param name="matchRatio">分析済みの正確な適合率。</param>
        /// <param name="betweenTierTwoThreshold">適合率閾値規則で基準を満たすとみなす包含閾値。</param>
        /// <param name="isParameterReachable">ParameterAが式の計算結果として到達可能か。</param>
        /// <returns>確定Tier。</returns>
        public static int Resolve(
            MissionCondition condition,
            Rational matchRatio,
            Rational betweenTierTwoThreshold,
            bool isParameterReachable)
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

            switch (definition.PositiveTierRule)
            {
                case MissionPositiveTierRule.Fixed:
                    return definition.PositiveTier;
                case MissionPositiveTierRule.MatchRatioThreshold:
                    return SelectTier(definition, matchRatio >= betweenTierTwoThreshold);
                case MissionPositiveTierRule.ParameterReachability:
                    return SelectTier(definition, isParameterReachable);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(definition.PositiveTierRule),
                        definition.PositiveTierRule,
                        null);
            }
        }

        private static int SelectTier(MissionKindDefinition definition, bool isCriterionMet)
        {
            return isCriterionMet
                ? definition.PositiveTier
                : definition.CriterionUnmetPositiveTier;
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
