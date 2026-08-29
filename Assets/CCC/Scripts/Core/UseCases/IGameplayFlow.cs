using System;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using R3;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Gameplayの状態遷移と表示用ストリームを公開するUseCaseポートです。
    /// </summary>
    public interface IGameplayFlow
    {
        /// <summary>
        /// Gameplay状態の更新ストリームを取得します。
        /// </summary>
        Observable<GameplayState> States { get; }

        /// <summary>
        /// 提出式の更新ストリームを取得します。
        /// </summary>
        Observable<Expression> Expressions { get; }

        /// <summary>
        /// Gameplayイベントのストリームを取得します。
        /// </summary>
        Observable<GameplayEvent> Events { get; }

        /// <summary>
        /// Gameplay継続不能エラーのストリームを取得します。
        /// </summary>
        Observable<GameplayFlowFailure> Failures { get; }

        /// <summary>
        /// SectionTimerの表示用Snapshotストリームを取得します。
        /// </summary>
        Observable<SectionTimerSnapshot> TimerSnapshots { get; }

        /// <summary>
        /// Gameplayセッションを開始します。
        /// </summary>
        /// <param name="request">セッション開始Request。</param>
        /// <returns>開始結果。</returns>
        GameplayStartResult StartSession(GameplayStartRequest request);

        /// <summary>
        /// 指定された側の数字変更を入力します。
        /// </summary>
        /// <param name="side">変更対象の左右。</param>
        void ChangeNumber(PlayerSide side);

        /// <summary>
        /// 演算子変更を入力します。
        /// </summary>
        void ChangeOperator();

        /// <summary>
        /// 指定されたMissionの現在式を提出します。
        /// </summary>
        /// <param name="missionId">提出対象のMissionId。</param>
        void Submit(MissionId missionId);

        /// <summary>
        /// 指定されたMissionのフィードバック演出完了を入力します。
        /// </summary>
        /// <param name="missionId">演出対象のMissionId。</param>
        void NotifyFeedbackCompleted(MissionId missionId);
    }
}
