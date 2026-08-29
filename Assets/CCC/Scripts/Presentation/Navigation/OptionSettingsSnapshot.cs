using System;

namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// Gameplay開始時点のOptionMenu設定を保持する不変Snapshotです。
    /// </summary>
    public sealed class OptionSettingsSnapshot
    {
        /// <summary>
        /// 効果音の音量レベルを取得します。
        /// </summary>
        public int SeVolumeLevel { get; }

        /// <summary>
        /// BGMの音量レベルを取得します。
        /// </summary>
        public int BgmVolumeLevel { get; }

        /// <summary>
        /// OptionSettingsSnapshotを生成します。
        /// </summary>
        /// <param name="seVolumeLevel">効果音の音量レベルです。</param>
        /// <param name="bgmVolumeLevel">BGMの音量レベルです。</param>
        public OptionSettingsSnapshot(
            int seVolumeLevel,
            int bgmVolumeLevel)
        {
            ValidateVolumeLevel(
                seVolumeLevel,
                nameof(seVolumeLevel));

            ValidateVolumeLevel(
                bgmVolumeLevel,
                nameof(bgmVolumeLevel));

            SeVolumeLevel = seVolumeLevel;
            BgmVolumeLevel = bgmVolumeLevel;
        }

        private static void ValidateVolumeLevel(
            int volumeLevel,
            string parameterName)
        {
            if (volumeLevel < OptionVolumeLevels.Min ||
                volumeLevel > OptionVolumeLevels.Max)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    volumeLevel,
                    "音量レベルは0から5の範囲で指定してください。");
            }
        }
    }
}