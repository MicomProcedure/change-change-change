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
        private AudioKey key;

        [SerializeField]
        private AudioClip clip;

        [SerializeField, Range(0f, 1f)]
        private float volume = 1f;

        /// <summary>
        ///     登録キーマッピング。
        /// </summary>
        public AudioKey Key => key;

        /// <summary>
        ///     再生するAudioClip。
        /// </summary>
        public AudioClip Clip => clip;

        /// <summary>
        ///     音量。
        /// </summary>
        public float Volume => volume;

        /// <summary>
        ///     指定された設定でAudioEntryを生成する。
        /// </summary>
        /// <param name="key">登録するサウンドキー。</param>
        /// <param name="clip">再生するクリップ。</param>
        /// <param name="volume">音量。</param>
        public AudioEntry(AudioKey key, AudioClip clip, float volume)
        {
            this.key = key;
            this.clip = clip;
            this.volume = volume;
        }

        public AudioEntry()
        {
        }
    }
}
