namespace Ccc.Presentation
{
    /// <summary>
    /// 起動時に確定したプレイ環境のプラットフォームを提供する契約です。
    /// </summary>
    public interface IPlayPlatformMode
    {
        /// <summary>
        /// プレイ環境のプラットフォームを取得します。
        /// </summary>
        PlayPlatform Platform { get; }
    }
}
