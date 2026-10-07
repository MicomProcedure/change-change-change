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
        private const float EmptyRatio = 0f;
        private const float FullSectionRatio = 1f;

        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TimerIndicator _timerIndicator;
        [SerializeField] private TimerIndicator _bonusTimerIndicator;

        #if UNITY_EDITOR
        [SerializeField] private bool _isBonusPreviewEnabled;
        [SerializeField] private float _previewBonusSeconds;
        #endif

        /// <summary>
        /// 残り時間の数値とインジケーターを更新します。
        /// </summary>
        /// <param name="hasRemainingTime">残り時間を表示するかどうか。</param>
        /// <param name="remainingSeconds">表示する残り秒数。</param>
        /// <param name="sectionDurationSeconds">現在Sectionの制限時間。</param>
        public void Render(
            bool hasRemainingTime,
            float remainingSeconds,
            float sectionDurationSeconds)
        {
            #if UNITY_EDITOR
            if (_isBonusPreviewEnabled &&
                hasRemainingTime &&
                sectionDurationSeconds > 0f &&
                _previewBonusSeconds > 0f)
            {
                remainingSeconds += _previewBonusSeconds;
            }
            #endif

            if (_timerText != null)
            {
                _timerText.text = hasRemainingTime
                    ? remainingSeconds.ToString(RemainingSecondsFormat)
                    : string.Empty;
            }

            if (!hasRemainingTime ||
                float.IsNaN(remainingSeconds) ||
                float.IsInfinity(remainingSeconds) ||
                remainingSeconds < 0f ||
                float.IsNaN(sectionDurationSeconds) ||
                float.IsInfinity(sectionDurationSeconds) ||
                sectionDurationSeconds <= 0f)
            {
                RenderIndicators(EmptyRatio, EmptyRatio);
                return;
            }

            float remainingTimeRatio =
                remainingSeconds / sectionDurationSeconds;

            float normalRemainingRatio =
                Mathf.Min(remainingTimeRatio, FullSectionRatio);

            float bonusRemainingRatio =
                Mathf.Max(remainingTimeRatio - FullSectionRatio, EmptyRatio);

            RenderIndicators(normalRemainingRatio, bonusRemainingRatio);
        }

        private void RenderIndicators(
            float normalRemainingRatio,
            float bonusRemainingRatio)
        {
            if (_timerIndicator != null)
            {
                _timerIndicator.Render(normalRemainingRatio);
            }

            if (_bonusTimerIndicator != null)
            {
                _bonusTimerIndicator.Render(bonusRemainingRatio);
            }
        }
    }
}