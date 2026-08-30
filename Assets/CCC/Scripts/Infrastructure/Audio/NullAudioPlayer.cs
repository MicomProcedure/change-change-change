using Ccc.Presentation.Audio;

namespace Ccc.Infrastructure.Audio
{
    /// <summary>
    /// Audio設定がない起動環境で使用するNo-op Audio実装です。
    /// </summary>
    public sealed class NullAudioPlayer : IAudioPlayer
    {
        /// <summary>
        /// 効果音の再生要求を無視します。
        /// </summary>
        /// <param name="key">再生対象の効果音キー。</param>
        public void PlaySE(AudioKey key)
        {
        }

        /// <summary>
        /// ループ効果音の再生要求を無視します。
        /// </summary>
        /// <param name="key">再生対象の効果音キー。</param>
        public void PlayLoopingSE(AudioKey key)
        {
        }

        /// <summary>
        /// ループ効果音の停止要求を無視します。
        /// </summary>
        /// <param name="key">停止対象の効果音キー。</param>
        public void StopLoopingSE(AudioKey key)
        {
        }

        /// <summary>
        /// BGMの再生要求を無視します。
        /// </summary>
        /// <param name="key">再生対象のBGMキー。</param>
        public void PlayBgm(AudioKey key)
        {
        }

        /// <summary>
        /// BGM停止要求を無視します。
        /// </summary>
        public void StopBgm()
        {
        }
    }
}
