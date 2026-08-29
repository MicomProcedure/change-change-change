#nullable enable
using System;
using System.Collections.Generic;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 重み付きリストからインデックスを選択するユーティリティクラス。
    /// </summary>
    public static class WeightedIndexSelector
    {
        /// <summary>
        /// 指定された重みリストと乱数生成器を用いて、インデックスを重み付き抽選します。
        /// </summary>
        /// <param name="weights">各インデックスの重みリスト（0以上の有限値）。</param>
        /// <param name="random">乱数生成器。</param>
        /// <returns>抽選されたインデックス（0以上 weights.Count 未満）。</returns>
        public static int SelectIndex(IReadOnlyList<float> weights, Random random)
        {
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (weights.Count == 0)
            {
                throw new ArgumentException("Weights collection cannot be empty.", nameof(weights));
            }

            float totalWeight = 0f;
            for (int i = 0; i < weights.Count; i++)
            {
                float weight = weights[i];
                if (float.IsNaN(weight) || float.IsInfinity(weight) || weight < 0f)
                {
                    throw new ArgumentOutOfRangeException(nameof(weights), weight, "All weights must be non-negative finite numbers.");
                }

                totalWeight += weight;
            }

            if (totalWeight <= 0f)
            {
                throw new InvalidOperationException("Sum of weights must be greater than zero.");
            }

            double roll = random.NextDouble() * totalWeight;
            double cumulative = 0.0;

            for (int i = 0; i < weights.Count; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                {
                    return i;
                }
            }

            return weights.Count - 1;
        }
    }
}
