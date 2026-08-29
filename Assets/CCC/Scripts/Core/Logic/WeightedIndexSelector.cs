#nullable enable
using System;
using System.Collections.Generic;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 重み付きリストと固定sampleからインデックスを選択するユーティリティクラス。
    /// </summary>
    public static class WeightedIndexSelector
    {
        private const double SelectionBoundaryTolerance = 0.0000001d;

        /// <summary>
        /// 指定された重みリストと乱数生成器を用いて、インデックスを重み付き抽選します。
        /// </summary>
        /// <param name="weights">各インデックスの重みリスト（0以上の有限値）。</param>
        /// <param name="sample">0以上1未満の固定sample。</param>
        /// <returns>抽選されたインデックス（0以上 weights.Count 未満）。</returns>
        public static int SelectIndex(IReadOnlyList<float> weights, double sample)
        {
            if (weights == null)
            {
                throw new ArgumentNullException(nameof(weights));
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

            if (double.IsNaN(sample) || double.IsInfinity(sample) || sample < 0d || sample >= 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(sample), sample, "Sample must be finite and in the range [0, 1).");
            }

            double roll = sample * totalWeight;
            double cumulative = 0.0;

            for (int i = 0; i < weights.Count; i++)
            {
                cumulative += weights[i];
                // float重みを受け取るため、十進数の境界sampleでは丸め誤差を吸収する。
                if (roll < cumulative - SelectionBoundaryTolerance)
                {
                    return i;
                }
            }

            return weights.Count - 1;
        }
    }
}
