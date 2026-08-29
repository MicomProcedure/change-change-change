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
        /// 残り時間を取得します。
        /// </summary>
        public TimeSpan RemainingTime { get; }

        /// <summary>
        /// 指定された値からSectionTimerのSnapshotを生成します。
        /// </summary>
        /// <param name="missionId">対象ミッションの識別子。</param>
        /// <param name="status">タイマーの状態。</param>
        /// <param name="remainingTime">残り時間。</param>
        /// <exception cref="ArgumentOutOfRangeException">状態が未定義、または残り時間が負の場合。</exception>
        public SectionTimerSnapshot(
            MissionId missionId,
            SectionTimerStatus status,
            TimeSpan remainingTime)
        {
            if (!Enum.IsDefined(typeof(SectionTimerStatus), status))
            {
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            if (remainingTime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(remainingTime),
                    remainingTime,
                    "Remaining time must be zero or positive.");
            }

            MissionId = missionId;
            Status = status;
            RemainingTime = remainingTime;
        }
    }
}
