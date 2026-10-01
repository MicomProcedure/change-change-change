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
        [SerializeField] private TimerIndicator _timerIndicator;


        /// <summary>
        /// 残り時間の表示を更新します。
        /// </summary>
        /// <param name="hasRemainingTime">残り時間を表示するかどうか。</param>
        /// <param name="remainingSeconds">表示する残り秒数。</param>
        /// <param name="sectionDurationSeconds">現在Sectionの制限時間。</param>
        public void Render(
            bool hasRemainingTime,
            float remainingSeconds,
            float sectionDurationSeconds)
        {
            if (_timerText != null)
            {
                _timerText.text = hasRemainingTime
                    ? remainingSeconds.ToString(RemainingSecondsFormat)
                    : string.Empty;
            }

            if (_timerIndicator == null)
            {
                return;
            }

            if (!hasRemainingTime ||
                float.IsNaN(remainingSeconds) ||
                float.IsInfinity(remainingSeconds) ||
                remainingSeconds < 0f ||
                float.IsNaN(sectionDurationSeconds) ||
                float.IsInfinity(sectionDurationSeconds) ||
                sectionDurationSeconds <= 0f)
            {
                _timerIndicator.Render(0f);
                return;
            }

            float remainingTimeRatio =
                remainingSeconds / sectionDurationSeconds;

            _timerIndicator.Render(remainingTimeRatio);
        }
    }
}