namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 0以上1未満の乱数を提供するポートです。
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>
        /// 0.0以上1.0未満の乱数を1つ取得します。
        /// </summary>
        /// <returns>契約範囲内の乱数。</returns>
        float NextFloat01();
    }
}
