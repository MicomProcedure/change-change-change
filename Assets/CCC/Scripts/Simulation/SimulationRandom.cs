#nullable enable
using System;
using Ccc.Core.UseCases;

namespace Ccc.Simulation
{
    /// <summary>
    /// seedから同じ乱数列を再現する、シミュレーション用の乱数提供元です。
    /// </summary>
    public sealed class SimulationRandom : IRandomSource
    {
        private const float MaximumValueBelowOne = 0.99999994f;

        private readonly Random _random;

        /// <summary>
        /// seedから乱数提供元を生成します。
        /// </summary>
        /// <param name="seed">乱数列を決めるseed。</param>
        public SimulationRandom(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// 0以上1未満の乱数を1つ取得します。
        /// </summary>
        /// <returns>0.0以上1.0未満の乱数。</returns>
        public float NextFloat01()
        {
            float value = (float)_random.NextDouble();
            return value < 1f ? value : MaximumValueBelowOne;
        }

        /// <summary>
        /// 0以上指定数未満の整数を一様に1つ取得します。
        /// </summary>
        /// <param name="count">選択肢の数。</param>
        /// <returns>0以上<paramref name="count"/>未満の整数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">選択肢の数が1未満の場合。</exception>
        public int NextIndex(int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be at least one.");
            }

            return _random.Next(count);
        }
    }
}
