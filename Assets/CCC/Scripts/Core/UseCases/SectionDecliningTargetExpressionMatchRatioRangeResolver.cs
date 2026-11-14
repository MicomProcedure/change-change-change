using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Sectionが進むごとに下限を直線的に下げ、設定の目標式適合割合を上限とする範囲を決める部品です。
    /// </summary>
    public sealed class SectionDecliningTargetExpressionMatchRatioRangeResolver : ITargetExpressionMatchRatioRangeResolver
    {
        /// <summary>
        /// 下限を「目標式適合割合 − Sectionごとの低下量 ×（Section − 1）」とし、設定の最小値で止めた範囲を返します。
        /// </summary>
        /// <param name="section">抽選対象のSection。1以上とします。</param>
        /// <param name="config">抽選に使うGameplay設定。</param>
        /// <returns>
        /// 上限が <see cref="GameplayConfig.TargetExpressionMatchRatio"/>、下限がSectionに応じて下げた値または
        /// <see cref="GameplayConfig.MinimumTargetExpressionMatchRatio"/> の大きい方となる範囲。Section 1では下限と上限が等しくなります。
        /// </returns>
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

            float upperBound = config.TargetExpressionMatchRatio;
            double reducedLowerBound =
                upperBound - ((double)config.TargetExpressionMatchRatioReductionPerSection * (section - 1));
            float lowerBound = reducedLowerBound < config.MinimumTargetExpressionMatchRatio
                ? config.MinimumTargetExpressionMatchRatio
                : (float)reducedLowerBound;
            return new TargetExpressionMatchRatioRange(lowerBound, upperBound);
        }
    }
}
