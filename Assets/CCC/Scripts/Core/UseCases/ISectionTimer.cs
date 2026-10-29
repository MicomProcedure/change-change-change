using System;
using R3;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Section単位の制限時間を管理するUseCaseポートです。
    /// </summary>
    public interface ISectionTimer : IDisposable
    {
        /// <summary>
        /// Timer表示用Snapshotのストリームを取得します。
        /// </summary>
        Observable<SectionTimerSnapshot> Snapshots { get; }

        /// <summary>
        /// Timer満了Signalのストリームを取得します。
        /// </summary>
        Observable<SectionTimerExpiredSignal> ExpiredSignals { get; }

        /// <summary>
        /// 指定MissionのTimerを開始します。
        /// </summary>
        /// <param name="missionId">対象MissionId。</param>
        /// <param name="normalDuration">Sectionの通常制限時間。</param>
        /// <param name="bonusDuration">付与するボーナス時間。</param>
        void Start(Ccc.Core.Model.MissionId missionId, TimeSpan normalDuration, TimeSpan bonusDuration);

        /// <summary>
        /// 指定Missionと一致する実行中Timerを停止し、停止時点のSnapshotを返します。
        /// </summary>
        /// <param name="missionId">停止対象MissionId。</param>
        /// <param name="stoppedSnapshot">停止時点のSnapshot。停止しなかった場合はdefault。</param>
        /// <returns>停止した場合はtrue。満了済み・不一致などで停止しなかった場合はfalse。</returns>
        bool TryStop(Ccc.Core.Model.MissionId missionId, out SectionTimerSnapshot stoppedSnapshot);

        /// <summary>
        /// 実行中のTimerをすべて停止します。
        /// </summary>
        void StopAll();
    }
}
