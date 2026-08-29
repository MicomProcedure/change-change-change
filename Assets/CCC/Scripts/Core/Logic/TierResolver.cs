#nullable enable
using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// セクション番号およびゲーム設定に基づいて、現在の Tier を決定するクラス。
    /// </summary>
    public static class TierResolver
    {
        /// <summary>
        /// 現在のセクション番号とゲーム設定から現在の Tier を決定します。
        /// </summary>
        /// <param name="section">現在のセクション番号（1以上）。</param>
        /// <param name="config">ゲームプレイ設定。</param>
        /// <returns>現在の Tier 番号（1以上）。</returns>
        public static int ResolveCurrentTier(int section, GameplayConfig config)
        {
            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, "Section must be greater than zero.");
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return config.TierProgression.GetTier(section);
        }
    }
}
