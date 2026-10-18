using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// ボーナスインジケーターの割合計算方式です。
    /// </summary>
    public enum BonusIndicatorMode
    {
        /// <summary>Section制限時間を100%として残りボーナス秒数を表示します。</summary>
        SectionScaled = 0,

        /// <summary>ボーナス量に関係なく100%から0%まで減らします。</summary>
        FullToEmpty,

        /// <summary>Section制限時間に関係なく一定速度で減らします。開始割合はボーナス秒数×減少速度です。</summary>
        FixedSpeed
    }

    /// <summary>
    /// Gameplay画面の残り時間を表示するViewです。
    /// </summary>
    public sealed class TimerView : MonoBehaviour
    {
        private const string RemainingSecondsFormat = "0.0";
        private const float EmptyRatio = 0f;
        private const float EmptySeconds = 0f;
        private const float FullSectionRatio = 1f;

        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TimerIndicator _timerIndicator;
        [SerializeField] private TimerIndicator _bonusTimerIndicator;

        [SerializeField] private BonusIndicatorMode _bonusIndicatorMode;
        [SerializeField] private float _fixedBonusDecreaseRatioPerSecond;

        private float _bonusStartSeconds;
        private float _previousBonusSeconds;

        /// <summary>
        /// 残り時間の数値とインジケーターを更新します。
        /// 残り時間のうちSection制限時間を超える分をボーナスとして表示します。
        /// </summary>
        /// <param name="hasRemainingTime">残り時間を表示するかどうか。</param>
        /// <param name="remainingSeconds">表示する残り秒数（ボーナス込み）。</param>
        /// <param name="sectionDurationSeconds">現在Sectionの通常制限時間。</param>
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

            if (!hasRemainingTime ||
                float.IsNaN(remainingSeconds) ||
                float.IsInfinity(remainingSeconds) ||
                remainingSeconds < 0f ||
                float.IsNaN(sectionDurationSeconds) ||
                float.IsInfinity(sectionDurationSeconds) ||
                sectionDurationSeconds <= 0f)
            {
                ResetBonusState();
                RenderIndicators(EmptyRatio, EmptyRatio);
                return;
            }

            float normalRemainingRatio =
                Mathf.Min(remainingSeconds / sectionDurationSeconds, FullSectionRatio);

            float bonusSeconds = Mathf.Max(remainingSeconds - sectionDurationSeconds, EmptySeconds);
            float bonusRemainingRatio = CalculateBonusRemainingRatio(bonusSeconds, sectionDurationSeconds);

            RenderIndicators(normalRemainingRatio, bonusRemainingRatio);
        }

        private void RenderIndicators(float normalRemainingRatio, float bonusRemainingRatio)
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

        private float CalculateBonusRemainingRatio(float bonusSeconds, float sectionDurationSeconds)
        {
            if (bonusSeconds <= EmptySeconds)
            {
                ResetBonusState();
                return EmptyRatio;
            }

            bool isBonusIncreased = bonusSeconds > _previousBonusSeconds;
            _previousBonusSeconds = bonusSeconds;

            switch (_bonusIndicatorMode)
            {
                case BonusIndicatorMode.FullToEmpty:
                    if (isBonusIncreased)
                    {
                        _bonusStartSeconds = bonusSeconds;
                    }

                    return bonusSeconds / _bonusStartSeconds;

                case BonusIndicatorMode.FixedSpeed:
                    // ボーナスゲージはSection秒に関係なく固定速度で減る
                    return bonusSeconds * _fixedBonusDecreaseRatioPerSecond;

                default:
                    return bonusSeconds / sectionDurationSeconds;
            }
        }

        private void ResetBonusState()
        {
            _bonusStartSeconds = EmptySeconds;
            _previousBonusSeconds = EmptySeconds;
        }
    }
}