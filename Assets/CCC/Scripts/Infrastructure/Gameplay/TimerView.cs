using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{

    // / <summary>
    // / ボーナスインジケーターの割合計算方式です。
    // / </summary>
    // public enum BonusIndicatorMode
    // {
    //     /// <summary>Section制限時間を100%として残りボーナス秒数を表示します。</summary>
    //     SectionScaled = 0,

    //     /// <summary>ボーナス量に関係なく100%から0%まで減らします。</summary>
    //     FullToEmpty,

    //     /// <summary>Section制限時間に関係なく一定速度で減らします。開始割合はボーナス秒数×減少速度です。</summary>
    //     FixedSpeed
    // }
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
        // [SerializeField] private TimerIndicator _bonusTimerIndicator;

        // [SerializeField] private BonusIndicatorMode _bonusIndicatorMode;
        // [SerializeField] private float _fixedBonusDecreaseRatioPerSecond;

        // private float _bonusStartSeconds;
        // private float _previousBonusSeconds;

        // #if UNITY_EDITOR
        // [SerializeField] private bool _isBonusPreviewEnabled;
        // [SerializeField] private float _previewBonusSeconds;
        // #endif

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
            // #if UNITY_EDITOR
            // if (_isBonusPreviewEnabled &&
            //     hasRemainingTime &&
            //     sectionDurationSeconds > 0f &&
            //     _previewBonusSeconds > 0f)
            // {
            //     remainingSeconds += _previewBonusSeconds;
            // }
            // #endif

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
                RenderIndicators(EmptyRatio);
                return;
            }

            float remainingTimeRatio =
                remainingSeconds / sectionDurationSeconds;

            float normalRemainingRatio =
                Mathf.Min(remainingTimeRatio, FullSectionRatio);

            // float bonusSeconds = Mathf.Max(remainingSeconds - sectionDurationSeconds, EmptyRatio);
            // float bonusRemainingRatio = CalculateBonusRemainingRatio(bonusSeconds, sectionDurationSeconds);

            RenderIndicators(normalRemainingRatio);
        }

        private void RenderIndicators(float normalRemainingRatio)
        {
            if (_timerIndicator != null)
            {
                _timerIndicator.Render(normalRemainingRatio);
            }

            // if (_bonusTimerIndicator != null)
            // {
            //     _bonusTimerIndicator.Render(bonusRemainingRatio);
            // }
        }

        // private float CalculateBonusRemainingRatio(float bonusSeconds, float sectionDurationSeconds)
        // {
        //     if (bonusSeconds <= 0f)
        //     {
        //         ResetBonusState();
        //         return EmptyRatio;
        //     }

        //     bool isBonusIncreased = bonusSeconds > _previousBonusSeconds;
        //     _previousBonusSeconds = bonusSeconds;

        //     switch (_bonusIndicatorMode)
        //     {
        //         case BonusIndicatorMode.FullToEmpty:
        //             if (isBonusIncreased)
        //             {
        //                 _bonusStartSeconds = bonusSeconds;
        //             }
        //             return bonusSeconds / _bonusStartSeconds;

        //         case BonusIndicatorMode.FixedSpeed:
        //             // ボーナスゲージはSection秒に関係なく固定速度で減る
        //             return bonusSeconds * _fixedBonusDecreaseRatioPerSecond;

        //         default:
        //             return bonusSeconds / sectionDurationSeconds;
        //     }
        // }

        // private void ResetBonusState()
        // {
        //     _bonusStartSeconds = 0f;
        //     _previousBonusSeconds = 0f;
        // }
    }
}