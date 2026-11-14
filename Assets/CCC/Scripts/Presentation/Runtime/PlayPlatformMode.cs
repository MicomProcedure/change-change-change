using System;

namespace Ccc.Presentation
{
    /// <summary>
    /// プレイ環境のプラットフォームを保持する不変実装です。
    /// </summary>
    public sealed class PlayPlatformMode : IPlayPlatformMode
    {
        /// <summary>
        /// PlayPlatformModeを生成します。
        /// </summary>
        /// <param name="platform">プレイ環境のプラットフォーム。</param>
        /// <exception cref="ArgumentOutOfRangeException">未定義のプラットフォームの場合。</exception>
        public PlayPlatformMode(PlayPlatform platform)
        {
            if (!Enum.IsDefined(typeof(PlayPlatform), platform))
            {
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }

            Platform = platform;
        }

        /// <summary>
        /// プレイ環境のプラットフォームを取得します。
        /// </summary>
        public PlayPlatform Platform { get; }
    }
}
