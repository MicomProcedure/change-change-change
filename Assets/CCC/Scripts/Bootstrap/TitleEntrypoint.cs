using System;
using Ccc.Infrastructure.Navigation;
using Ccc.Presentation.Audio;
using VContainer.Unity;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// TitleScene開始時の初期処理を担当します。
    /// </summary>
    public sealed class TitleEntryPoint : IStartable
    {
        private readonly OptionVolumeAudioSynchronizer _optionVolumeAudioSynchronizer;
        private readonly TitleScreenTransitionSoundPlayer _titleScreenTransitionSoundPlayer;
        private readonly TitleSelectorChangeSoundPlayer _titleSelectorChangeSoundPlayer;
        private readonly IAudioPlayer _audio;

        /// <summary>
        /// TitleEntryPointを生成します。
        /// </summary>
        /// <param name="optionVolumeAudioSynchronizer">Option音量のAudio反映処理。</param>
        /// <param name="titleScreenTransitionSoundPlayer">Title画面遷移のSE再生処理。</param>
        /// <param name="titleSelectorChangeSoundPlayer">Title画面の選択値切り替えのSE再生処理。</param>
        /// <param name="audio">Audio再生ポート。</param>
        public TitleEntryPoint(
            OptionVolumeAudioSynchronizer optionVolumeAudioSynchronizer,
            TitleScreenTransitionSoundPlayer titleScreenTransitionSoundPlayer,
            TitleSelectorChangeSoundPlayer titleSelectorChangeSoundPlayer,
            IAudioPlayer audio)
        {
            _optionVolumeAudioSynchronizer = optionVolumeAudioSynchronizer ??
                throw new ArgumentNullException(nameof(optionVolumeAudioSynchronizer));
            _titleScreenTransitionSoundPlayer = titleScreenTransitionSoundPlayer ??
                throw new ArgumentNullException(nameof(titleScreenTransitionSoundPlayer));
            _titleSelectorChangeSoundPlayer = titleSelectorChangeSoundPlayer ??
                throw new ArgumentNullException(nameof(titleSelectorChangeSoundPlayer));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        /// <summary>
        /// 音量反映とTitle画面SEの購読を開始し、Title BGMを再生します。
        /// </summary>
        public void Start()
        {
            // SE音量変更時のchange音を変更後の音量で鳴らすため、音量反映より後に購読する
            _optionVolumeAudioSynchronizer.StartSubscription();
            _titleScreenTransitionSoundPlayer.StartSubscription();
            _titleSelectorChangeSoundPlayer.StartSubscription();
            _audio.PlayBgm(AudioKey.BgmTitle);
        }
    }
}
