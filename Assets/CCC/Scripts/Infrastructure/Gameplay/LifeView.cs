using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面の残りLifeを表示するViewです。
    /// </summary>
    public sealed class LifeView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _lifeText;
        [SerializeField] private List<GameObject> _sprites;

        private void Awake()
        {
            Render(0);
        }

        /// <summary>
        /// 現在の残りLifeを文言とImageへ反映します。
        /// </summary>
        /// <param name="life">表示するLife数。</param>
        public void Render(int life)
        {
            if (life < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(life), life, "Life must be zero or positive.");
            }

            if (_lifeText != null)
            {
                _lifeText.text = life.ToString();
            }

            if (_sprites == null || _sprites.Count == 0)
            {
                return;
            }

            if (life > _sprites.Count)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(life),
                    life,
                    $"LifeView: Render called with life={life}, but only {_sprites.Count} images are available.");
            }

            for (int index = 0; index < _sprites.Count; index++)
            {
                GameObject sprite = _sprites[index];
                if (sprite == null)
                {
                    continue;
                }

                bool isEnabled = index < life;
                sprite.SetActive(isEnabled);
            }
        }
    }
}
