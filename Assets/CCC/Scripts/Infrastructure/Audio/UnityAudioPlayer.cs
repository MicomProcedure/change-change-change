using System;
using System.Collections.Generic;
using Ccc.Presentation.Audio;
using UnityEngine;

namespace Ccc.Infrastructure.Audio
{
    /// <summary>
    /// AudioCatalogAssetの定義を2つのAudioSourceで再生するUnity実装です。
    /// </summary>
    public sealed class UnityAudioPlayer : MonoBehaviour, IAudioPlayer
    {
        [SerializeField] private AudioCatalogAsset catalog;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;

        private readonly HashSet<AudioKey> _warnedKeys = new HashSet<AudioKey>();
        private AudioKey _currentBgmKey;

        /// <summary>
        /// Inspector参照を検証し、Audio Catalogの不正をログへ出力します。
        /// </summary>
        private void Awake()
        {
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

            if (bgmSource != null && ReferenceEquals(bgmSource, seSource))
            {
                configurationErrors.Add(
                    "UnityAudioPlayer requires different AudioSource instances for BGM and SE.");
            }

            for (int index = 0; index < configurationErrors.Count; index++)
            {
                Debug.LogError($"[UnityAudioPlayer] {name}: {configurationErrors[index]}");
            }

            if (configurationErrors.Count > 0)
            {
                return;
            }

            IReadOnlyList<string> errors = catalog.Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                Debug.LogError($"[UnityAudioPlayer] {name}: {errors[index]}");
            }
        }

        /// <summary>
        /// 指定されたSEをCatalogから検索して再生します。
        /// </summary>
        /// <param name="key">再生するSEのキー。</param>
        public void PlaySE(AudioKey key)
        {
            if (catalog == null || seSource == null || !catalog.TryGetSE(key, out AudioEntry entry))
            {
                WarnMissingKey(key, "SE");
                return;
            }

            seSource.PlayOneShot(entry.Clip, entry.Volume);
        }

        /// <summary>
        /// 指定されたBGMへ切り替えます。同じBGMが再生中なら再起動しません。
        /// </summary>
        /// <param name="key">再生するBGMのキー。</param>
        public void PlayBgm(AudioKey key)
        {
            if (catalog == null || bgmSource == null || !catalog.TryGetBgm(key, out AudioEntry entry))
            {
                WarnMissingKey(key, "BGM");
                return;
            }

            if (_currentBgmKey == key && bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.Stop();
            bgmSource.clip = entry.Clip;
            bgmSource.volume = entry.Volume;
            bgmSource.loop = true;
            bgmSource.Play();
            _currentBgmKey = key;
        }

        /// <summary>
        /// 再生中のBGMを停止します。
        /// </summary>
        public void StopBgm()
        {
            if (bgmSource == null)
            {
                return;
            }

            bgmSource.Stop();
            _currentBgmKey = AudioKey.None;
        }

        private void WarnMissingKey(AudioKey key, string category)
        {
            if (_warnedKeys.Add(key))
            {
                Debug.LogWarning(
                    $"[UnityAudioPlayer] {category} key is not configured or is invalid: {key}.");
            }
        }
    }
}
