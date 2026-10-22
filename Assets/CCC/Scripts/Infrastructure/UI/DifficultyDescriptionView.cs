using System.Collections.Generic;
using Ccc.Core.Model;
using UnityEngine;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    /// 選択中の難易度に対応する説明GameObjectだけを表示します。
    /// </summary>
    public sealed class DifficultyDescriptionView : MonoBehaviour
    {
        [Tooltip("難易度ごとに表示する説明GameObject。対応する項目がない難易度の選択中は全て非表示にする。")]
        [SerializeField] private List<DifficultyDescriptionEntry> _descriptionEntries = new();

        /// <summary>
        /// 指定した難易度に対応する説明GameObjectが設定されているかを判定します。
        /// </summary>
        /// <param name="difficulty">判定する難易度。</param>
        /// <returns>説明GameObjectが設定されている場合はtrue。</returns>
        public bool HasDescription(GameDifficulty difficulty)
        {
            if (_descriptionEntries == null)
            {
                return false;
            }

            for (int index = 0; index < _descriptionEntries.Count; index++)
            {
                DifficultyDescriptionEntry entry = _descriptionEntries[index];
                if (entry != null && entry.Difficulty == difficulty && entry.DescriptionRoot != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 指定した難易度の説明を表示し、それ以外の説明を非表示にします。
        /// </summary>
        /// <param name="difficulty">選択中の難易度。</param>
        public void Render(GameDifficulty difficulty)
        {
            if (_descriptionEntries == null)
            {
                return;
            }

            for (int index = 0; index < _descriptionEntries.Count; index++)
            {
                DifficultyDescriptionEntry entry = _descriptionEntries[index];
                if (entry == null || entry.DescriptionRoot == null)
                {
                    continue;
                }

                entry.DescriptionRoot.SetActive(entry.Difficulty == difficulty);
            }
        }
    }
}
