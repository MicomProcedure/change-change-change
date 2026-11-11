using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Presentation.Audio;
using UnityEngine;

namespace Ccc.Infrastructure.Audio
{
    /// <summary>
    ///     効果音とBGMの割り当てを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Audio Catalog")]
    public sealed class AudioCatalogAsset : ScriptableObject
    {
        private enum AudioCategory
        {
            Unknown = 0,
            SE = 1,
            Bgm = 2
        }

        [SerializeField]
        private List<AudioEntry> _seEntries = new List<AudioEntry>();

        [SerializeField]
        private List<AudioEntry> _bgmEntries = new List<AudioEntry>();

        private void OnEnable()
        {
            LogValidationErrors();
        }

        private void OnValidate()
        {
            LogValidationErrors();
        }

        /// <summary>
        ///     効果音の定義を取得する。
        /// </summary>
        /// <param name="key">取得対象のキー。</param>
        /// <param name="entry">取得できた定義。</param>
        /// <returns>定義が存在する場合はtrue。</returns>
        public bool TryGetSE(AudioKey key, out AudioEntry entry)
        {
            if (key == AudioKey.None || _seEntries == null)
            {
                entry = null;
                return false;
            }

            entry = _seEntries.FirstOrDefault(candidate => candidate != null && candidate.Key == key);
            return entry != null;
        }

        /// <summary>
        ///     BGMの定義を取得する。
        /// </summary>
        /// <param name="key">取得対象のキー。</param>
        /// <param name="entry">取得できた定義。</param>
        /// <returns>定義が存在する場合はtrue。</returns>
        public bool TryGetBgm(AudioKey key, out AudioEntry entry)
        {
            if (key == AudioKey.None || _bgmEntries == null)
            {
                entry = null;
                return false;
            }

            entry = _bgmEntries.FirstOrDefault(candidate => candidate != null && candidate.Key == key);
            return entry != null;
        }

        /// <summary>
        ///     カタログの不正状態を一覧で返す。
        /// </summary>
        /// <returns>検証エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            var allEntries = new List<AudioEntry>();
            if (_seEntries != null)
            {
                allEntries.AddRange(_seEntries);
            }

            if (_bgmEntries != null)
            {
                allEntries.AddRange(_bgmEntries);
            }

            ValidateEntries(_seEntries, AudioCategory.SE, "SE", errors);
            ValidateEntries(_bgmEntries, AudioCategory.Bgm, "BGM", errors);

            foreach (var duplicateGroup in allEntries
                         .Where(entry => entry != null)
                         .GroupBy(entry => entry.Key)
                         .Where(group => group.Count() > 1))
            {
                errors.Add($"Key {duplicateGroup.Key} is duplicated {duplicateGroup.Count()} times in the catalog.");
            }

            foreach (AudioKey key in Enum.GetValues(typeof(AudioKey)))
            {
                if (key == AudioKey.None)
                {
                    continue;
                }

                AudioCategory expectedCategory = GetCategory(key);
                int registeredCount = allEntries.Count(entry => entry != null && entry.Key == key);
                if (registeredCount != 1)
                {
                    if (registeredCount == 0)
                    {
                        errors.Add($"{key} is not registered in any catalog list.");
                    }
                    else
                    {
                        errors.Add($"{key} must be registered exactly once in the catalog.");
                    }
                }

                if (expectedCategory == AudioCategory.SE)
                {
                    var seCount = _seEntries == null ? 0 : _seEntries.Count(entry => entry != null && entry.Key == key);
                    if (seCount != 1)
                    {
                        errors.Add($"{key} must be registered exactly once in the SE list.");
                    }
                }

                if (expectedCategory == AudioCategory.Bgm)
                {
                    var bgmCount = _bgmEntries == null ? 0 : _bgmEntries.Count(entry => entry != null && entry.Key == key);
                    if (bgmCount != 1)
                    {
                        errors.Add($"{key} must be registered exactly once in the BGM list.");
                    }
                }
            }

            return errors.Distinct().ToArray();
        }

        private static void ValidateEntries(
            IReadOnlyList<AudioEntry> entries,
            AudioCategory expectedCategory,
            string categoryName,
            ICollection<string> errors)
        {
            if (entries == null)
            {
                return;
            }

            for (int index = 0; index < entries.Count; index++)
            {
                AudioEntry entry = entries[index];
                if (entry == null)
                {
                    errors.Add($"{categoryName} list contains a null entry.");
                    continue;
                }

                if (entry.Key == AudioKey.None)
                {
                    errors.Add("AudioKey.None is not allowed in the catalog.");
                }

                if (GetCategory(entry.Key) != expectedCategory)
                {
                    errors.Add($"{entry.Key} should be registered in the {categoryName} list.");
                }

                if (entry.Clip == null)
                {
                    errors.Add($"{entry.Key} has no AudioClip.");
                }

                bool isVolumeValid = expectedCategory == AudioCategory.Bgm
                    ? AudioVolumeRules.IsValidAudioSourceVolume(entry.Volume)
                    : AudioVolumeRules.IsValidOneShotVolume(entry.Volume);
                if (!isVolumeValid)
                {
                    errors.Add($"{entry.Key} has an invalid volume: {entry.Volume}.");
                }
            }
        }

        private static AudioCategory GetCategory(AudioKey key)
        {
            switch (key)
            {
                case AudioKey.BgmTitle:
                case AudioKey.BgmGameplayNormal:
                case AudioKey.BgmGameplayDifficult:
                case AudioKey.BgmResult:
                    return AudioCategory.Bgm;
                case AudioKey.SEChangeClick:
                case AudioKey.SEMissionPresented:
                case AudioKey.SENormalClick:
                case AudioKey.SECorrect:
                case AudioKey.SEWrong:
                case AudioKey.SETimeClock:
                case AudioKey.SEGameOver:
                case AudioKey.SECancelClick:
                case AudioKey.SECountdown:
                case AudioKey.SEHizimere:
                case AudioKey.SESubmit:
                    return AudioCategory.SE;
                default:
                    return AudioCategory.Unknown;
            }
        }

        private void LogValidationErrors()
        {
            var errors = Validate();
            if (errors.Count == 0)
            {
                return;
            }

            foreach (var error in errors)
            {
                Debug.LogError($"[AudioCatalogAsset] {name}: {error}");
            }
        }
    }
}
