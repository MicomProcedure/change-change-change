namespace Ccc.Infrastructure.Audio
{
    /// <summary>
    /// 再生方法ごとに許可する音量の範囲を定義します。
    /// </summary>
    public static class AudioVolumeRules
    {
        /// <summary>
        /// 音量の下限です。
        /// </summary>
        public const float MinVolume = 0f;

        /// <summary>
        /// AudioSource.volumeで表現できる音量の上限です。BGMとループSEはこの値を超えて大きくなりません。
        /// </summary>
        public const float MaxAudioSourceVolume = 1f;

        /// <summary>
        /// AudioSource.PlayOneShotで再生する音量として有効かどうかを判定します。1を超える値も増幅できるため有効です。
        /// </summary>
        /// <param name="volume">判定する音量。</param>
        /// <returns>下限以上の有限値の場合はtrue。</returns>
        public static bool IsValidOneShotVolume(float volume)
        {
            return !float.IsNaN(volume) &&
                   !float.IsInfinity(volume) &&
                   volume >= MinVolume;
        }

        /// <summary>
        /// AudioSource.volumeに設定する音量として有効かどうかを判定します。
        /// </summary>
        /// <param name="volume">判定する音量。</param>
        /// <returns>下限以上かつ上限以下の有限値の場合はtrue。</returns>
        public static bool IsValidAudioSourceVolume(float volume)
        {
            return IsValidOneShotVolume(volume) &&
                   volume <= MaxAudioSourceVolume;
        }
    }
}
