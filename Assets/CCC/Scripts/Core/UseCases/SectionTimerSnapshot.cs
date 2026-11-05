using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// SectionTimerが公開する読み取り専用の現在値です。
    /// </summary>
    public readonly struct SectionTimerSnapshot
    {
        /// <summary>
        /// このSnapshotが対象とするミッション識別子を取得します。
        /// </summary>
        public MissionId MissionId { get; }

        /// <summary>
        /// タイマーの状態を取得します。
        /// </summary>
        public SectionTimerStatus Status { get; }

        /// <summary>
        /// 通常時間の残りを取得します。
        /// </summary>
        public TimeSpan NormalRemainingTime { get; }

        /// <summary>
        /// ボーナス時間の残りを取得します。
        /// </summary>
        public TimeSpan BonusRemainingTime { get; }

        /// <summary>
        /// 開始時の通常時間を取得します。
        /// </summary>
        public TimeSpan NormalDuration { get; }

        /// <summary>
        /// 開始時のボーナス時間を取得します。
        /// </summary>
        public TimeSpan BonusDuration { get; }

        /// <summary>
        /// ボーナス込みの残り時間を取得します。
        /// </summary>
        public TimeSpan RemainingTime => NormalRemainingTime + BonusRemainingTime;

        /// <summary>
        /// 残り時間を通常時間とボーナス時間に分けてSnapshotを生成します。
        /// ボーナス時間は通常時間より先に消費されます。
        /// </summary>
        /// <param name="missionId">対象ミッションの識別子。</param>
        /// <param name="status">タイマーの状態。</param>
        /// <param name="remainingTime">ボーナス込みの残り時間。</param>
        /// <param name="normalDuration">開始時の通常時間。</param>
        /// <param name="bonusDuration">開始時のボーナス時間。</param>
        /// <exception cref="ArgumentOutOfRangeException">状態が未定義、または時間が範囲外の場合。</exception>
        public SectionTimerSnapshot(
            MissionId missionId,
            SectionTimerStatus status,
            TimeSpan remainingTime,
            TimeSpan normalDuration,
            TimeSpan bonusDuration)
        {
            if (!Enum.IsDefined(typeof(SectionTimerStatus), status))
            {
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            if (normalDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(normalDuration), normalDuration, "Normal duration must be positive.");
            }

            if (bonusDuration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(bonusDuration), bonusDuration, "Bonus duration must be zero or positive.");
            }

            if (remainingTime < TimeSpan.Zero || remainingTime > normalDuration + bonusDuration)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(remainingTime),
                    remainingTime,
                    "Remaining time must be between zero and the total duration.");
            }

            MissionId = missionId;
            Status = status;
            NormalDuration = normalDuration;
            BonusDuration = bonusDuration;
            // ボーナス時間は通常時間より先に消費するため、通常時間を超える分がボーナスの残りになる
            BonusRemainingTime = remainingTime > normalDuration ? remainingTime - normalDuration : TimeSpan.Zero;
            NormalRemainingTime = remainingTime - BonusRemainingTime;
        }
    }
}
