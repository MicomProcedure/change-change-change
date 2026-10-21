using System;
using Ccc.Presentation;
using UnityEngine;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// 画面の種類とプラットフォームの組み合わせに対応するScene名です。
    /// </summary>
    [Serializable]
    public sealed class SceneCatalogEntry
    {
        [SerializeField]
        private SceneKind sceneKind;

        [SerializeField]
        private PlayPlatform playPlatform;

        [SerializeField]
        private string sceneName;

        /// <summary>
        /// 遷移先となる画面の種類を取得します。
        /// </summary>
        public SceneKind SceneKind
        {
            get { return sceneKind; }
        }

        /// <summary>
        /// 対象とするプラットフォームを取得します。
        /// </summary>
        public PlayPlatform PlayPlatform
        {
            get { return playPlatform; }
        }

        /// <summary>
        /// ロードするScene名を取得します。
        /// </summary>
        public string SceneName
        {
            get { return sceneName; }
        }
    }
}
