#nullable enable

namespace Ccc.Core.Logic
{
    /// <summary>
    /// ミッションの抽選戦略を定義するインターフェース。
    /// </summary>
    public interface IMissionDrawStrategy
    {
        /// <summary>
        /// 指定されたコンテキストに基づいてミッションを抽選します。
        /// </summary>
        /// <param name="context">ミッション抽選に必要なコンテキスト情報。</param>
        /// <returns>ミッション抽選の結果。</returns>
        MissionDrawResult Draw(MissionDrawContext context);
    }
}
