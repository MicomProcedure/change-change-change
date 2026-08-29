#nullable enable
using System;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 一様確率（均等確率）でインデックスを選択するユーティリティクラス。
    /// </summary>
    public static class UniformIndexSelector
    {
        /// <summary>
        /// 指定された要素数の中から均等確率でインデックスを抽選します。
        /// </summary>
        /// <param name="count">要素数（1以上）。</param>
        /// <param name="sample">0以上1未満の固定sample。</param>
        /// <returns>抽選されたインデックス（0以上 count 未満）。</returns>
        public static int SelectIndex(int count, double sample)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be greater than zero.");
            }

            if (double.IsNaN(sample) || double.IsInfinity(sample) || sample < 0d || sample >= 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(sample), sample, "Sample must be finite and in the range [0, 1).");
            }

            return Math.Min((int)(sample * count), count - 1);
        }
    }
}
