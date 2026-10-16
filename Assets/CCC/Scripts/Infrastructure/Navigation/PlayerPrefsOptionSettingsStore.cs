using System;
using Ccc.Presentation.Navigation;
using UnityEngine;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// PlayerPrefsを使用してOption設定を保存します。
    /// </summary>
    public sealed class PlayerPrefsOptionSettingsStore : IOptionSettingsStore
    {
        private const string SEVolumeLevelKey = "Option.SEVolumeLevel";
        private const string BgmVolumeLevelKey = "Option.BgmVolumeLevel";

        /// <summary>
        /// 保存されているOption設定を読み込みます。保存値がない場合は既定値を返します。
        /// </summary>
        /// <returns>読み込んだOption設定。</returns>
        public OptionSettingsSnapshot Load()
        {
            return new OptionSettingsSnapshot(
                LoadVolumeLevel(SEVolumeLevelKey),
                LoadVolumeLevel(BgmVolumeLevelKey));
        }

        /// <summary>
        /// Option設定を保存します。
        /// </summary>
        /// <param name="snapshot">保存するOption設定。</param>
        /// <exception cref="ArgumentNullException">snapshotがnullの場合。</exception>
        public void Save(OptionSettingsSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            PlayerPrefs.SetInt(SEVolumeLevelKey, snapshot.SEVolumeLevel);
            PlayerPrefs.SetInt(BgmVolumeLevelKey, snapshot.BgmVolumeLevel);
            PlayerPrefs.Save();
        }

        private static int LoadVolumeLevel(string key)
        {
            int volumeLevel = PlayerPrefs.GetInt(key, OptionVolumeLevels.Default);
            if (volumeLevel < OptionVolumeLevels.Min ||
                volumeLevel > OptionVolumeLevels.Max)
            {
                Debug.LogWarning(
                    $"[{nameof(PlayerPrefsOptionSettingsStore)}] {key}の保存値{volumeLevel}が範囲外のため、" +
                    $"既定値{OptionVolumeLevels.Default}を使用します。");
                return OptionVolumeLevels.Default;
            }

            return volumeLevel;
        }
    }
}