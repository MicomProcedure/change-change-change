using System;
using System.Globalization;
using Ccc.Presentation.UI;

namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// OptionMenuの描画に必要な変換済み表示状態です。
    /// </summary>
    public sealed class OptionMenuViewStateDto
    {
        /// <summary>
        /// 効果音の音量セレクターの表示状態を取得します。
        /// </summary>
        public CycleSelectorStateDto SEVolumeSelector { get; }

        /// <summary>
        /// BGMの音量セレクターの表示状態を取得します。
        /// </summary>
        public CycleSelectorStateDto BgmVolumeSelector { get; }

        private OptionMenuViewStateDto(
            CycleSelectorStateDto seVolumeSelector,
            CycleSelectorStateDto bgmVolumeSelector)
        {
            SEVolumeSelector = seVolumeSelector;
            BgmVolumeSelector = bgmVolumeSelector;
        }

        /// <summary>
        /// Option設定から表示状態を生成します。
        /// </summary>
        /// <param name="settings">現在のOption設定。</param>
        /// <param name="isInteractable">入力を受け付けるかどうか。</param>
        /// <returns>OptionMenuの表示状態。</returns>
        /// <exception cref="ArgumentNullException">settingsがnullの場合。</exception>
        public static OptionMenuViewStateDto Create(OptionSettingsSnapshot settings, bool isInteractable)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return new OptionMenuViewStateDto(
                CreateVolumeSelector(settings.SEVolumeLevel, isInteractable),
                CreateVolumeSelector(settings.BgmVolumeLevel, isInteractable));
        }

        private static CycleSelectorStateDto CreateVolumeSelector(int volumeLevel, bool isInteractable)
        {
            // 音量は最小レベル（ミュート）を丸に含めず、最小レベルの時はどの丸も塗りつぶさない
            return new CycleSelectorStateDto(
                volumeLevel.ToString(CultureInfo.InvariantCulture),
                isInteractable,
                volumeLevel - OptionVolumeLevels.Min - 1,
                OptionVolumeLevels.Count - 1);
        }
    }
}
