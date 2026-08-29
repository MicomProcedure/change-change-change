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
        /// <param name="duration">制限時間。</param>
        void Start(Ccc.Core.Model.MissionId missionId, TimeSpan duration);

        /// <summary>
        /// 指定Missionと一致するTimerを停止します。
        /// </summary>
        /// <param name="missionId">停止対象MissionId。</param>
        void Stop(Ccc.Core.Model.MissionId missionId);

        /// <summary>
        /// 実行中のTimerをすべて停止します。
        /// </summary>
        void StopAll();
    }
}
