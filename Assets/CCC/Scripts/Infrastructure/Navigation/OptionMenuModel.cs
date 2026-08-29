using System;
using Ccc.Presentation.Navigation;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// OptionMenuの設定値を管理します。
    /// </summary>
    public sealed class OptionMenuModel
    {
        private int _seVolumeLevel =
            OptionVolumeLevels.Min;

        private int _bgmVolumeLevel =
            OptionVolumeLevels.Min;

        /// <summary>
        /// 効果音の音量レベルを1段階上げます。
        /// 最大値の場合は最大値を維持します。
        /// </summary>
        public void IncreaseSeVolumeLevel()
        {
            _seVolumeLevel = Math.Min(
                _seVolumeLevel + 1,
                OptionVolumeLevels.Max);
        }

        /// <summary>
        /// BGMの音量レベルを1段階上げます。
        /// 最大値の場合は最大値を維持します。
        /// </summary>
        public void IncreaseBgmVolumeLevel()
        {
            _bgmVolumeLevel = Math.Min(
                _bgmVolumeLevel + 1,
                OptionVolumeLevels.Max);
        }

        /// <summary>
        /// 現在のOption設定をSnapshotとして作成します。
        /// </summary>
        /// <returns>現在のSE・BGM音量を保持するSnapshotです。</returns>
        public OptionSettingsSnapshot CreateSnapshot()
        {
            return new OptionSettingsSnapshot(
                _seVolumeLevel,
                _bgmVolumeLevel);
        }
    }
}