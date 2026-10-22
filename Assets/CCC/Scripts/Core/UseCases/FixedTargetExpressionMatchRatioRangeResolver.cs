using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Sectionにかかわらず、設定の目標式適合割合だけを範囲とする部品です。
    /// </summary>
    public sealed class FixedTargetExpressionMatchRatioRangeResolver : ITargetExpressionMatchRatioRangeResolver
    {
        /// <summary>
        /// 設定の目標式適合割合を下限かつ上限とする範囲を返します。
        /// </summary>
        /// <param name="section">抽選対象のSection。1以上とします。</param>
        /// <param name="config">抽選に使うGameplay設定。</param>
        /// <returns>下限と上限が <see cref="GameplayConfig.TargetExpressionMatchRatio"/> に等しい範囲。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> がnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="section"/> が1未満の場合。</exception>
        public TargetExpressionMatchRatioRange Resolve(int section, GameplayConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, "Section must be one or greater.");
            }

            return new TargetExpressionMatchRatioRange(
                config.TargetExpressionMatchRatio,
                config.TargetExpressionMatchRatio);
        }
    }
}
