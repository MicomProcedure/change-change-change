#nullable enable
using System;
using System.Collections.Generic;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Logging;

namespace Ccc.Tests
{
    /// <summary>
    /// 診断出力のテストで共有するMission抽選TraceとSession識別情報を生成します。
    /// </summary>
    internal static class MissionDrawTraceTestFactory
    {
        private const int CandidateTier = 2;
        private const int EvaluatedExpressionCount = 10;

        /// <summary>
        /// 範囲0.10〜0.20から選んだ目標式適合割合0.15に対して差の昇順に並ぶ3候補を持ち、2位の候補を選出したTraceを生成します。
        /// </summary>
        /// <param name="poolSize">順位先頭から抽選対象にした候補数。</param>
        /// <returns>テスト用の抽選Trace。</returns>
        internal static MissionDrawTrace CreateTrace(int poolSize = 2)
        {
            MissionCandidate[] rankedCandidates =
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 1), matchCount: 2),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 2), matchCount: 3),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 3), matchCount: 5)
            };
            return new MissionDrawTrace(
                section: 5,
                isInitialDraw: false,
                currentTier: 2,
                config: CreateConfig(),
                enabledCandidateCount: 40,
                unlockedCandidateCount: 30,
                satisfiedCandidateCount: 12,
                unmatchedCandidateCount: 18,
                selectedExpressionGroup: CurrentExpressionCandidateGroup.Unmatched,
                tierStageCandidateCount: 10,
                isNegatedPolaritySelected: false,
                polarityStageCandidateCount: 6,
                repeatFilteredCandidateCount: 5,
                selectedKind: MissionKind.Exact,
                targetExpressionMatchRatioRange: new TargetExpressionMatchRatioRange(0.10f, 0.20f),
                targetExpressionMatchRatio: 0.15f,
                rankedCandidates: rankedCandidates,
                poolSize: poolSize,
                selectedCandidate: rankedCandidates[1]);
        }

        /// <summary>
        /// 肯定のExact候補、反転のExact候補、2パラメータのBetween候補をこの順に持つCatalogを生成します。
        /// </summary>
        /// <returns>テスト用のMissionCatalog。</returns>
        internal static MissionCatalog CreateCatalog()
        {
            return new MissionCatalog(new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 1), matchCount: 2),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 2, isNegated: true), matchCount: 7),
                CreateCandidate(MissionCondition.Between(3, 4), matchCount: 5)
            });
        }

        /// <summary>
        /// テスト用のSession識別情報を生成します。
        /// </summary>
        /// <returns>テスト用のSession識別情報。</returns>
        internal static DiagnosticsSessionDescriptor CreateSession()
        {
            return new DiagnosticsSessionDescriptor(
                "20261005_143012",
                "LifeEndless, Normal",
                GameMode.LifeEndless,
                GameDifficulty.Normal);
        }

        private static GameplayConfig CreateConfig()
        {
            return new GameplayConfig(
                minimumNumber: 1,
                maximumNumber: 5,
                initialExpression: new Expression(1, OperatorType.Add, 1),
                initialLife: 3,
                timer: new TimerConfig(
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(0.5),
                    TimeSpan.FromSeconds(3)),
                tierProgression: new TierProgression(new List<int> { 1, 4, 8, 12 }),
                showCurrentAnswer: false,
                minimumExpressionMatchRatio: 0.10f,
                maximumExpressionMatchRatio: 0.60f,
                minimumExpressionMatchCount: 2,
                zeroAndPrimeMinimumExpressionMatchCount: 1,
                currentTierDrawWeight: 0.60f,
                lowerTierDrawWeight: 0.40f,
                currentExpressionSatisfiedMissionSelectionRatio: 0.10f,
                targetExpressionMatchRatio: 0.20f,
                targetExpressionMatchRatioReductionPerSection: 0.025f,
                minimumTargetExpressionMatchRatio: 0.05f,
                negatedMissionSelectionWeight: 0.60f,
                missionParameterSelectionPoolSize: 3);
        }

        private static MissionCandidate CreateCandidate(MissionCondition condition, int matchCount)
        {
            var answerExpression = new Expression(1, OperatorType.Add, 1);
            return new MissionCandidate(
                condition,
                CandidateTier,
                matchCount,
                EvaluatedExpressionCount,
                new ExpressionCandidate(
                    answerExpression,
                    ExpressionCalculator.Evaluate(answerExpression)));
        }
    }
}
