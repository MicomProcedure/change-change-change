using System;
using Ccc.Presentation.Audio;
using Ccc.Presentation.Navigation;
using VContainer.Unity;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// アプリ起動時に保存済みの音量レベルをAudioへ反映します。
    /// </summary>
    public sealed class OptionAudioInitializer : IStartable
    {
        private readonly IOptionSettingsStore _optionSettingsStore;
        private readonly IAudioVolumeControl _audioVolumeControl;

        /// <summary>
        /// OptionAudioInitializerを生成します。
        /// </summary>
        /// <param name="optionSettingsStore">Option設定の保存先。</param>
        /// <param name="audioVolumeControl">音量レベル変更ポート。</param>
        public OptionAudioInitializer(
            IOptionSettingsStore optionSettingsStore,
            IAudioVolumeControl audioVolumeControl)
        {
            _optionSettingsStore =
                optionSettingsStore ?? throw new ArgumentNullException(nameof(optionSettingsStore));
            _audioVolumeControl =
                audioVolumeControl ?? throw new ArgumentNullException(nameof(audioVolumeControl));
        }

        /// <summary>
        /// 保存済みの音量レベルを反映します。
        /// </summary>
        public void Start()
        {
            OptionSettingsSnapshot savedSettings = _optionSettingsStore.Load();
            _audioVolumeControl.SetSeVolumeLevel(savedSettings.SeVolumeLevel);
            _audioVolumeControl.SetBgmVolumeLevel(savedSettings.BgmVolumeLevel);
        }
    }
}