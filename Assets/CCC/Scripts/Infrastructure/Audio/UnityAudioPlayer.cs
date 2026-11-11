using System;
using System.Collections.Generic;
using Ccc.Presentation.Audio;
using UnityEngine;
using Ccc.Presentation.Navigation;

namespace Ccc.Infrastructure.Audio
{
    /// <summary>
    /// AudioCatalogAssetの定義を用途別のAudioSourceで再生するUnity実装です。
    /// </summary>
    public sealed class UnityAudioPlayer : MonoBehaviour, IAudioPlayer, IAudioVolumeControl
    {
        [SerializeField] private AudioCatalogAsset _catalog;
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _seSource;
        [SerializeField] private AudioSource _loopingSESource;
        [SerializeField] private AudioVolumeLevelTableAsset _volumeLevelTable;

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
            if (_catalog == null)
            {
                configurationErrors.Add("UnityAudioPlayer requires an AudioCatalogAsset.");
            }

            if (_bgmSource == null)
            {
                configurationErrors.Add("UnityAudioPlayer requires a BGM AudioSource.");
            }

            if (_seSource == null)
            {
                configurationErrors.Add("UnityAudioPlayer requires an SE AudioSource.");
            }

            if (_loopingSESource == null)
            {
                configurationErrors.Add(
                    "UnityAudioPlayer requires a looping SE AudioSource.");
            }

            if (_bgmSource != null && ReferenceEquals(_bgmSource, _seSource))
            {
                configurationErrors.Add(
                    "UnityAudioPlayer requires different AudioSource instances for BGM and SE.");
            }

            if (_bgmSource != null && ReferenceEquals(_bgmSource, _loopingSESource))
            {
                configurationErrors.Add(
                    "UnityAudioPlayer requires different AudioSource instances for BGM and looping SE.");
            }

            if (_seSource != null && ReferenceEquals(_seSource, _loopingSESource))
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

            IReadOnlyList<string> errors = _catalog.Validate();
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
            if (_catalog == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioCatalogAsset.",
                    "AudioCatalogAsset is not configured. SE playback was ignored.");
                return;
            }

