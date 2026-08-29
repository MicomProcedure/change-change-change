using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// SectionTimerが指定Missionの満了を通知するSignalです。
    /// </summary>
    public readonly struct SectionTimerExpiredSignal
    {
        /// <summary>
        /// 満了したMissionIdを取得します。
        /// </summary>
        public MissionId MissionId { get; }

        /// <summary>
        /// MissionIdから満了Signalを生成します。
        /// </summary>
        /// <param name="missionId">満了したMissionId。</param>
        public SectionTimerExpiredSignal(MissionId missionId)
        {
            MissionId = missionId;
        }
    }
}
