#nullable enable
using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// セクション番号およびゲーム設定に基づいて、抽選対象となる Tier を決定するクラス。
    /// </summary>
    public static class TierResolver
    {
        /// <summary>
        /// 現在のセクション番号、ゲーム設定、乱数生成器から抽選対象の Tier を決定します。
        /// </summary>
        /// <param name="section">現在のセクション番号（1以上）。</param>
        /// <param name="config">ゲームプレイ設定。</param>
        /// <param name="randomSource">乱数sampleの供給元。</param>
        /// <returns>決定された Tier 番号（1以上）。</returns>
        public static int ResolveTier(int section, GameplayConfig config, IRandomSource randomSource)
        {
            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, "Section must be greater than zero.");
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            int currentMaxTier = config.TierProgression.GetTier(section);

            // Tier 1 の場合は下位 Tier が存在しないためそのまま Tier 1 を返却
            if (currentMaxTier <= 1)
            {
                return 1;
            }

            // CurrentTierDrawWeight と LowerTierDrawWeight による重み付き選択
            var weights = new[] { config.CurrentTierDrawWeight, config.LowerTierDrawWeight };
            int selectedWeightIndex = WeightedIndexSelector.SelectIndex(weights, randomSource.NextSample());

            if (selectedWeightIndex == 0)
            {
                // Current Tier を選択
                return currentMaxTier;
            }

            // Lower Tier: 1 〜 (currentMaxTier - 1) のいずれかを均等抽選
            int lowerTierCount = currentMaxTier - 1;
            int lowerTierOffset = UniformIndexSelector.SelectIndex(lowerTierCount, randomSource.NextSample());
            return 1 + lowerTierOffset;
        }
    }
}
