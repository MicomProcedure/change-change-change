using System;
using UnityEngine;
using Ccc.Presentation.Audio;

namespace Ccc.Infrastructure.Audio
{
    /// <summary>
    ///     AudioCatalogAsset に登録される1件分のオーディオ定義。
    /// </summary>
    [Serializable]
    public sealed class AudioEntry
    {
        [SerializeField]
        private AudioKey _key;

        [SerializeField]
        private AudioClip _clip;

        [Tooltip("SE一覧では1を超える値で増幅できるが、音割れする可能性がある。BGM一覧は1以下で指定する。")]
        [SerializeField, Min(AudioVolumeRules.MinVolume)]
        private float _volume = 1f;

        /// <summary>
        ///     登録キーマッピング。
        /// </summary>
        public AudioKey Key => _key;

        /// <summary>
        ///     再生するAudioClip。
        /// </summary>
        public AudioClip Clip => _clip;

        /// <summary>
        ///     音量。
        /// </summary>
        public float Volume => _volume;

        /// <summary>
        ///     指定された設定でAudioEntryを生成する。
        /// </summary>
        /// <param name="key">登録するサウンドキー。</param>
        /// <param name="clip">再生するクリップ。</param>
        /// <param name="volume">音量。</param>
        public AudioEntry(AudioKey key, AudioClip clip, float volume)
        {
            _key = key;
            _clip = clip;
            _volume = volume;
        }

        public AudioEntry()
        {
        }
    }
}
