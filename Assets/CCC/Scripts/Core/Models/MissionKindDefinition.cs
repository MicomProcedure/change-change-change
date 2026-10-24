using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// MissionKind固有の固定仕様を表す不変モデルです。
    /// </summary>
    public sealed class MissionKindDefinition
    {
        /// <summary>定義対象のMissionKindを取得します。</summary>
        public MissionKind Kind { get; }

        /// <summary>条件が保持するパラメータ構造を取得します。</summary>
        public MissionParameterStructure ParameterStructure { get; }

        /// <summary>肯定条件のTierを確定する規則を取得します。</summary>
        public MissionPositiveTierRule PositiveTierRule { get; }

        /// <summary>
        /// 肯定条件のTierを取得します。固定規則では固定Tier、それ以外の規則では判定基準を満たす場合のTierです。
        /// </summary>
        public int PositiveTier { get; }

        /// <summary>
        /// 判定基準を満たさない場合の肯定条件のTierを取得します。固定規則では0です。
        /// </summary>
        public int CriterionUnmetPositiveTier { get; }

        /// <summary>Negated候補の生成が許可されるかを取得します。</summary>
        public bool CanGenerateNegated { get; }

        /// <summary>Negated条件の固定Tierを取得します。生成不可の場合は0です。</summary>
        public int NegatedTier { get; }

        internal MissionKindDefinition(
            MissionKind kind,
            MissionParameterStructure parameterStructure,
            MissionPositiveTierRule positiveTierRule,
            int positiveTier,
            int criterionUnmetPositiveTier,
            bool canGenerateNegated,
            int negatedTier)
        {
            if (!Enum.IsDefined(typeof(MissionKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            if (!Enum.IsDefined(typeof(MissionParameterStructure), parameterStructure))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(parameterStructure),
                    parameterStructure,
                    null);
            }

            if (!Enum.IsDefined(typeof(MissionPositiveTierRule), positiveTierRule))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(positiveTierRule),
                    positiveTierRule,
                    null);
            }

            if (positiveTierRule == MissionPositiveTierRule.ParameterReachability &&
                parameterStructure != MissionParameterStructure.Single)
            {
                throw new ArgumentException(
                    "Parameter reachability tier rule requires a single-parameter MissionKind.",
                    nameof(positiveTierRule));
            }

            if (positiveTier <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(positiveTier), positiveTier, null);
            }

            bool isFixed = positiveTierRule == MissionPositiveTierRule.Fixed;
            if ((isFixed && criterionUnmetPositiveTier != 0) ||
                (!isFixed && criterionUnmetPositiveTier <= 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(criterionUnmetPositiveTier),
                    criterionUnmetPositiveTier,
                    null);
            }

            if ((canGenerateNegated && negatedTier <= 0) ||
                (!canGenerateNegated && negatedTier != 0))
            {
                throw new ArgumentOutOfRangeException(nameof(negatedTier), negatedTier, null);
            }

            Kind = kind;
            ParameterStructure = parameterStructure;
            PositiveTierRule = positiveTierRule;
            PositiveTier = positiveTier;
            CriterionUnmetPositiveTier = criterionUnmetPositiveTier;
            CanGenerateNegated = canGenerateNegated;
            NegatedTier = negatedTier;
        }
    }
}
