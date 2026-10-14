namespace Ccc.Presentation.Audio
{
    /// <summary>
    ///     音量レベル設定の公開API。
    /// </summary>
    public interface IAudioVolumeSettings
    {
        /// <summary>
        ///     効果音の音量レベルを設定する。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        void SetSeVolumeLevel(int level);

        /// <summary>
        ///     BGMの音量レベルを設定する。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        void SetBgmVolumeLevel(int level);
    }
}