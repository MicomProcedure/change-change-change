#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Microsoft.Extensions.Logging;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 現在式による候補群の確率選択、Tier、極性、直前Mission、Kind、適合率を順に考慮する本実装版Mission抽選Strategyです。
    /// </summary>
    public sealed class FullMissionDrawStrategy : IMissionDrawStrategy
    {
        private const int LoggedCandidateCount = 10;

        private readonly MissionDrawSelectionModule _selectionModule;
        private readonly ILogger<FullMissionDrawStrategy> _logger;

        /// <summary>
        /// 乱数提供元とLoggerから本実装版Strategyを生成します。
        /// </summary>
        /// <param name="randomSource">0以上1未満の乱数提供元。</param>
        /// <param name="logger">抽選診断を出力するLogger。</param>
        /// <exception cref="ArgumentNullException">乱数提供元またはLoggerがnullの場合。</exception>
        public FullMissionDrawStrategy(IRandomSource randomSource, ILogger<FullMissionDrawStrategy> logger)
        {
            _selectionModule = new MissionDrawSelectionModule(
                randomSource ?? throw new ArgumentNullException(nameof(randomSource)));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 抽選Contextに基づいて本実装版の規則でMission候補を1件選出します。
        /// </summary>
        /// <param name="context">抽選Context。</param>
        /// <returns>抽選結果。</returns>
        public MissionDrawResult Draw(MissionDrawContext context)
        {
            if (context == null)
            {
                return MissionDrawResult.Failure("Mission draw context is null.");
            }

            try
            {
                GameplayConfig config = context.SessionContext.Config;
                IReadOnlyList<MissionCandidate> catalogCandidates =
                    context.SessionContext.MissionCatalog.Candidates;
                if (catalogCandidates == null || catalogCandidates.Count == 0)
                {
                    return MissionDrawResult.Failure("Mission catalog contains no candidates.");
                }

                MissionCandidate[] enabledCandidates = catalogCandidates
                    .Where(candidate =>
                        !candidate.Condition.IsNegated ||
                        config.NegatedMissionSelectionWeight > 0f)
                    .ToArray();
                if (enabledCandidates.Length == 0)
                {
                    return MissionDrawResult.Failure(
                        "Negated mission selection is disabled and no positive candidates are available.");
                }

                if (!TrySelectCurrentExpressionCandidates(
                        context,
                        enabledCandidates,
                        out IReadOnlyList<MissionCandidate> currentExpressionCandidates,
                        out string currentExpressionFailure))
                {
                    return MissionDrawResult.Failure(currentExpressionFailure);
                }

                if (!_selectionModule.TrySelectTierCandidates(
                        currentExpressionCandidates,
                        context.Section,
                        config,
                        shouldSelectSpecificLowerTierWhenCurrentTierIsUnavailable: true,
                        out IReadOnlyList<MissionCandidate> selectedTierCandidates,
                        out string tierFailure))
                {
                    return MissionDrawResult.Failure(tierFailure);
                }

                if (!_selectionModule.TrySelectConditionPolarityCandidates(
                        selectedTierCandidates,
                        config.NegatedMissionSelectionWeight,
                        out IReadOnlyList<MissionCandidate> selectedPolarityCandidates,
                        out string polarityFailure))
                {
                    return MissionDrawResult.Failure(polarityFailure);
                }

                IReadOnlyList<MissionCandidate> repeatFilteredCandidates =
                    ExcludePreviousConditionWhenAlternativeExists(
                        context,
                        selectedPolarityCandidates);
                if (!_selectionModule.TrySelectKindCandidates(
                        repeatFilteredCandidates,
                        out IReadOnlyList<MissionCandidate> selectedKindCandidates,
                        out string kindFailure))
                {
                    return MissionDrawResult.Failure(kindFailure);
                }

                IReadOnlyList<MissionCandidate> rankedCandidates = RankCandidatesByTargetMatchRatio(
                    selectedKindCandidates,
                    config.TargetExpressionMatchRatio);
                IReadOnlyList<MissionCandidate> closestCandidates = rankedCandidates
                    .Take(config.MissionParameterSelectionPoolSize)
                    .ToArray();

                if (!_selectionModule.TrySelectCandidate(
                        closestCandidates,
                        "target match ratio candidate",
                        out MissionCandidate selectedCandidate,
                        out string candidateFailure))
                {
                    return MissionDrawResult.Failure(candidateFailure);
                }

                MissionDrawResult result = MissionDrawResult.Success(selectedCandidate);
                LogRankedCandidates(context, rankedCandidates, selectedCandidate);
                return result;
            }
            catch (Exception exception)
            {
                return MissionDrawResult.Failure(
                    $"Full mission draw failed because the context is invalid: {exception.Message}");
            }
        }

        private bool TrySelectCurrentExpressionCandidates(
            MissionDrawContext context,
            IReadOnlyList<MissionCandidate> enabledCandidates,
            out IReadOnlyList<MissionCandidate> selectedCandidates,
            out string failureMessage)
        {
            selectedCandidates = Array.Empty<MissionCandidate>();
            failureMessage = null;
            int currentTier = TierResolver.ResolveCurrentTier(
                context.Section,
                context.SessionContext.Config);
            MissionCandidate[] unlockedCandidates = enabledCandidates
                .Where(candidate => candidate.Tier <= currentTier)
                .ToArray();
            if (context.IsInitialDraw)
            {
                selectedCandidates = unlockedCandidates;
                return true;
            }

            Expression currentExpression = context.CurrentExpression
                ?? throw new InvalidOperationException(
                    "A next-mission draw context must contain the current expression.");
            CalculationResult currentResult = ExpressionCalculator.Evaluate(currentExpression);
            var satisfiedCandidates = new List<MissionCandidate>();
            var unmatchedCandidates = new List<MissionCandidate>();
            for (int index = 0; index < unlockedCandidates.Length; index++)
            {
                MissionCandidate candidate = unlockedCandidates[index];
                if (MissionConditionMatcher.IsMatch(candidate.Condition, currentResult))
                {
                    satisfiedCandidates.Add(candidate);
                }
                else
                {
                    unmatchedCandidates.Add(candidate);
                }
            }

            return _selectionModule.TrySelectCandidateGroupByRatio(
                satisfiedCandidates,
                unmatchedCandidates,
                context.SessionContext.Config.CurrentExpressionSatisfiedMissionSelectionRatio,
                "current expression satisfaction",
                out selectedCandidates,
                out failureMessage);
        }

        private static IReadOnlyList<MissionCandidate> ExcludePreviousConditionWhenAlternativeExists(
            MissionDrawContext context,
            IReadOnlyList<MissionCandidate> candidates)
        {
            if (context.IsInitialDraw)
            {
                return candidates;
            }

            Mission previousMission = context.PreviousMission
                ?? throw new InvalidOperationException(
                    "A next-mission draw context must contain the previous mission.");
            MissionCandidate[] alternatives = candidates
                .Where(candidate => candidate.Condition != previousMission.Condition)
                .ToArray();
            return alternatives.Length > 0 ? alternatives : candidates;
        }

        private static IReadOnlyList<MissionCandidate> RankCandidatesByTargetMatchRatio(
            IReadOnlyList<MissionCandidate> candidates,
            float targetExpressionMatchRatio)
        {
            return candidates
                .Select((candidate, catalogOrder) => new
                {
                    Candidate = candidate,
                    CatalogOrder = catalogOrder,
                    Difference = Math.Abs(
                        candidate.ExpressionMatchRatio - targetExpressionMatchRatio)
                })
                .OrderBy(entry => entry.Difference)
                .ThenBy(entry => entry.CatalogOrder)
                .Select(entry => entry.Candidate)
                .ToArray();
        }

        private void LogRankedCandidates(
            MissionDrawContext context,
            IReadOnlyList<MissionCandidate> rankedCandidates,
            MissionCandidate selectedCandidate)
        {
            try
            {
                if (!_logger.IsEnabled(LogLevel.Debug))
                {
                    return;
                }

                GameplayConfig config = context.SessionContext.Config;
                int poolSize = Math.Min(config.MissionParameterSelectionPoolSize, rankedCandidates.Count);
                int displayedCount = Math.Min(LoggedCandidateCount, rankedCandidates.Count);
                int selectedRank = -1;
                for (int index = 0; index < rankedCandidates.Count; index++)
                {
                    if (ReferenceEquals(rankedCandidates[index], selectedCandidate))
                    {
                        selectedRank = index + 1;
                        break;
                    }
                }

                var message = new StringBuilder();
                message.Append("Mission draw ranking: Section=").Append(context.Section)
                    .Append(", Tier=").Append(selectedCandidate.Tier)
                    .Append(", Kind=").Append(selectedCandidate.Condition.Kind)
                    .Append(", Negated=").Append(selectedCandidate.Condition.IsNegated)
                    .Append(", SelectedRank=").Append(selectedRank)
                    .Append(", SelectedParameterA=").Append(selectedCandidate.Condition.ParameterA)
                    .Append(", SelectedParameterB=").Append(selectedCandidate.Condition.ParameterB)
                    .Append(", TargetRatio=").Append(
                        config.TargetExpressionMatchRatio.ToString("0.0000", CultureInfo.InvariantCulture))
                    .Append(", PoolSize=").Append(poolSize)
                    .Append(", Candidates=").Append(rankedCandidates.Count);

                for (int index = 0; index < displayedCount; index++)
                {
                    MissionCandidate candidate = rankedCandidates[index];
                    MissionCondition condition = candidate.Condition;
                    float difference = Math.Abs(
                        candidate.ExpressionMatchRatio - config.TargetExpressionMatchRatio);
                    message.AppendLine()
                        .Append('#').Append(index + 1)
                        .Append(" Kind=").Append(condition.Kind)
                        .Append(" ParameterA=").Append(condition.ParameterA)
                        .Append(" ParameterB=").Append(condition.ParameterB)
                        .Append(" Negated=").Append(condition.IsNegated)
                        .Append(" Match=").Append(candidate.ExpressionMatchCount)
                        .Append('/').Append(candidate.EvaluatedExpressionCount)
                        .Append(" Ratio=").Append(
                            candidate.ExpressionMatchRatio.ToString("0.0000", CultureInfo.InvariantCulture))
                        .Append(" Difference=").Append(difference.ToString("0.0000", CultureInfo.InvariantCulture));

                    if (index < poolSize)
                    {
                        message.Append(" [Pool]");
                    }

                    if (ReferenceEquals(candidate, selectedCandidate))
                    {
                        message.Append(" [Selected]");
                    }
                }

                _logger.LogDebug("{MissionDrawRanking}", message.ToString());
            }
            catch (Exception)
            {
                // 診断ログの失敗をMission抽選の失敗に変えない。
            }
        }
    }
}
