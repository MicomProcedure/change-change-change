using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面のSection、Tier、Mission文言を表示するViewです。
    /// </summary>
    public sealed class MissionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _sectionText;
        [SerializeField] private TMP_Text _tierText;
        [SerializeField] private TMP_Text _missionText;

        /// <summary>
        /// Missionに関する表示を更新します。
        /// </summary>
        /// <param name="section">表示するSection。</param>
        /// <param name="tier">表示するTier。</param>
        /// <param name="missionText">表示するMission文言。</param>
        public void Render(int section, int tier, string missionText)
        {
            SetText(_sectionText, section.ToString());
            SetText(_tierText, tier.ToString());
            SetText(_missionText, missionText);
        }

        private static void SetText(TMP_Text text, string textValue)
        {
            if (text != null)
            {
                text.text = textValue;
            }
        }
    }
}
