using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面のScoreとMission文言を表示するViewです。
    /// </summary>
    public sealed class MissionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _missionText;

        /// <summary>
        /// Missionに関する表示を更新します。
        /// </summary>
        /// <param name="score">表示するScore。</param>
        /// <param name="missionText">表示するMission文言。</param>
        public void Render(int score, string missionText)
        {
            SetText(_scoreText, score.ToString());
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
