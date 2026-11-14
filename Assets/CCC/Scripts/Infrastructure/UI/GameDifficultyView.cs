using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using UnityEngine;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    /// 難易度に対応する表示用GameObjectを切り替えるViewです。
    /// </summary>
    public sealed class GameDifficultyView : MonoBehaviour
    {
        [Tooltip("難易度と表示するGameObjectの組み合わせ。数字や文言は各GameObjectに設定する。同じGameObjectを異なる難易度に重複登録しない。")]
        [SerializeField] private List<DifficultyDisplayEntry> _displayEntries = new();

        /// <summary>
        /// 指定した難易度のGameObjectだけを表示します。対応する項目がない場合はすべて非表示にします。
        /// </summary>
        /// <param name="difficulty">表示する難易度。</param>
        /// <exception cref="ArgumentOutOfRangeException">難易度が未定義の場合。</exception>
        public void Render(GameDifficulty difficulty)
        {
            if (!Enum.IsDefined(typeof(GameDifficulty), difficulty))
            {
                throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null);
            }

            if (_displayEntries == null)
            {
                return;
            }

            for (int index = 0; index < _displayEntries.Count; index++)
            {
                DifficultyDisplayEntry entry = _displayEntries[index];
                if (entry == null || entry.DisplayRoot == null)
                {
                    continue;
                }

                entry.DisplayRoot.SetActive(entry.Difficulty == difficulty);
            }
        }

        /// <summary>
        /// 難易度と、その難易度で表示するGameObjectの組み合わせです。
        /// </summary>
        [Serializable]
        private sealed class DifficultyDisplayEntry
        {
            [SerializeField] private GameDifficulty _difficulty;
            [SerializeField] private GameObject _displayRoot;

            /// <summary>
            /// 表示対象の難易度を取得します。
            /// </summary>
            public GameDifficulty Difficulty => _difficulty;

            /// <summary>
            /// 難易度に対応する表示用GameObjectを取得します。
            /// </summary>
            public GameObject DisplayRoot => _displayRoot;
        }
    }
}
