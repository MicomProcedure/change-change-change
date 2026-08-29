#nullable enable
using System;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// <see cref="System.Random"/> を抽選用sampleへ変換する乱数源。
    /// </summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        /// <summary>
        /// <see cref="SystemRandomSource"/> の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="random">使用する乱数生成器。</param>
        public SystemRandomSource(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>
        /// 0以上1未満の乱数sampleを取得します。
        /// </summary>
        /// <returns>0以上1未満のsample。</returns>
        public double NextSample() => _random.NextDouble();
    }
}
