using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// SectionとGameplay設定から、Mission抽選で目標式適合割合を選ぶ範囲を決める部品です。
    /// </summary>
    public interface ITargetExpressionMatchRatioRangeResolver
    {
        /// <summary>
        /// 抽選対象のSectionで目標式適合割合を選ぶ範囲を決めます。同じ引数には常に同じ範囲を返し、乱数を消費しません。
        /// </summary>
        /// <param name="section">抽選対象のSection。1以上とします。</param>
        /// <param name="config">抽選に使うGameplay設定。</param>
        /// <returns>目標式適合割合を選ぶ範囲。</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="config"/> がnullの場合。</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="section"/> が1未満の場合。</exception>
        TargetExpressionMatchRatioRange Resolve(int section, GameplayConfig config);
    }
}
