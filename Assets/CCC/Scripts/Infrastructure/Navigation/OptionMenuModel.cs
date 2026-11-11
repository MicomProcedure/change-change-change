using System;
using Ccc.Presentation.Navigation;
using R3;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// OptionMenuの設定値を管理します。
    /// </summary>
    public sealed class OptionMenuModel : IDisposable
    {
        private readonly IOptionSettingsStore _optionSettingsStore;
        private readonly ReactiveProperty<OptionSettingsSnapshot> _settings;

        private bool _isDisposed;

        /// <summary>
        /// 現在のOption設定を取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<OptionSettingsSnapshot> Settings { get; }

        /// <summary>
        /// 効果音の音量レベルを取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<int> SEVolumeLevel { get; }

        /// <summary>
        /// BGMの音量レベルを取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<int> BgmVolumeLevel { get; }

        /// <summary>
        /// OptionMenuModelを生成し、保存済みの設定を読み込みます。
        /// </summary>
        /// <param name="optionSettingsStore">Option設定の保存先。</param>
        /// <exception cref="ArgumentNullException">optionSettingsStoreがnullの場合。</exception>
        public OptionMenuModel(IOptionSettingsStore optionSettingsStore)
        {
            _optionSettingsStore =
                optionSettingsStore ?? throw new ArgumentNullException(nameof(optionSettingsStore));

            _settings = new ReactiveProperty<OptionSettingsSnapshot>(_optionSettingsStore.Load());

            Settings = _settings.ToReadOnlyReactiveProperty();
            SEVolumeLevel = _settings
                .Select(settings => settings.SEVolumeLevel)
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty();
            BgmVolumeLevel = _settings
                .Select(settings => settings.BgmVolumeLevel)
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty();
        }

        /// <summary>
        /// 効果音の音量レベルを次の段階へ切り替えて保存します。最大値の次は最小値に戻ります。
        /// </summary>
        public void CycleSEVolumeLevel()
        {
            if (_isDisposed)
            {
                return;
            }

            OptionSettingsSnapshot currentSettings = _settings.Value;
            UpdateSettings(new OptionSettingsSnapshot(
                GetNextVolumeLevel(currentSettings.SEVolumeLevel),
                currentSettings.BgmVolumeLevel));
        }

        /// <summary>
        /// BGMの音量レベルを次の段階へ切り替えて保存します。最大値の次は最小値に戻ります。
        /// </summary>
        public void CycleBgmVolumeLevel()
        {
            if (_isDisposed)
            {
                return;
            }

            OptionSettingsSnapshot currentSettings = _settings.Value;
            UpdateSettings(new OptionSettingsSnapshot(
                currentSettings.SEVolumeLevel,
                GetNextVolumeLevel(currentSettings.BgmVolumeLevel)));
        }

        /// <summary>
        /// 保持しているReactivePropertyを破棄します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            Settings.Dispose();
            SEVolumeLevel.Dispose();
            BgmVolumeLevel.Dispose();

            _settings.Dispose();
        }

        private void UpdateSettings(OptionSettingsSnapshot settings)
        {
            _settings.Value = settings;
            _optionSettingsStore.Save(settings);
        }

        private static int GetNextVolumeLevel(int currentLevel)
        {
            return (currentLevel - OptionVolumeLevels.Min + 1) % OptionVolumeLevels.Count
                   + OptionVolumeLevels.Min;
        }
    }
}