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

        /// <summary>肯定条件が固定Tierを持つかを取得します。</summary>
        public bool HasFixedPositiveTier { get; }

        /// <summary>肯定条件の固定Tierを取得します。Betweenでは0です。</summary>
        public int FixedPositiveTier { get; }

        /// <summary>Negated候補の生成が許可されるかを取得します。</summary>
        public bool CanGenerateNegated { get; }

        /// <summary>Negated条件の固定Tierを取得します。生成不可の場合は0です。</summary>
        public int NegatedTier { get; }

        internal MissionKindDefinition(
            MissionKind kind,
            MissionParameterStructure parameterStructure,
            int fixedPositiveTier,
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

            bool isBetween = kind == MissionKind.Between;
            if ((isBetween && fixedPositiveTier != 0) ||
                (!isBetween && fixedPositiveTier <= 0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fixedPositiveTier),
                    fixedPositiveTier,
                    null);
            }

            if ((canGenerateNegated && negatedTier <= 0) ||
                (!canGenerateNegated && negatedTier != 0))
            {
                throw new ArgumentOutOfRangeException(nameof(negatedTier), negatedTier, null);
            }

            Kind = kind;
            ParameterStructure = parameterStructure;
            HasFixedPositiveTier = !isBetween;
            FixedPositiveTier = fixedPositiveTier;
            CanGenerateNegated = canGenerateNegated;
            NegatedTier = negatedTier;
        }
    }
}
