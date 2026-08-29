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
        /// <param name="random">乱数生成器。</param>
        /// <returns>抽選されたインデックス（0以上 count 未満）。</returns>
        public static int SelectIndex(int count, Random random)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be greater than zero.");
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            return random.Next(0, count);
        }
    }
}
