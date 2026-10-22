#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 現在式による候補群の確率選択、Tier、極性、直前Mission、Kind、適合率を順に考慮する本実装版Mission抽選Strategyです。
    /// </summary>
    public sealed class FullMissionDrawStrategy : IMissionDrawStrategy
    {
        private readonly MissionDrawSelectionModule _selectionModule;
        private readonly IDiagnosticsReporter<MissionDrawTrace> _diagnosticsReporter;

        /// <summary>
        /// 乱数提供元と診断Reporterから本実装版Strategyを生成します。
        /// </summary>
        /// <param name="randomSource">0以上1未満の乱数提供元。</param>
        /// <param name="diagnosticsReporter">抽選成功時のTraceを受け取る診断Reporter。</param>
        /// <exception cref="ArgumentNullException">乱数提供元または診断Reporterがnullの場合。</exception>
        public FullMissionDrawStrategy(
            IRandomSource randomSource,
            IDiagnosticsReporter<MissionDrawTrace> diagnosticsReporter)
        {
            _selectionModule = new MissionDrawSelectionModule(
                randomSource ?? throw new ArgumentNullException(nameof(randomSource)));
            _diagnosticsReporter = diagnosticsReporter ??
                throw new ArgumentNullException(nameof(diagnosticsReporter));
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

                int currentTier = TierResolver.ResolveCurrentTier(context.Section, config);
                if (!TrySelectCurrentExpressionCandidates(
                        context,
                        enabledCandidates,
                        currentTier,
                        out CurrentExpressionSelection expressionSelection,
                        out string currentExpressionFailure))
                {
                    return MissionDrawResult.Failure(currentExpressionFailure);
                }

                if (!_selectionModule.TrySelectTierCandidates(
                        expressionSelection.SelectedCandidates,
                        context.Section,
                        config,
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

                if (_diagnosticsReporter.IsEnabled)
                {
                    _diagnosticsReporter.Report(new MissionDrawTrace(
                        section: context.Section,
                        isInitialDraw: context.IsInitialDraw,
                        currentTier: currentTier,
                        config: config,
                        enabledCandidateCount: enabledCandidates.Length,
                        unlockedCandidateCount: expressionSelection.UnlockedCandidateCount,
                        satisfiedCandidateCount: expressionSelection.SatisfiedCandidateCount,
                        unmatchedCandidateCount: expressionSelection.UnmatchedCandidateCount,
                        selectedExpressionGroup: expressionSelection.SelectedGroup,
                        tierStageCandidateCount: selectedTierCandidates.Count,
                        isNegatedPolaritySelected: selectedPolarityCandidates[0].Condition.IsNegated,
                        polarityStageCandidateCount: selectedPolarityCandidates.Count,
                        repeatFilteredCandidateCount: repeatFilteredCandidates.Count,
                        selectedKind: selectedKindCandidates[0].Condition.Kind,
                        rankedCandidates: rankedCandidates,
                        poolSize: closestCandidates.Count,
                        selectedCandidate: selectedCandidate));
                }

                return MissionDrawResult.Success(selectedCandidate);
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
            int currentTier,
            out CurrentExpressionSelection selection,
            out string failureMessage)
        {
            selection = default;
            failureMessage = null;
            MissionCandidate[] unlockedCandidates = enabledCandidates
                .Where(candidate => candidate.Tier <= currentTier)
                .ToArray();
            if (context.IsInitialDraw)
            {
                selection = new CurrentExpressionSelection(
                    unlockedCandidates,
                    unlockedCandidates.Length,
                    satisfiedCandidateCount: 0,
                    unmatchedCandidateCount: 0,
                    CurrentExpressionCandidateGroup.AllUnlocked);
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

            if (!_selectionModule.TrySelectCandidateGroupByRatio(
                    satisfiedCandidates,
                    unmatchedCandidates,
                    context.SessionContext.Config.CurrentExpressionSatisfiedMissionSelectionRatio,
                    "current expression satisfaction",
                    out IReadOnlyList<MissionCandidate> selectedCandidates,
                    out failureMessage))
            {
                return false;
            }

            selection = new CurrentExpressionSelection(
                selectedCandidates,
                unlockedCandidates.Length,
                satisfiedCandidates.Count,
                unmatchedCandidates.Count,
                ReferenceEquals(selectedCandidates, satisfiedCandidates)
                    ? CurrentExpressionCandidateGroup.Satisfied
                    : CurrentExpressionCandidateGroup.Unmatched);
            return true;
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

        private readonly struct CurrentExpressionSelection
        {
            public CurrentExpressionSelection(
                IReadOnlyList<MissionCandidate> selectedCandidates,
                int unlockedCandidateCount,
                int satisfiedCandidateCount,
                int unmatchedCandidateCount,
                CurrentExpressionCandidateGroup selectedGroup)
            {
                SelectedCandidates = selectedCandidates;
                UnlockedCandidateCount = unlockedCandidateCount;
                SatisfiedCandidateCount = satisfiedCandidateCount;
                UnmatchedCandidateCount = unmatchedCandidateCount;
                SelectedGroup = selectedGroup;
            }

            public IReadOnlyList<MissionCandidate> SelectedCandidates { get; }

            public int UnlockedCandidateCount { get; }

            public int SatisfiedCandidateCount { get; }

            public int UnmatchedCandidateCount { get; }

            public CurrentExpressionCandidateGroup SelectedGroup { get; }
        }
    }
}
