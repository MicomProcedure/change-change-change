using System;
using Ccc.Core.Model;
using UnityEngine;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    /// 難易度と、その難易度の選択中に表示する説明GameObjectの組み合わせです。
    /// </summary>
    [Serializable]
    public sealed class DifficultyDescriptionEntry
    {
        [SerializeField] private GameDifficulty _difficulty;
        [SerializeField] private GameObject _descriptionRoot;

        /// <summary>
        /// 説明の対象となる難易度を取得します。
        /// </summary>
        public GameDifficulty Difficulty
        {
            get { return _difficulty; }
        }

        /// <summary>
        /// 難易度の選択中に表示する説明GameObjectを取得します。
        /// </summary>
        public GameObject DescriptionRoot
        {
            get { return _descriptionRoot; }
        }
    }
}
