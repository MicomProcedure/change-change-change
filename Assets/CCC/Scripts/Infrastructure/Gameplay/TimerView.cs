using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面の残り時間を表示するViewです。
    /// </summary>
    public sealed class TimerView : MonoBehaviour
    {
        private const string RemainingSecondsFormat = "0.0";

        [SerializeField] private TMP_Text _timerText;

        /// <summary>
        /// 残り時間の表示を更新します。
        /// </summary>
        /// <param name="hasRemainingTime">残り時間を表示するかどうか。</param>
        /// <param name="remainingSeconds">表示する残り秒数。</param>
        public void Render(bool hasRemainingTime, float remainingSeconds)
        {
            if (_timerText == null)
            {
                return;
            }

            _timerText.text = hasRemainingTime
                ? remainingSeconds.ToString(RemainingSecondsFormat)
                : string.Empty;
        }
    }
}
