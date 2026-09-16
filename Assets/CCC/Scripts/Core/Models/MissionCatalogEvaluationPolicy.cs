using System;
using System.Collections.Generic;

namespace Ccc.Core.Model
{
    /// <summary>
    /// Mission候補をCatalogへ採用するための調整可能な評価規則を保持します。
    /// </summary>
    public sealed class MissionCatalogEvaluationPolicy
    {
        private readonly HashSet<MissionKind> _matchRatioAppliedKinds;

        /// <summary>適合率の包含下限を取得します。</summary>
        public Rational MinimumMatchRatio { get; }

        /// <summary>適合率の包含上限を取得します。</summary>
        public Rational MaximumMatchRatio { get; }

        /// <summary>通常Kindの最小適合件数を取得します。</summary>
        public int MinimumMatchCount { get; }

        /// <summary>ZeroおよびPrimeの最小適合件数を取得します。</summary>
        public int ZeroAndPrimeMinimumMatchCount { get; }

        /// <summary>
        /// Catalog評価規則を生成します。
        /// </summary>
        /// <param name="minimumMatchRatio">適合率下限。</param>
        /// <param name="maximumMatchRatio">適合率上限。</param>
        /// <param name="minimumMatchCount">通常Kindの最小適合件数。</param>
        /// <param name="zeroAndPrimeMinimumMatchCount">ZeroおよびPrimeの最小適合件数。</param>
        /// <param name="matchRatioAppliedKinds">適合率範囲を適用するKind。</param>
        public MissionCatalogEvaluationPolicy(
            Rational minimumMatchRatio,
            Rational maximumMatchRatio,
            int minimumMatchCount,
            int zeroAndPrimeMinimumMatchCount,
            IEnumerable<MissionKind> matchRatioAppliedKinds)
        {
            Rational zero = Rational.Create(0, 1);
            Rational one = Rational.Create(1, 1);
            if (minimumMatchRatio.Denominator <= 0 ||
                minimumMatchRatio < zero ||
                minimumMatchRatio > one)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumMatchRatio),
                    minimumMatchRatio,
                    null);
            }

            if (maximumMatchRatio.Denominator <= 0 ||
                maximumMatchRatio < minimumMatchRatio ||
                maximumMatchRatio > one)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumMatchRatio),
                    maximumMatchRatio,
                    null);
            }

            if (minimumMatchCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumMatchCount),
                    minimumMatchCount,
                    null);
            }

            if (zeroAndPrimeMinimumMatchCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(zeroAndPrimeMinimumMatchCount),
                    zeroAndPrimeMinimumMatchCount,
                    null);
            }

            if (matchRatioAppliedKinds == null)
            {
                throw new ArgumentNullException(nameof(matchRatioAppliedKinds));
            }

            _matchRatioAppliedKinds = new HashSet<MissionKind>();
            foreach (MissionKind kind in matchRatioAppliedKinds)
            {
                MissionKindDefinitionCatalog.Get(kind);
                _matchRatioAppliedKinds.Add(kind);
            }

            MinimumMatchRatio = minimumMatchRatio;
            MaximumMatchRatio = maximumMatchRatio;
            MinimumMatchCount = minimumMatchCount;
            ZeroAndPrimeMinimumMatchCount = zeroAndPrimeMinimumMatchCount;
        }

        /// <summary>
        /// 指定されたKindに適合率範囲を適用するかを取得します。
        /// </summary>
        /// <param name="kind">判定対象のMissionKind。</param>
        /// <returns>適用する場合はtrue。</returns>
        public bool AppliesMatchRatioRange(MissionKind kind)
        {
            MissionKindDefinitionCatalog.Get(kind);
            return _matchRatioAppliedKinds.Contains(kind);
        }

        /// <summary>
        /// 指定されたKindに必要な最小適合件数を取得します。
        /// </summary>
        /// <param name="kind">判定対象のMissionKind。</param>
        /// <returns>必要最小件数。</returns>
        public int GetMinimumMatchCount(MissionKind kind)
        {
            MissionKindDefinitionCatalog.Get(kind);
            return kind == MissionKind.Zero || kind == MissionKind.Prime
                ? ZeroAndPrimeMinimumMatchCount
                : MinimumMatchCount;
        }
    }
}
