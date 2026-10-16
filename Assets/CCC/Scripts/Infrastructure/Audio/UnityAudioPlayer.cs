using System;
using System.Collections.Generic;
using Ccc.Presentation.Audio;
using UnityEngine;
using UnityEngine.Serialization;
using Ccc.Presentation.Navigation;

namespace Ccc.Infrastructure.Audio
{
    /// <summary>
    /// AudioCatalogAssetの定義を用途別のAudioSourceで再生するUnity実装です。
    /// </summary>
    public sealed class UnityAudioPlayer : MonoBehaviour, IAudioPlayer, IAudioVolumeControl
    {
        [SerializeField] private AudioCatalogAsset catalog;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        [FormerlySerializedAs("loopingSeSource")]
        [SerializeField] private AudioSource loopingSESource;
        [SerializeField] private AudioVolumeLevelTableAsset volumeLevelTable;

        private readonly HashSet<string> _warnedIssues = new HashSet<string>();
        private AudioKey _currentBgmKey;
        private AudioKey _currentLoopingSEKey;
        private float _seVolumeRate = 1f;
        private float _bgmVolumeRate = 1f;

        /// <summary>
        /// Inspector参照を検証し、Audio Catalogの不正をログへ出力します。
        /// </summary>
        private void Awake()
        {
            ValidateVolumeLevelTable();
            SetSEVolumeLevel(OptionVolumeLevels.Default);
            SetBgmVolumeLevel(OptionVolumeLevels.Default);
            var configurationErrors = new List<string>();
            if (catalog == null)
            {
                configurationErrors.Add("UnityAudioPlayer requires an AudioCatalogAsset.");
            }

            if (bgmSource == null)
            {
                configurationErrors.Add("UnityAudioPlayer requires a BGM AudioSource.");
            }

            if (seSource == null)
            {
                configurationErrors.Add("UnityAudioPlayer requires an SE AudioSource.");
            }

            if (loopingSESource == null)
            {
                configurationErrors.Add(
                    "UnityAudioPlayer requires a looping SE AudioSource.");
            }

            if (bgmSource != null && ReferenceEquals(bgmSource, seSource))
            {
                configurationErrors.Add(
                    "UnityAudioPlayer requires different AudioSource instances for BGM and SE.");
            }

            if (bgmSource != null && ReferenceEquals(bgmSource, loopingSESource))
            {
                configurationErrors.Add(
                    "UnityAudioPlayer requires different AudioSource instances for BGM and looping SE.");
            }

            if (seSource != null && ReferenceEquals(seSource, loopingSESource))
            {
                configurationErrors.Add(
                    "UnityAudioPlayer requires different AudioSource instances for SE and looping SE.");
            }

            for (int index = 0; index < configurationErrors.Count; index++)
            {
                LogErrorOnce(
                    $"configuration:{configurationErrors[index]}",
                    configurationErrors[index]);
            }

            if (configurationErrors.Count > 0)
            {
                return;
            }

            IReadOnlyList<string> errors = catalog.Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                LogErrorOnce($"catalog:{errors[index]}", errors[index]);
            }
        }

        /// <summary>
        /// 指定されたSEをCatalogから検索して再生します。
        /// </summary>
        /// <param name="key">再生するSEのキー。</param>
        public void PlaySE(AudioKey key)
        {
            if (catalog == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioCatalogAsset.",
                    "AudioCatalogAsset is not configured. SE playback was ignored.");
                return;
            }

