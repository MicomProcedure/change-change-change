using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 提出結果から、次に提示するMissionへ付与するボーナス時間を算出します。
    /// ボーナスは通常時間より先に消費される前提で、残り時間のうち通常時間を超える分を未消費ボーナスとみなします。
    /// </summary>
    public static class BonusTimeCalculator
    {
        /// <summary>
        /// 次に提示するMissionへ付与するボーナス時間を算出します。
        /// </summary>
        /// <param name="timerConfig">タイマー設定。</param>
        /// <param name="currentSection">提出時点のSection。</param>
        /// <param name="remainingTime">提出時点の残り時間（ボーナス込み）。</param>
        /// <param name="isSuccess">提出が成功したかどうか。</param>
        /// <param name="trigger">提出のきっかけ。</param>
        /// <returns>次に提示するMissionへ付与するボーナス時間。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timerConfig"/> が null の場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="remainingTime"/> が負の場合。</exception>
        public static TimeSpan CalculateNextBonusDuration(
            TimerConfig timerConfig,
            int currentSection,
            TimeSpan remainingTime,
            bool isSuccess,
            SubmissionTrigger trigger)
        {
            if (timerConfig == null)
            {
                throw new ArgumentNullException(nameof(timerConfig));
            }

            if (remainingTime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(remainingTime), remainingTime, "Remaining time must be zero or positive.");
            }

            TimeSpan currentDuration = timerConfig.GetDuration(currentSection);
            TimeSpan unusedBonus = remainingTime > currentDuration
                ? remainingTime - currentDuration
                : TimeSpan.Zero;

            if (!isSuccess)
            {
                return timerConfig.IsBonusCarryOverOnFailureEnabled
                    ? ClampBonusDuration(timerConfig, unusedBonus)
                    : TimeSpan.Zero;
            }

            TimeSpan normalRemainingTime = remainingTime < currentDuration
                ? remainingTime
                : currentDuration;
            TimeSpan earnedBonus = Multiply(normalRemainingTime, timerConfig.BonusRatio);
            if (trigger == SubmissionTrigger.ManualSubmit)
            {
                earnedBonus += timerConfig.ManualSubmitBonusDuration;
            }

            TimeSpan nextBonus = timerConfig.IsBonusCarryOverOnSuccessEnabled
                ? earnedBonus + unusedBonus
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