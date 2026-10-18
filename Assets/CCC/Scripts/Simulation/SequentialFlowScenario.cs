#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;

namespace Ccc.Simulation
{
    /// <summary>
    /// 毎回正解したものとしてSectionを1ずつ進め、指定Session数ぶん実プレイの抽選の流れを再現するシナリオです。
    /// </summary>
    /// <remarks>
    /// 各SessionはSection 1の初回抽選から始まり、最大Sectionに達するか抽選に失敗すると次のSessionへ移ります。
    /// 2回目以降の現在式は直前Missionを満たす式から一様に選び、満たす式がない場合は候補式全体から選びます。
    /// </remarks>
    public sealed class SequentialFlowScenario : IMissionDrawScenario
    {
        private const int InitialSection = 1;

        private readonly MissionDrawScenarioEnvironment _environment;
        private readonly int _sessionCount;
        private readonly int _maximumSection;
        private int _startedSessionCount;

        /// <summary>
        /// 環境、Session数、最大Sectionからシナリオを生成します。
        /// </summary>
        /// <param name="environment">準備済みCatalogと候補式とシナリオ用乱数。</param>
        /// <param name="sessionCount">再現するSession数。</param>
        /// <param name="maximumSection">1Sessionで到達させる最大Section。</param>
        /// <exception cref="ArgumentNullException">環境がnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Session数または最大Sectionが1未満の場合。</exception>
        public SequentialFlowScenario(
            MissionDrawScenarioEnvironment environment,
            int sessionCount,
            int maximumSection)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            if (sessionCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionCount), sessionCount, "Session count must be at least one.");
            }

            if (maximumSection < InitialSection)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumSection),
                    maximumSection,
                    "Maximum section must be at least one.");
            }

            _sessionCount = sessionCount;
            _maximumSection = maximumSection;
        }

        /// <summary>
        /// 直前の抽選結果に応じて、同じSessionの次Sectionか、次Sessionの初回抽選のContextを生成します。
        /// </summary>
        /// <param name="previousContext">直前に抽選したContext。最初の呼び出しではnullです。</param>
        /// <param name="previousResult">直前の抽選結果。最初の呼び出しではnullです。</param>
        /// <param name="context">次に抽選するContext。</param>
        /// <returns>次の抽選がある場合はtrue、全Sessionを終えた場合はfalse。</returns>
        public bool TryCreateNextContext(
            MissionDrawContext? previousContext,
            MissionDrawResult? previousResult,
            out MissionDrawContext context)
        {
            bool canContinueSession = previousContext != null &&
                previousResult != null &&
                previousResult.IsSuccess &&
                previousContext.Section < _maximumSection;
            if (canContinueSession)
            {
                int nextSection = previousContext!.Section + 1;
                MissionCandidate previousCandidate = previousResult!.Candidate;
                context = MissionDrawContext.CreateForNextMission(
                    _environment.SessionContext,
                    nextSection,
                    PickExpressionSatisfying(previousCandidate.Condition),
                    MissionDrawScenarioEnvironment.CreateMission(previousCandidate, previousContext.Section));
                return true;
            }

            if (_startedSessionCount >= _sessionCount)
            {
                context = null!;
                return false;
            }

            _startedSessionCount++;
            context = MissionDrawContext.CreateForInitialMission(_environment.SessionContext, InitialSection);
            return true;
        }

        private Expression PickExpressionSatisfying(MissionCondition condition)
        {
            ExpressionCandidate[] satisfyingCandidates = _environment.DefinedExpressionCandidates
                .Where(candidate => MissionConditionMatcher.IsMatch(condition, candidate.Result))
                .ToArray();
            if (satisfyingCandidates.Length == 0)
            {
                return _environment.PickExpression();
            }

            return satisfyingCandidates[_environment.Random.NextIndex(satisfyingCandidates.Length)].Expression;
        }
    }
}