            if (seSource == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an SE AudioSource.",
                    "SE AudioSource is not configured. SE playback was ignored.");
                return;
            }

            if (!catalog.TryGetSE(key, out AudioEntry entry))
            {
                WarnMissingKey(key, "SE");
                return;
            }

            if (entry.Clip == null)
            {
                LogWarningOnce(
                    $"catalog:{key} has no AudioClip.",
                    $"SE key {key} has no AudioClip. Playback was ignored.");
                return;
            }

            if (!IsValidVolume(entry.Volume))
            {
                LogWarningOnce(
                    $"catalog:{key} has an invalid volume: {entry.Volume}.",
                    $"SE key {key} has an invalid volume: {entry.Volume}. Playback was ignored.");
                return;
            }

            seSource.PlayOneShot(entry.Clip, entry.Volume * _seVolumeRate);
        }

        /// <summary>
        /// 指定されたSEを専用Sourceでループ再生します。同じSEが再生中なら再起動しません。
        /// </summary>
        /// <param name="key">ループ再生するSEのキー。</param>
        public void PlayLoopingSE(AudioKey key)
        {
            if (catalog == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioCatalogAsset.",
                    "AudioCatalogAsset is not configured. Looping SE playback was ignored.");
                return;
            }

            if (loopingSESource == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires a looping SE AudioSource.",
                    "Looping SE AudioSource is not configured. Looping SE playback was ignored.");
                return;
            }

            if (!catalog.TryGetSE(key, out AudioEntry entry))
            {
                WarnMissingKey(key, "Looping SE");
                return;
            }

            if (entry.Clip == null)
            {
                LogWarningOnce(
                    $"catalog:{key} has no AudioClip.",
                    $"Looping SE key {key} has no AudioClip. Playback was ignored.");
                return;
            }

            if (!IsValidVolume(entry.Volume))
            {
                LogWarningOnce(
                    $"catalog:{key} has an invalid volume: {entry.Volume}.",
                    $"Looping SE key {key} has an invalid volume: {entry.Volume}. Playback was ignored.");
                return;
            }

            if (_currentLoopingSEKey == key && loopingSESource.isPlaying)
            {
                return;
            }

            loopingSESource.Stop();
            loopingSESource.clip = entry.Clip;
            loopingSESource.volume = entry.Volume * _seVolumeRate;
            loopingSESource.loop = true;
            loopingSESource.Play();
            _currentLoopingSEKey = key;
        }

        /// <summary>
        /// 指定されたループSEを停止します。
        /// </summary>
        /// <param name="key">停止対象のループSEのキー。</param>
        public void StopLoopingSE(AudioKey key)
        {
            if (_currentLoopingSEKey != key)
            {
                return;
            }

            _currentLoopingSEKey = AudioKey.None;
            if (loopingSESource == null)
            {
                return;
            }

            loopingSESource.Stop();
        }

        /// <summary>
        /// 指定されたBGMへ切り替えます。同じBGMが再生中なら再起動しません。
        /// </summary>
        /// <param name="key">再生するBGMのキー。</param>
        public void PlayBgm(AudioKey key)
        {
            if (catalog == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioCatalogAsset.",
                    "AudioCatalogAsset is not configured. BGM playback was ignored.");
                return;
            }

            if (bgmSource == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires a BGM AudioSource.",
                    "BGM AudioSource is not configured. BGM playback was ignored.");
                return;
            }

            if (!catalog.TryGetBgm(key, out AudioEntry entry))
            {
                WarnMissingKey(key, "BGM");
                return;
            }

            if (entry.Clip == null)
            {
                LogWarningOnce(
                    $"catalog:{key} has no AudioClip.",
                    $"BGM key {key} has no AudioClip. Playback was ignored.");
                return;
            }

            if (!IsValidVolume(entry.Volume))
            {
                LogWarningOnce(
                    $"catalog:{key} has an invalid volume: {entry.Volume}.",
                    $"BGM key {key} has an invalid volume: {entry.Volume}. Playback was ignored.");
                return;
            }

            if (_currentBgmKey == key && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.Stop();
            bgmSource.clip = entry.Clip;
            bgmSource.volume = entry.Volume * _bgmVolumeRate;
            bgmSource.loop = true;
            bgmSource.Play();
            _currentBgmKey = key;
        }

        /// <summary>
        /// 再生中のBGMを停止します。
        /// </summary>
        public void StopBgm()
        {
            _currentBgmKey = AudioKey.None;
            if (bgmSource == null)
            {
                return;
            }

            bgmSource.Stop();
        }
        
        /// <summary>
        /// 効果音の音量レベルを設定し、再生中のループSEにも反映します。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        public void SetSEVolumeLevel(int level)
        {
            if (volumeLevelTable == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioVolumeLevelTableAsset.",
                    "AudioVolumeLevelTableAsset is not configured. SE volume level was ignored.");
                return;
            }

            if (!volumeLevelTable.TryGetSEVolumeRate(level, out float volumeRate))
            {
                LogWarningOnce(
                    $"volume-level:SE:{level}",
                    $"SE volume level {level} is not configured or is invalid. Volume was not changed.");
                return;
            }

            _seVolumeRate = volumeRate;

            if (loopingSESource != null &&
                catalog != null &&
                _currentLoopingSEKey != AudioKey.None &&
                catalog.TryGetSE(_currentLoopingSEKey, out AudioEntry entry))
            {
                loopingSESource.volume = entry.Volume * _seVolumeRate;
            }
        }

        /// <summary>
        /// BGMの音量レベルを設定し、再生中のBGMにも反映します。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        public void SetBgmVolumeLevel(int level)
        {
            if (volumeLevelTable == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioVolumeLevelTableAsset.",
                    "AudioVolumeLevelTableAsset is not configured. BGM volume level was ignored.");
                return;
            }

            if (!volumeLevelTable.TryGetBgmVolumeRate(level, out float volumeRate))
            {
                LogWarningOnce(
                    $"volume-level:BGM:{level}",
                    $"BGM volume level {level} is not configured or is invalid. Volume was not changed.");
                return;
            }

            _bgmVolumeRate = volumeRate;

            if (bgmSource != null &&
                catalog != null &&
                _currentBgmKey != AudioKey.None &&
                catalog.TryGetBgm(_currentBgmKey, out AudioEntry entry))
            {
                bgmSource.volume = entry.Volume * _bgmVolumeRate;
            }
        }

        private void ValidateVolumeLevelTable()
        {
            if (volumeLevelTable == null)
            {
                LogErrorOnce(
                    "configuration:UnityAudioPlayer requires an AudioVolumeLevelTableAsset.",
                    "UnityAudioPlayer requires an AudioVolumeLevelTableAsset.");
                return;
            }

            IReadOnlyList<string> errors = volumeLevelTable.Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                LogErrorOnce($"volume-table:{errors[index]}", errors[index]);
            }
        }

        private void WarnMissingKey(AudioKey key, string category)
        {
            LogWarningOnce(
                $"missing-key:{category}:{key}",
                $"{category} key is not configured or is invalid: {key}.");
        }

        private void LogErrorOnce(string issueId, string message)
        {
            if (_warnedIssues.Add(issueId))
            {
                Debug.LogError($"[UnityAudioPlayer] {name}: {message}");
            }
        }

        private void LogWarningOnce(string issueId, string message)
        {
            if (_warnedIssues.Add(issueId))
            {
                Debug.LogWarning($"[UnityAudioPlayer] {name}: {message}");
            }
        }

        private static bool IsValidVolume(float volume)
        {
            return !float.IsNaN(volume) &&
                   !float.IsInfinity(volume) &&
                   volume >= 0f &&
                   volume <= 1f;
        }
    }
}
