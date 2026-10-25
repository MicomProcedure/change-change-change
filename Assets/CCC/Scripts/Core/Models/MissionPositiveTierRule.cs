namespace Ccc.Core.Model
{
    /// <summary>
    /// 肯定条件のTierをCatalog採用時にどう確定するかを表します。
    /// </summary>
    public enum MissionPositiveTierRule
    {
        /// <summary>Kind定義の固定Tierを使用します。</summary>
        Fixed = 0,

        /// <summary>適合率がBetween Tier判定閾値以上かで2つのTierを切り替えます。</summary>
        MatchRatioThreshold = 1,

        /// <summary>ParameterAが式の計算結果として到達可能かで2つのTierを切り替えます。</summary>
        ParameterReachability = 2
    }
}
