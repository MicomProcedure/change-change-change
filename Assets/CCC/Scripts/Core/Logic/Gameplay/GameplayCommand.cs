using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// ゲームプレイの状態遷移を要求する基底 Command です。
    /// </summary>
    public abstract class GameplayCommand
    {
    }

    /// <summary>
    /// プレイヤーが現在の式を提出する Command です。
    /// </summary>
    public sealed class SubmitCommand : GameplayCommand
    {
        /// <summary>
        /// 提出対象のミッション識別子を取得します。
        /// </summary>
        public MissionId MissionId { get; }

        /// <summary>
        /// 入力時点で固定された提出式の snapshot を取得します。
        /// </summary>
        public Expression SubmittedExpression { get; }

        /// <summary>
        /// 提出時点の残り時間を取得します。
        /// </summary>
        public TimeSpan RemainingTime { get; }


        /// <summary>
        /// ミッション識別子、提出式の snapshot、提出時点の残り時間から Command を生成します。
        /// </summary>
        /// <param name="missionId">提出対象のミッション識別子。</param>
        /// <param name="submittedExpression">入力時点で固定された提出式。</param>
        /// <param name="remainingTime">提出時点の残り時間。</param>
        public SubmitCommand(MissionId missionId, Expression submittedExpression, TimeSpan remainingTime)
        {
            MissionId = missionId;
            SubmittedExpression = submittedExpression;
            RemainingTime = remainingTime;
        }
    }

    /// <summary>
    /// 制限時間切れによる現在の式の提出を表す Command です。
    /// </summary>
    public sealed class TimeExpiredCommand : GameplayCommand
    {
        /// <summary>
        /// 提出対象のミッション識別子を取得します。
        /// </summary>
        public MissionId MissionId { get; }

        /// <summary>
        /// 時間切れ時点で固定された提出式の snapshot を取得します。
        /// </summary>
        public Expression SubmittedExpression { get; }

        /// <summary>
        /// ミッション識別子と提出式の snapshot から Command を生成します。
        /// </summary>
        /// <param name="missionId">提出対象のミッション識別子。</param>
        /// <param name="submittedExpression">時間切れ時点で固定された提出式。</param>
        public TimeExpiredCommand(MissionId missionId, Expression submittedExpression)
        {
            MissionId = missionId;
            SubmittedExpression = submittedExpression;
        }
    }

    /// <summary>
    /// 提出結果のフィードバック完了を通知する Command です。
    /// </summary>
    public sealed class FeedbackCompletedCommand : GameplayCommand
    {
        /// <summary>
        /// フィードバック対象のミッション識別子を取得します。
        /// </summary>
        public MissionId MissionId { get; }

        /// <summary>
        /// ミッション識別子から Command を生成します。
        /// </summary>
        /// <param name="missionId">フィードバック対象のミッション識別子。</param>
        public FeedbackCompletedCommand(MissionId missionId)
        {
            MissionId = missionId;
        }
    }

    /// <summary>
    /// 次のミッション候補を提示する Command です。
    /// </summary>
    public sealed class PresentMissionCommand : GameplayCommand
    {
        /// <summary>
        /// 現在のミッション識別子を取得します。
        /// </summary>
        public MissionId PreviousMissionId { get; }

        /// <summary>
        /// 新しいミッションの候補を取得します。
        /// </summary>
        public MissionCandidate Candidate { get; }

        /// <summary>
        /// 現在のミッション識別子と候補から Command を生成します。
        /// </summary>
        /// <param name="previousMissionId">現在のミッション識別子。</param>
        /// <param name="candidate">新しいミッション候補。</param>
        /// <exception cref="ArgumentNullException"><paramref name="candidate"/> が null の場合。</exception>
        public PresentMissionCommand(MissionId previousMissionId, MissionCandidate candidate)
        {
            PreviousMissionId = previousMissionId;
            Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
        }
    }
}
