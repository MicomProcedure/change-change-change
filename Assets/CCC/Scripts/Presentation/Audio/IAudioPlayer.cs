namespace Ccc.Presentation.Audio
{
    /// <summary>
    ///     オーディオ再生要求の公開API。
    /// </summary>
    public interface IAudioPlayer
    {
        /// <summary>
        ///     効果音を再生する。
        /// </summary>
        /// <param name="key">再生対象の効果音キー。</param>
        void PlaySE(AudioKey key);

        /// <summary>
        ///     BGMを再生する。
        /// </summary>
        /// <param name="key">再生対象のBGMキー。</param>
        void PlayBgm(AudioKey key);

        /// <summary>
        ///     現在再生中のBGMを停止する。
        /// </summary>
        void StopBgm();
    }
}
