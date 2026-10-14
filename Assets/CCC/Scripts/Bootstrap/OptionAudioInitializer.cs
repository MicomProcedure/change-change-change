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
        private readonly IAudioVolumeSettings _audioVolumeSettings;

        /// <summary>
        /// OptionAudioInitializerを生成します。
        /// </summary>
        /// <param name="optionSettingsStore">Option設定の保存先。</param>
        /// <param name="audioVolumeSettings">音量レベル設定ポート。</param>
        public OptionAudioInitializer(
            IOptionSettingsStore optionSettingsStore,
            IAudioVolumeSettings audioVolumeSettings)
        {
            _optionSettingsStore =
                optionSettingsStore ?? throw new ArgumentNullException(nameof(optionSettingsStore));
            _audioVolumeSettings =
                audioVolumeSettings ?? throw new ArgumentNullException(nameof(audioVolumeSettings));
        }

        /// <summary>
        /// 保存済みの音量レベルを反映します。
        /// </summary>
        public void Start()
        {
            OptionSettingsSnapshot savedSettings = _optionSettingsStore.Load();
            _audioVolumeSettings.SetSeVolumeLevel(savedSettings.SeVolumeLevel);
            _audioVolumeSettings.SetBgmVolumeLevel(savedSettings.BgmVolumeLevel);
        }
    }
}