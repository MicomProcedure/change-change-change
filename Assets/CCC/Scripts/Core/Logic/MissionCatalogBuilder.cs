#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// Tier未確定候補を一度だけ分析し、採用候補と確定Tierを持つMissionCatalogを構築します。
    /// </summary>
    public static class MissionCatalogBuilder
    {
        /// <summary>
        /// 式候補、Missionパラメータ候補、評価規則からCatalogを構築します。
        /// </summary>
        /// <param name="expressions">Sessionで到達可能な式候補。</param>
        /// <param name="parameters">Tier未確定Missionパラメータ候補。</param>
        /// <param name="evaluationPolicy">Catalog採用規則。</param>
        /// <param name="betweenTierTwoThreshold">Between Tier判定閾値。</param>
        /// <returns>Catalog構築結果。</returns>
        public static MissionCatalogBuildResult Build(
            IReadOnlyList<ExpressionCandidate> expressions,
            IReadOnlyList<MissionParameterCandidate> parameters,
            MissionCatalogEvaluationPolicy evaluationPolicy,
            Rational betweenTierTwoThreshold)
        {
            if (expressions == null) throw new ArgumentNullException(nameof(expressions));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (evaluationPolicy == null) throw new ArgumentNullException(nameof(evaluationPolicy));

            IReadOnlyList<MissionParameterCandidate> normalizedParameters =
                NormalizeParameters(parameters);
            ExpressionCandidate[] definedExpressions = expressions
                .Where(expression => expression.Result.IsDefined)
                .ToArray();
            if (definedExpressions.Length == 0)
            {
                return MissionCatalogBuildResult.Failure(
                    MissionCatalogBuildFailureKind.NoExpressions,
                    "No defined expression candidates provided.");
            }

            if (normalizedParameters.Count == 0)
            {
                return MissionCatalogBuildResult.Failure(
                    MissionCatalogBuildFailureKind.NoParameters,
                    "No mission parameter candidates provided.");
            }

            HashSet<int> reachableIntegers = ReachableIntegerCollector.Collect(definedExpressions);
            var acceptedCandidates = new List<MissionCandidate>();
            for (int index = 0; index < normalizedParameters.Count; index++)
            {
                MissionParameterCandidate parameter = normalizedParameters[index];
                MissionCandidateAnalysis analysis = Analyze(
                    parameter.Condition,
                    definedExpressions);
                if (!ShouldInclude(parameter.Condition.Kind, analysis, evaluationPolicy))
                {
                    continue;
                }

                int tier = MissionCandidateTierResolver.Resolve(
                    parameter.Condition,
                    analysis.MatchRatio,
                    betweenTierTwoThreshold,
                    reachableIntegers.Contains(parameter.Condition.ParameterA));
                acceptedCandidates.Add(new MissionCandidate(
                    parameter.Condition,
                    tier,
                    analysis.MatchCount,
                    analysis.EvaluatedExpressionCount,
                    analysis.FirstMatch!.Value));
            }

            if (acceptedCandidates.Count == 0)
            {
                return MissionCatalogBuildResult.Failure(
                    MissionCatalogBuildFailureKind.EmptyCatalog,
                    "No mission candidates satisfied the catalog requirements.");
            }

            MissionCandidate[] sortedCandidates = acceptedCandidates
                .OrderBy(candidate => candidate.Tier)
                .ThenBy(candidate => candidate.Condition.Kind)
                .ThenBy(candidate => candidate.Condition.ParameterA)
                .ThenBy(candidate => candidate.Condition.ParameterB)
                .ThenBy(candidate => candidate.Condition.IsNegated)
                .ToArray();
            return MissionCatalogBuildResult.Success(new MissionCatalog(sortedCandidates));
        }

        private static MissionCandidateAnalysis Analyze(
            MissionCondition condition,
            IReadOnlyList<ExpressionCandidate> definedExpressions)
        {
            int matchCount = 0;
            ExpressionCandidate? firstMatch = null;
            for (int index = 0; index < definedExpressions.Count; index++)
            {
                ExpressionCandidate expression = definedExpressions[index];
                if (!MissionConditionMatcher.IsMatch(condition, expression.Result))
                {
                    continue;
                }

                matchCount++;
                if (!firstMatch.HasValue)
                {
                    firstMatch = expression;
                }
            }

            int evaluatedExpressionCount = definedExpressions.Count;
            return new MissionCandidateAnalysis(
                matchCount,
                evaluatedExpressionCount - matchCount,
                evaluatedExpressionCount,
                Rational.Create(matchCount, evaluatedExpressionCount),
                firstMatch);
        }

        private static bool ShouldInclude(
            MissionKind kind,
            MissionCandidateAnalysis analysis,
            MissionCatalogEvaluationPolicy evaluationPolicy)
        {
            if (analysis.MatchCount < 1 ||
                analysis.NonMatchCount < 1 ||
                !analysis.FirstMatch.HasValue ||
                analysis.MatchCount < evaluationPolicy.GetMinimumMatchCount(kind))
            {
                return false;
            }

            return !evaluationPolicy.AppliesMatchRatioRange(kind) ||
                   (analysis.MatchRatio >= evaluationPolicy.MinimumMatchRatio &&
                    analysis.MatchRatio <= evaluationPolicy.MaximumMatchRatio);
        }

        private static IReadOnlyList<MissionParameterCandidate> NormalizeParameters(
            IReadOnlyList<MissionParameterCandidate> parameters)
        {
            var seen = new HashSet<MissionCondition>();
            var normalizedParameters = new List<MissionParameterCandidate>(parameters.Count);
            for (int index = 0; index < parameters.Count; index++)
            {
                MissionParameterCandidate parameter = parameters[index];
                if (seen.Add(parameter.Condition))
                {
                    normalizedParameters.Add(parameter);
                }
            }

            return normalizedParameters;
        }
    }
}
