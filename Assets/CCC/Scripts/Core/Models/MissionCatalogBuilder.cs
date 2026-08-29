using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// ExpressionCandidate と MissionParameterCandidate から条件適合を分析し、
    /// カタログ成立基準を通過した候補で MissionCatalog を構築するビルダー。
    /// </summary>
    public static class MissionCatalogBuilder
    {
        public static MissionCatalogBuildResult Build(
            IReadOnlyList<ExpressionCandidate> expressions,
            IReadOnlyList<MissionParameterCandidate> parameters,
            GameplayConfig config)
        {
            if (expressions == null) throw new ArgumentNullException(nameof(expressions));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (config == null) throw new ArgumentNullException(nameof(config));

            // 同一条件・Tierの候補は最初の1件へ正規化する
            IReadOnlyList<MissionParameterCandidate> normalizedParameters =
                NormalizeParameters(parameters);

            // 0除算を除いた正常式候補のみを分母とする
            var definedExpressions = expressions.Where(e => e.Result.IsDefined).ToList();
            if (definedExpressions.Count == 0)
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

            int evaluatedCount = definedExpressions.Count;
            var acceptedCandidates = new List<MissionCandidate>();

            Rational minimumMatchRatio = Rational.FromSingle(
                config.MinimumExpressionMatchRatio);
            Rational maximumMatchRatio = Rational.FromSingle(
                config.MaximumExpressionMatchRatio);

            foreach (var param in normalizedParameters)
            {
                int matchCount = 0;
                ExpressionCandidate? firstMatch = null;

                foreach (var expr in definedExpressions)
                {
                    if (MissionConditionMatcher.IsMatch(param.Condition, expr.Result))
                    {
                        matchCount++;
                        if (!firstMatch.HasValue)
                        {
                            firstMatch = expr;
                        }
                    }
                }

                int nonMatchCount = evaluatedCount - matchCount;
                Rational matchRatio = Rational.Create(matchCount, evaluatedCount);
                int requiredMinMatchCount = config.GetMinimumExpressionMatchCount(param.Condition.Kind);

                // 選出条件判定:
                // 1. 正常な適合式が1件以上
                // 2. 正常な非適合式が1件以上
                // 3. 式適合割合が [MinRatio, MaxRatio] の範囲内
                // 4. 適合式件数が Kindごとの必要最小件数以上
                // 5. 正常な AnswerExample を保持している
                if (matchCount >= 1 &&
                    nonMatchCount >= 1 &&
                    matchRatio >= minimumMatchRatio &&
                    matchRatio <= maximumMatchRatio &&
                    matchCount >= requiredMinMatchCount &&
                    firstMatch.HasValue)
                {
                    acceptedCandidates.Add(new MissionCandidate(
                        param.Condition,
                        param.Tier,
                        matchCount,
                        evaluatedCount,
                        firstMatch.Value));
                }
            }

            if (acceptedCandidates.Count == 0)
            {
                return MissionCatalogBuildResult.Failure(
                    MissionCatalogBuildFailureKind.EmptyCatalog,
                    "No mission candidates satisfied the catalog requirements.");
            }

            // 抽選の決定性を保証するため、Tier -> Kind -> ParameterA -> ParameterB -> IsNegated 順でソート
            var sortedCandidates = acceptedCandidates
                .OrderBy(c => c.Tier)
                .ThenBy(c => c.Condition.Kind)
                .ThenBy(c => c.Condition.ParameterA)
                .ThenBy(c => c.Condition.ParameterB)
                .ThenBy(c => c.Condition.IsNegated)
                .ToArray();

            return MissionCatalogBuildResult.Success(new MissionCatalog(sortedCandidates));
        }

        private static IReadOnlyList<MissionParameterCandidate> NormalizeParameters(
            IReadOnlyList<MissionParameterCandidate> parameters)
        {
            var seen = new HashSet<(MissionCondition, int)>();
            var normalizedParameters = new List<MissionParameterCandidate>(parameters.Count);
            for (int index = 0; index < parameters.Count; index++)
            {
                MissionParameterCandidate parameter = parameters[index];
                if (seen.Add((parameter.Condition, parameter.Tier)))
                {
                    normalizedParameters.Add(parameter);
                }
            }

            return normalizedParameters;
        }
    }
}
