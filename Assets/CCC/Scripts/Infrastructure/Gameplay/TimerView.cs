using System;
using Ccc.Presentation;
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

        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TimerIndicator _timerIndicator;
        [SerializeField] private TimerIndicator _bonusTimerIndicator;

        [SerializeField] private BonusIndicatorMode _bonusIndicatorMode;
        [SerializeField] private float _fixedBonusDecreaseRatioPerSecond;

        /// <summary>
        /// 残り時間の数値とインジケーターを更新します。
        /// </summary>
        /// <param name="state">Gameplay画面の表示状態。</param>
        /// <exception cref="ArgumentNullException"><paramref name="state"/> が null の場合。</exception>
        public void Render(GameplayViewStateDto state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (_timerText != null)
            {
                _timerText.text = state.HasRemainingTime
                    ? state.RemainingSeconds.ToString(RemainingSecondsFormat)
                    : string.Empty;
            }

            if (!state.HasRemainingTime || state.NormalDurationSeconds <= EmptySeconds)
            {
                RenderIndicators(EmptyRatio, EmptyRatio);
                return;
            }

            float normalRemainingRatio = state.NormalRemainingSeconds / state.NormalDurationSeconds;
            RenderIndicators(normalRemainingRatio, CalculateBonusRemainingRatio(state));
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

        private float CalculateBonusRemainingRatio(GameplayViewStateDto state)
        {
            if (state.BonusRemainingSeconds <= EmptySeconds || state.BonusDurationSeconds <= EmptySeconds)
            {
                return EmptyRatio;
            }

            switch (_bonusIndicatorMode)
            {
                case BonusIndicatorMode.FullToEmpty:
                    return state.BonusRemainingSeconds / state.BonusDurationSeconds;

                case BonusIndicatorMode.FixedSpeed:
                    // ボーナスゲージはSection秒に関係なく固定速度で減る
                    return state.BonusRemainingSeconds * _fixedBonusDecreaseRatioPerSecond;

                default:
                    return state.BonusRemainingSeconds / state.NormalDurationSeconds;
            }
        }
    }
}