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

            // 入力パラメータの重複・不正チェック
            ValidateParameters(parameters);

            // 0除算を除いた正常式候補のみを分母とする
            var definedExpressions = expressions.Where(e => e.Result.IsDefined).ToList();
            if (definedExpressions.Count == 0)
            {
                return MissionCatalogBuildResult.Failure(
                    MissionCatalogBuildFailureKind.NoExpressions,
                    "No defined expression candidates provided.");
            }

            if (parameters.Count == 0)
            {
                return MissionCatalogBuildResult.Failure(
                    MissionCatalogBuildFailureKind.NoParameters,
                    "No mission parameter candidates provided.");
            }

            int evaluatedCount = definedExpressions.Count;
            var acceptedCandidates = new List<MissionCandidate>();

            foreach (var param in parameters)
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
                float matchRatio = (float)matchCount / evaluatedCount;
                int requiredMinMatchCount = config.GetMinimumExpressionMatchCount(param.Condition.Kind);

                // 選出条件判定:
                // 1. 正常な適合式が1件以上
                // 2. 正常な非適合式が1件以上
                // 3. 式適合割合が [MinRatio, MaxRatio] の範囲内
                // 4. 適合式件数が Kindごとの必要最小件数以上
                // 5. 正常な AnswerExample を保持している
                if (matchCount >= 1 &&
                    nonMatchCount >= 1 &&
                    matchRatio >= config.MinimumExpressionMatchRatio &&
                    matchRatio <= config.MaximumExpressionMatchRatio &&
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

        private static void ValidateParameters(IReadOnlyList<MissionParameterCandidate> parameters)
        {
            var seen = new HashSet<(MissionCondition, int)>();
            for (int i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                var key = (p.Condition, p.Tier);
                if (!seen.Add(key))
                {
                    throw new ArgumentException($"Duplicate parameter candidate: '{p.Condition.Kind}' in Tier {p.Tier}.", nameof(parameters));
                }
            }
        }
    }
}
