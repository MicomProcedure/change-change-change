using Ccc.Core.UseCases;
using UnityEngine;

namespace Ccc.Infrastructure.Random
{
    /// <summary>
    /// UnityEngine.RandomをIRandomSourceへ適合させる実装です。
    /// </summary>
    public sealed class UnityRandomSource : IRandomSource
    {
        private const float MaximumValueBelowOne = 0.99999994f;

        /// <summary>
        /// Unityの乱数を0以上1未満へ正規化して返します。
        /// </summary>
        /// <returns>0.0以上1.0未満の乱数。</returns>
        public float NextFloat01()
        {
            float randomValue = UnityEngine.Random.value;
            if (float.IsNaN(randomValue))
            {
                return 0f;
            }

            if (randomValue >= 1f || float.IsPositiveInfinity(randomValue))
            {
                return MaximumValueBelowOne;
            }

            if (randomValue < 0f || float.IsNegativeInfinity(randomValue))
            {
                return 0f;
            }

            return randomValue;
        }
    }
}
