using System;

namespace Ccc.Core.Model
{
    public sealed class TimerConfig
    {
        private const double DisabledBonusRatio = 0d;
        private const double DefaultMaximumBonusRatio = 1d;

        public TimeSpan InitialDuration { get; }
        public TimeSpan ReductionPerSection { get; }
        public TimeSpan MinimumDuration { get; }

        /// <summary>
        /// 成功時に「通常時間の残り」へ掛けてボーナスを算出する割合を取得します。0の場合は割合ボーナスを付与しません。
        /// </summary>
        public double BonusRatio { get; }

        /// <summary>
        /// 手動Submitで成功した場合に追加で付与する固定ボーナス時間を取得します。
        /// </summary>
        public TimeSpan ManualSubmitBonusDuration { get; }

        /// <summary>
        /// 付与先Sectionの通常制限時間に対するボーナス上限の割合を取得します。
        /// </summary>
        public double MaximumBonusRatio { get; }

        /// <summary>
        /// 成功時に、使い切らなかったボーナスを次のSectionへ持ち越すかどうかを取得します。
        /// </summary>
        public bool IsBonusCarryOverOnSuccessEnabled { get; }

        /// <summary>
        /// 失敗時に、使い切らなかったボーナスを同じSectionの次Missionへ持ち越すかどうかを取得します。
        /// </summary>
        public bool IsBonusCarryOverOnFailureEnabled { get; }

        public TimerConfig(
            TimeSpan initialDuration,
            TimeSpan reductionPerSection,
            TimeSpan minimumDuration,
            double bonusRatio = DisabledBonusRatio,
            double maximumBonusRatio = DefaultMaximumBonusRatio,
            bool isBonusCarryOverOnSuccessEnabled = false,
            bool isBonusCarryOverOnFailureEnabled = false,
            TimeSpan manualSubmitBonusDuration = default)
        {
            if (initialDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(initialDuration), initialDuration, "Initial duration must be positive.");
            }

            if (minimumDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumDuration), minimumDuration, "Minimum duration must be positive.");
            }

            if (reductionPerSection < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(reductionPerSection), reductionPerSection, "Reduction per section must be zero or positive.");
            }

            if (minimumDuration > initialDuration)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumDuration), minimumDuration, "Minimum duration must not exceed the initial duration.");
            }

            if (double.IsNaN(bonusRatio) || double.IsInfinity(bonusRatio) || bonusRatio < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(bonusRatio), bonusRatio, "Bonus ratio must be finite and zero or positive.");
            }

            if (double.IsNaN(maximumBonusRatio) || double.IsInfinity(maximumBonusRatio) || maximumBonusRatio < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumBonusRatio), maximumBonusRatio, "Maximum bonus ratio must be finite and zero or positive.");
            }

            if (manualSubmitBonusDuration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(manualSubmitBonusDuration), manualSubmitBonusDuration, "Manual submit bonus duration must be zero or positive.");
            }

            InitialDuration = initialDuration;
            ReductionPerSection = reductionPerSection;
            MinimumDuration = minimumDuration;
            BonusRatio = bonusRatio;
            MaximumBonusRatio = maximumBonusRatio;
            IsBonusCarryOverOnSuccessEnabled = isBonusCarryOverOnSuccessEnabled;
            IsBonusCarryOverOnFailureEnabled = isBonusCarryOverOnFailureEnabled;
            ManualSubmitBonusDuration = manualSubmitBonusDuration;
        }

        public TimeSpan GetDuration(int section)
        {
            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }

            var reducedDuration = InitialDuration - (ReductionPerSection * (section - 1));
            if (reducedDuration < MinimumDuration)
            {
                return MinimumDuration;
            }

            return reducedDuration;
        }
    }
}