            if (_seSource == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an SE AudioSource.",
                    "SE AudioSource is not configured. SE playback was ignored.");
                return;
            }

            if (!_catalog.TryGetSE(key, out AudioEntry entry))
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

            if (!AudioVolumeRules.IsValidOneShotVolume(entry.Volume))
            {
                LogWarningOnce(
                    $"catalog:{key} has an invalid volume: {entry.Volume}.",
                    $"SE key {key} has an invalid volume: {entry.Volume}. Playback was ignored.");
                return;
            }

            _seSource.PlayOneShot(entry.Clip, entry.Volume * _seVolumeRate);
        }

        /// <summary>
        /// 指定されたSEを専用Sourceでループ再生します。同じSEが再生中なら再起動しません。
        /// </summary>
        /// <param name="key">ループ再生するSEのキー。</param>
        public void PlayLoopingSE(AudioKey key)
        {
            if (_catalog == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioCatalogAsset.",
                    "AudioCatalogAsset is not configured. Looping SE playback was ignored.");
                return;
            }

            if (_loopingSESource == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires a looping SE AudioSource.",
                    "Looping SE AudioSource is not configured. Looping SE playback was ignored.");
                return;
            }

            if (!_catalog.TryGetSE(key, out AudioEntry entry))
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

            if (!AudioVolumeRules.IsValidOneShotVolume(entry.Volume))
            {
                LogWarningOnce(
                    $"catalog:{key} has an invalid volume: {entry.Volume}.",
                    $"Looping SE key {key} has an invalid volume: {entry.Volume}. Playback was ignored.");
                return;
            }

            if (!AudioVolumeRules.IsValidAudioSourceVolume(entry.Volume))
            {
                LogWarningOnce(
                    $"catalog:{key} exceeds the AudioSource volume limit: {entry.Volume}.",
                    $"Looping SE key {key} has volume {entry.Volume}, but looping playback cannot exceed {AudioVolumeRules.MaxAudioSourceVolume}.");
            }

            if (_currentLoopingSEKey == key && _loopingSESource.isPlaying)
            {
                return;
            }

            _loopingSESource.Stop();
            _loopingSESource.clip = entry.Clip;
            _loopingSESource.volume = entry.Volume * _seVolumeRate;
            _loopingSESource.loop = true;
            _loopingSESource.Play();
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
            if (_loopingSESource == null)
            {
                return;
            }

            _loopingSESource.Stop();
        }

        /// <summary>
        /// 指定されたBGMへ切り替えます。同じBGMが再生中なら再起動しません。
        /// </summary>
        /// <param name="key">再生するBGMのキー。</param>
        public void PlayBgm(AudioKey key)
        {
            if (_catalog == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioCatalogAsset.",
                    "AudioCatalogAsset is not configured. BGM playback was ignored.");
                return;
            }

            if (_bgmSource == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires a BGM AudioSource.",
                    "BGM AudioSource is not configured. BGM playback was ignored.");
                return;
            }

            if (!_catalog.TryGetBgm(key, out AudioEntry entry))
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

            if (!AudioVolumeRules.IsValidAudioSourceVolume(entry.Volume))
            {
                LogWarningOnce(
                    $"catalog:{key} has an invalid volume: {entry.Volume}.",
                    $"BGM key {key} has an invalid volume: {entry.Volume}. Playback was ignored.");
                return;
            }

            if (_currentBgmKey == key && _bgmSource.isPlaying)
            {
                return;
            }

            _bgmSource.Stop();
            _bgmSource.clip = entry.Clip;
            _bgmSource.volume = entry.Volume * _bgmVolumeRate;
            _bgmSource.loop = true;
            _bgmSource.Play();
            _currentBgmKey = key;
        }

        /// <summary>
        /// 再生中のBGMを停止します。
        /// </summary>
        public void StopBgm()
        {
            _currentBgmKey = AudioKey.None;
            if (_bgmSource == null)
            {
                return;
            }

            _bgmSource.Stop();
        }
        
        /// <summary>
        /// 効果音の音量レベルを設定し、再生中のループSEにも反映します。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        public void SetSEVolumeLevel(int level)
        {
            if (_volumeLevelTable == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioVolumeLevelTableAsset.",
                    "AudioVolumeLevelTableAsset is not configured. SE volume level was ignored.");
                return;
            }

            if (!_volumeLevelTable.TryGetSEVolumeRate(level, out float volumeRate))
            {
                LogWarningOnce(
                    $"volume-level:SE:{level}",
                    $"SE volume level {level} is not configured or is invalid. Volume was not changed.");
                return;
            }

            _seVolumeRate = volumeRate;

            if (_loopingSESource != null &&
                _catalog != null &&
                _currentLoopingSEKey != AudioKey.None &&
                _catalog.TryGetSE(_currentLoopingSEKey, out AudioEntry entry))
            {
                _loopingSESource.volume = entry.Volume * _seVolumeRate;
            }
        }

        /// <summary>
        /// BGMの音量レベルを設定し、再生中のBGMにも反映します。
        /// </summary>
        /// <param name="level">音量レベル。</param>
        public void SetBgmVolumeLevel(int level)
        {
            if (_volumeLevelTable == null)
            {
                LogWarningOnce(
                    "configuration:UnityAudioPlayer requires an AudioVolumeLevelTableAsset.",
                    "AudioVolumeLevelTableAsset is not configured. BGM volume level was ignored.");
                return;
            }

            if (!_volumeLevelTable.TryGetBgmVolumeRate(level, out float volumeRate))
            {
                LogWarningOnce(
                    $"volume-level:BGM:{level}",
                    $"BGM volume level {level} is not configured or is invalid. Volume was not changed.");
                return;
            }

            _bgmVolumeRate = volumeRate;

            if (_bgmSource != null &&
                _catalog != null &&
                _currentBgmKey != AudioKey.None &&
                _catalog.TryGetBgm(_currentBgmKey, out AudioEntry entry))
            {
                _bgmSource.volume = entry.Volume * _bgmVolumeRate;
            }
        }

        private void ValidateVolumeLevelTable()
        {
            if (_volumeLevelTable == null)
            {
                LogErrorOnce(
                    "configuration:UnityAudioPlayer requires an AudioVolumeLevelTableAsset.",
                    "UnityAudioPlayer requires an AudioVolumeLevelTableAsset.");
                return;
            }

            IReadOnlyList<string> errors = _volumeLevelTable.Validate();
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
    }
}
