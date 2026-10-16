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
        private readonly ReactiveProperty<int> _seVolumeLevel;
        private readonly ReactiveProperty<int> _bgmVolumeLevel;

        private bool _isDisposed;

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

            OptionSettingsSnapshot savedSettings = _optionSettingsStore.Load();
            _seVolumeLevel = new ReactiveProperty<int>(savedSettings.SEVolumeLevel);
            _bgmVolumeLevel = new ReactiveProperty<int>(savedSettings.BgmVolumeLevel);

            SEVolumeLevel = _seVolumeLevel.ToReadOnlyReactiveProperty();
            BgmVolumeLevel = _bgmVolumeLevel.ToReadOnlyReactiveProperty();
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

            _seVolumeLevel.Value = GetNextVolumeLevel(_seVolumeLevel.Value);
            _optionSettingsStore.Save(CreateSnapshot());
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

            _bgmVolumeLevel.Value = GetNextVolumeLevel(_bgmVolumeLevel.Value);
            _optionSettingsStore.Save(CreateSnapshot());
        }

        /// <summary>
        /// 現在のOption設定をSnapshotとして作成します。
        /// </summary>
        /// <returns>現在のSE・BGM音量を保持するSnapshotです。</returns>
        public OptionSettingsSnapshot CreateSnapshot()
        {
            return new OptionSettingsSnapshot(
                _seVolumeLevel.Value,
                _bgmVolumeLevel.Value);
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

            SEVolumeLevel.Dispose();
            BgmVolumeLevel.Dispose();

            _seVolumeLevel.Dispose();
            _bgmVolumeLevel.Dispose();
        }

        private static int GetNextVolumeLevel(int currentLevel)
        {
            return (currentLevel - OptionVolumeLevels.Min + 1) % OptionVolumeLevels.Count
                   + OptionVolumeLevels.Min;
        }
    }
}