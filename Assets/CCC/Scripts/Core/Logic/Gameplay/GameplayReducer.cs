using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// Gameplay Command を決定的に評価し、状態遷移と Event を生成する状態なし Reducer です。
    /// </summary>
    public static class GameplayReducer
    {
        /// <summary>
        /// 現在の状態と Command をセッション遷移へ還元します。
        /// </summary>
        /// <param name="context">セッション設定とミッションカタログ。</param>
        /// <param name="currentState">還元前の状態。</param>
        /// <param name="command">還元対象の Command。</param>
        /// <returns>次状態、Event、受理結果を含む遷移。</returns>
        /// <exception cref="ArgumentNullException">引数のいずれかが null の場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">未対応の Command 派生型の場合。</exception>
        public static GameplayTransition Reduce(
            GameplaySessionContext context,
            GameplayState currentState,
            GameplayCommand command)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (currentState == null)
            {
                throw new ArgumentNullException(nameof(currentState));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            switch (command)
            {
                case SubmitCommand submitCommand:
                    return ReduceSubmission(
                        context,
                        currentState,
                        submitCommand.MissionId,
                        submitCommand.SubmittedExpression,
                        SubmissionTrigger.ManualSubmit,
                        submitCommand.RemainingTime);

                case TimeExpiredCommand timeExpiredCommand:
                    return ReduceSubmission(
                        context,
                        currentState,
                        timeExpiredCommand.MissionId,
                        timeExpiredCommand.SubmittedExpression,
                        SubmissionTrigger.TimeExpired,
                        TimeSpan.Zero);

                case FeedbackCompletedCommand feedbackCompletedCommand:
                    return ReduceFeedbackCompleted(
                        currentState,
                        feedbackCompletedCommand.MissionId);

                case PresentMissionCommand presentMissionCommand:
                    return ReducePresentMission(
                        context,
                        currentState,
                        presentMissionCommand.PreviousMissionId,
                        presentMissionCommand.Candidate);

                default:
                    throw new ArgumentOutOfRangeException(nameof(command));
            }
        }

        private static GameplayTransition ReduceSubmission(
            GameplaySessionContext context,
            GameplayState currentState,
            MissionId missionId,
            Expression submittedExpression,
            SubmissionTrigger trigger,
            TimeSpan remainingTime)
        {
            if (currentState.Status != GameplayStatus.Playing ||
                currentState.CurrentMission.Id != missionId)
            {
                return GameplayTransition.Ignored(currentState);
            }

            var calculationResult = ExpressionCalculator.Evaluate(submittedExpression);
            var isSuccess = calculationResult.IsDefined &&
                            MissionConditionMatcher.IsMatch(
                                currentState.CurrentMission.Condition,
                                calculationResult);
            var resolution = new SubmissionResolution(
                missionId,
                trigger,
                submittedExpression,
                isSuccess,
                calculationResult);

            int remainingLife = isSuccess
                ? currentState.Life
                : Math.Max(0, currentState.Life - 1);
            int nextSection = isSuccess
                ? checked(currentState.Section + 1)
                : currentState.Section;
            TimeSpan pendingBonusDuration = BonusTimeCalculator.CalculateNextBonusDuration(
                context.Config.Timer,
                currentState.Section,
                remainingTime,
                isSuccess,
                trigger);
            var nextState = new GameplayState(
                GameplayStatus.Transitioning,
                nextSection,
                remainingLife,
                currentState.CurrentMission,
                resolution,
                pendingBonusDuration);
            var missionCandidate = FindMissionCandidate(
                context,
                currentState.CurrentMission);
            var resolvedEvent = new SubmissionResolvedEvent(
                resolution,
                missionCandidate.AnswerExample,
                remainingLife);

            return GameplayTransition.Accepted(nextState, resolvedEvent);
        }

        private static GameplayTransition ReduceFeedbackCompleted(
            GameplayState currentState,
            MissionId missionId)
        {
            if (currentState.Status != GameplayStatus.Transitioning ||
                currentState.CurrentMission.Id != missionId)
            {
                return GameplayTransition.Ignored(currentState);
            }

            if (currentState.Life > 0)
            {
                return GameplayTransition.Accepted(currentState);
            }

            var lastResolution = currentState.LastResolution;
            if (lastResolution == null)
            {
                throw new InvalidOperationException(
                    "A transitioning state must contain a last resolution.");
            }

            var nextState = new GameplayState(
                GameplayStatus.GameOver,
                currentState.Section,
                0,
                currentState.CurrentMission,
                lastResolution);
            var sessionResult = new SessionResult(
                SessionTerminationReason.LifeDepleted,
                currentState.Section,
                currentState.CurrentMission,
                lastResolution);
            var endedEvent = new SessionEndedEvent(sessionResult);

            return GameplayTransition.Accepted(nextState, endedEvent);
        }

        private static GameplayTransition ReducePresentMission(
            GameplaySessionContext context,
            GameplayState currentState,
            MissionId previousMissionId,
            MissionCandidate candidate)
        {
            if (currentState.Status != GameplayStatus.Transitioning ||
                currentState.CurrentMission.Id != previousMissionId ||
                currentState.Life <= 0)
            {
                return GameplayTransition.Ignored(currentState);
            }

            var nextMissionId = CreateNextMissionId(previousMissionId);
            var mission = new Mission(
                nextMissionId,
                candidate.Condition,
                candidate.Tier);
            var nextState = new GameplayState(
                GameplayStatus.Playing,
                currentState.Section,
                currentState.Life,
                mission,
                currentState.LastResolution);
            var duration = context.Config.Timer.GetDuration(nextState.Section) +
                           currentState.PendingBonusDuration;
            var presentedEvent = new MissionPresentedEvent(mission, duration);

            return GameplayTransition.Accepted(nextState, presentedEvent);
        }

        private static MissionId CreateNextMissionId(MissionId previousMissionId)
        {
            checked
            {
                return new MissionId(previousMissionId.Value + 1);
            }
        }

        private static MissionCandidate FindMissionCandidate(
            GameplaySessionContext context,
            Mission mission)
        {
            var candidates = context.MissionCatalog.Candidates;
            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate.Condition == mission.Condition && candidate.Tier == mission.Tier)
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException(
                "The current mission does not have a matching candidate in the mission catalog.");
        }
    }
}