using System;
using System.Collections.Generic;
using Ccc.Presentation.Navigation;
using UnityEngine;

namespace Ccc.Infrastructure.Audio
{
    /// <summary>
    ///     音量レベルごとの音量倍率をSE / BGM別に保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Audio Volume Level Table")]
    public sealed class AudioVolumeLevelTableAsset : ScriptableObject
    {
        [Tooltip("要素番号が音量レベルに対応します。値は0.0〜1.0です。")]
        [SerializeField] private float[] seVolumeRates = Array.Empty<float>();

        [Tooltip("要素番号が音量レベルに対応します。値は0.0〜1.0です。")]
        [SerializeField] private float[] bgmVolumeRates = Array.Empty<float>();

        private void OnValidate()
        {
            IReadOnlyList<string> errors = Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                Debug.LogError($"[{nameof(AudioVolumeLevelTableAsset)}] {name}: {errors[index]}");
            }
        }

        /// <summary>
        ///     効果音の音量倍率を取得する。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        /// <param name="volumeRate">取得できた音量倍率。</param>
        /// <returns>取得できた場合はtrue。</returns>
        public bool TryGetSeVolumeRate(int level, out float volumeRate)
        {
            return TryGetVolumeRate(seVolumeRates, level, out volumeRate);
        }

        /// <summary>
        ///     BGMの音量倍率を取得する。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        /// <param name="volumeRate">取得できた音量倍率。</param>
        /// <returns>取得できた場合はtrue。</returns>
        public bool TryGetBgmVolumeRate(int level, out float volumeRate)
        {
            return TryGetVolumeRate(bgmVolumeRates, level, out volumeRate);
        }

        /// <summary>
        ///     対応表の不正状態を一覧で返す。
        /// </summary>
        /// <returns>検証エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            ValidateVolumeRates(seVolumeRates, "SE", errors);
            ValidateVolumeRates(bgmVolumeRates, "BGM", errors);
            return errors;
        }

        private static bool TryGetVolumeRate(float[] volumeRates, int level, out float volumeRate)
        {
            volumeRate = 0f;
            if (volumeRates == null ||
                volumeRates.Length != OptionVolumeLevels.Count ||
                level < OptionVolumeLevels.Min ||
                level > OptionVolumeLevels.Max)
            {
                return false;
            }

            float candidateRate = volumeRates[level - OptionVolumeLevels.Min];
            if (!IsValidVolumeRate(candidateRate))
            {
                return false;
            }

            volumeRate = candidateRate;
            return true;
        }

        private static void ValidateVolumeRates(
            float[] volumeRates,
            string categoryName,
            ICollection<string> errors)
        {
            if (volumeRates == null || volumeRates.Length != OptionVolumeLevels.Count)
            {
                errors.Add(
                    $"{categoryName} volume rates must have {OptionVolumeLevels.Count} elements.");
                return;
            }

            for (int index = 0; index < volumeRates.Length; index++)
            {
                if (!IsValidVolumeRate(volumeRates[index]))
                {
                    errors.Add(
                        $"{categoryName} volume rate at level {index + OptionVolumeLevels.Min} is invalid: {volumeRates[index]}.");
                }
            }
        }

        private static bool IsValidVolumeRate(float volumeRate)
        {
            return !float.IsNaN(volumeRate) &&
                   !float.IsInfinity(volumeRate) &&
                   volumeRate >= 0f &&
                   volumeRate <= 1f;
        }
    }
}