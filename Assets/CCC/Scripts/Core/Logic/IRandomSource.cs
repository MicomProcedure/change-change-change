#nullable enable

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 抽選処理へ一様乱数sampleを供給するインターフェース。
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>
        /// 0以上1未満の乱数sampleを1件取得します。
        /// </summary>
        /// <returns>0以上1未満のsample。</returns>
        double NextSample();
    }
}
