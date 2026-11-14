namespace Ccc.Infrastructure.Platform
{
    /// <summary>
    /// Editor実行時にプレイ環境のプラットフォームを上書きする設定です。
    /// </summary>
    public enum PlayPlatformOverride
    {
        /// <summary>
        /// 上書きせず、Editor既定のプラットフォームを使用します。
        /// </summary>
        Auto = 0,

        /// <summary>
        /// モバイル向け構成として実行します。
        /// </summary>
        Mobile,

        /// <summary>
        /// PC向け構成として実行します。
        /// </summary>
        PC
    }
}
