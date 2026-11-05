using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 提出結果から、次に提示するMissionへ付与するボーナス時間を算出します。
    /// </summary>
    public static class BonusTimeCalculator
    {
        /// <summary>
        /// 次に提示するMissionへ付与するボーナス時間を算出します。
        /// </summary>
        /// <param name="timerConfig">タイマー設定。</param>
        /// <param name="normalRemainingTime">提出時点の通常時間の残り。</param>
        /// <param name="bonusRemainingTime">提出時点のボーナス時間の残り。</param>
        /// <param name="isSuccess">提出が成功したかどうか。</param>
        /// <param name="trigger">提出のきっかけ。</param>
        /// <returns>次に提示するMissionへ付与するボーナス時間。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timerConfig"/> が null の場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">残り時間のいずれかが負の場合。</exception>
        public static TimeSpan CalculateNextBonusDuration(
            TimerConfig timerConfig,
            TimeSpan normalRemainingTime,
            TimeSpan bonusRemainingTime,
            bool isSuccess,
            SubmissionTrigger trigger)
        {
            if (timerConfig == null)
            {
                throw new ArgumentNullException(nameof(timerConfig));
            }

            if (normalRemainingTime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(normalRemainingTime), normalRemainingTime, "Remaining time must be zero or positive.");
            }

            if (bonusRemainingTime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(bonusRemainingTime), bonusRemainingTime, "Remaining time must be zero or positive.");
            }

            if (!isSuccess)
            {
                return timerConfig.IsBonusCarryOverOnFailureEnabled
                    ? ClampBonusDuration(timerConfig, bonusRemainingTime)
                    : TimeSpan.Zero;
            }

            TimeSpan earnedBonus = Multiply(normalRemainingTime, timerConfig.BonusRatio);
            if (trigger == SubmissionTrigger.ManualSubmit)
            {
                earnedBonus += timerConfig.ManualSubmitBonusDuration;
            }

            TimeSpan nextBonus = timerConfig.IsBonusCarryOverOnSuccessEnabled
                ? earnedBonus + bonusRemainingTime
                : earnedBonus;

            return ClampBonusDuration(timerConfig, nextBonus);
        }

        private static TimeSpan ClampBonusDuration(TimerConfig timerConfig, TimeSpan bonusDuration)
        {
            TimeSpan maximumBonus = timerConfig.MaximumBonusDuration;
            return bonusDuration < maximumBonus ? bonusDuration : maximumBonus;
        }

        private static TimeSpan Multiply(TimeSpan duration, double ratio)
        {
            return TimeSpan.FromTicks((long)(duration.Ticks * ratio));
        }
    }
}