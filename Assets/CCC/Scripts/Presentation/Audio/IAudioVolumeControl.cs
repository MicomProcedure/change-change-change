namespace Ccc.Presentation.Audio
{
    /// <summary>
    ///     音量レベルを変更する公開API
    /// </summary>
    public interface IAudioVolumeControl
    {
        /// <summary>
        ///     効果音の音量レベルを設定する。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        void SetSEVolumeLevel(int level);

        /// <summary>
        ///     BGMの音量レベルを設定する。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        void SetBgmVolumeLevel(int level);
    }
}