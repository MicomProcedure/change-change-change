using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Mission抽選Strategy間で共有する段階別の候補選定を担当します。
    /// </summary>
    internal sealed class MissionDrawSelectionModule
    {
        private const float PositiveMissionSelectionWeight = 1f;

        private readonly IRandomSource _randomSource;

        /// <summary>
        /// 乱数提供元から候補選定モジュールを生成します。
        /// </summary>
        /// <param name="randomSource">0以上1未満の乱数提供元。</param>
        /// <exception cref="ArgumentNullException">乱数提供元がnullの場合。</exception>
        internal MissionDrawSelectionModule(IRandomSource randomSource)
        {
            _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        }

        /// <summary>
        /// SectionとConfigに基づいて抽選対象Tierの候補群を選びます。
        /// </summary>
        internal bool TrySelectTierCandidates(
            IReadOnlyList<MissionCandidate> candidates,
            int section,
            GameplayConfig config,
            bool shouldSelectSpecificLowerTierWhenCurrentTierIsUnavailable,
            out IReadOnlyList<MissionCandidate> selectedCandidates,
            out string failureMessage)
        {
            selectedCandidates = null;
            failureMessage = null;
            int currentTier = TierResolver.ResolveCurrentTier(section, config);
            var currentTierCandidates = candidates
                .Where(candidate => candidate.Tier == currentTier)
                .ToArray();
            var lowerTierCandidates = candidates
                .Where(candidate => candidate.Tier < currentTier)
                .ToArray();

            if (currentTierCandidates.Length == 0 && lowerTierCandidates.Length == 0)
            {
                failureMessage =
                    $"No mission candidates are available for tier {currentTier} at section {section}.";
                return false;
            }

            if (currentTierCandidates.Length == 0)
            {
                if (!shouldSelectSpecificLowerTierWhenCurrentTierIsUnavailable)
                {
                    selectedCandidates = lowerTierCandidates;
                    return true;
                }

                return TrySelectLowerTierCandidates(
                    lowerTierCandidates,
                    out selectedCandidates,
                    out failureMessage);
            }

            if (lowerTierCandidates.Length == 0)
            {
                selectedCandidates = currentTierCandidates;
                return true;
            }

            if (!TrySelectWeightedFirst(
                    config.CurrentTierDrawWeight,
                    config.LowerTierDrawWeight,
                    out bool selectCurrentTier,
                    out failureMessage))
            {
                return false;
            }

            if (selectCurrentTier)
            {
                selectedCandidates = currentTierCandidates;
                return true;
            }

            return TrySelectLowerTierCandidates(
                lowerTierCandidates,
                out selectedCandidates,
                out failureMessage);
        }

        /// <summary>
        /// 肯定MissionとNegatedMissionの候補群を相対重みで選びます。
        /// </summary>
        internal bool TrySelectConditionPolarityCandidates(
            IReadOnlyList<MissionCandidate> candidates,
            float negatedMissionSelectionWeight,
            out IReadOnlyList<MissionCandidate> selectedCandidates,
            out string failureMessage)
        {
            selectedCandidates = null;
            failureMessage = null;
            var positiveCandidates = candidates
                .Where(candidate => !candidate.Condition.IsNegated)
                .ToArray();
            var negatedCandidates = candidates
                .Where(candidate => candidate.Condition.IsNegated)
                .ToArray();

            if (positiveCandidates.Length == 0 && negatedCandidates.Length == 0)
            {
                failureMessage = "Mission polarity selection contains no candidates.";
                return false;
            }

            if (positiveCandidates.Length == 0)
            {
                if (!IsValidRandomWeight(negatedMissionSelectionWeight) ||
                    negatedMissionSelectionWeight <= 0f)
                {
                    failureMessage = "Negated mission candidates are disabled by their selection weight.";
                    return false;
                }

                selectedCandidates = negatedCandidates;
                return true;
            }

            if (negatedCandidates.Length == 0)
            {
                selectedCandidates = positiveCandidates;
                return true;
            }

            if (!TrySelectWeightedFirst(
                    PositiveMissionSelectionWeight,
                    negatedMissionSelectionWeight,
                    out bool selectPositive,
                    out failureMessage))
            {
                return false;
            }

            selectedCandidates = selectPositive ? positiveCandidates : negatedCandidates;
            return true;
        }

        /// <summary>
        /// 候補が存在するMissionKindを一様抽選し、該当候補群を返します。
        /// </summary>
        internal bool TrySelectKindCandidates(
            IReadOnlyList<MissionCandidate> candidates,
            out IReadOnlyList<MissionCandidate> selectedCandidates,
            out string failureMessage)
        {
            selectedCandidates = null;
            failureMessage = null;
            var kindValues = candidates
                .Select(candidate => candidate.Condition.Kind)
                .Distinct()
                .OrderBy(kind => kind)
                .ToArray();
            if (!TrySelectUniformIndex(
                    kindValues.Length,
                    "kind",
                    out int kindIndex,
                    out failureMessage))
            {
                return false;
            }

            MissionKind selectedKind = kindValues[kindIndex];
            selectedCandidates = candidates
                .Where(candidate => candidate.Condition.Kind == selectedKind)
                .ToArray();
            return true;
        }

        /// <summary>
        /// 候補群から1件を一様抽選します。
        /// </summary>
        internal bool TrySelectCandidate(
            IReadOnlyList<MissionCandidate> candidates,
            string stageName,
            out MissionCandidate selectedCandidate,
            out string failureMessage)
        {
            selectedCandidate = null;
            if (!TrySelectUniformIndex(
                    candidates.Count,
                    stageName,
                    out int candidateIndex,
                    out failureMessage))
            {
                return false;
            }

            selectedCandidate = candidates[candidateIndex];
            if (selectedCandidate == null)
            {
                failureMessage = $"Mission draw stage '{stageName}' selected a null candidate.";
                return false;
            }

            return true;
        }

        private bool TrySelectLowerTierCandidates(
            IReadOnlyList<MissionCandidate> lowerTierCandidates,
            out IReadOnlyList<MissionCandidate> selectedCandidates,
            out string failureMessage)
        {
            selectedCandidates = null;
            var tierValues = lowerTierCandidates
                .Select(candidate => candidate.Tier)
                .Distinct()
                .OrderBy(tier => tier)
                .ToArray();
            if (!TrySelectUniformIndex(
                    tierValues.Length,
                    "lower tier",
                    out int tierIndex,
                    out failureMessage))
            {
                return false;
            }

            int selectedTier = tierValues[tierIndex];
            selectedCandidates = lowerTierCandidates
                .Where(candidate => candidate.Tier == selectedTier)
                .ToArray();
            return true;
        }

        private bool TrySelectWeightedFirst(
            float firstWeight,
            float secondWeight,
            out bool selectFirst,
            out string failureMessage)
        {
            selectFirst = false;
            failureMessage = null;
            if (!IsValidRandomWeight(firstWeight) || !IsValidRandomWeight(secondWeight))
            {
                failureMessage = "Mission draw weights must be finite and non-negative.";
                return false;
            }

            float totalWeight = firstWeight + secondWeight;
            if (!IsFinite(totalWeight) || totalWeight <= 0f)
            {
                failureMessage = "Mission draw weights must contain a positive total.";
                return false;
            }

            if (!TryGetRandomValue(out float randomValue, out failureMessage))
            {
                return false;
            }

            selectFirst = WeightedIndexSelector.SelectIndex(
                new[] { firstWeight, secondWeight },
                randomValue) == 0;
            return true;
        }

        private bool TrySelectUniformIndex(
            int candidateCount,
            string stageName,
            out int selectedIndex,
            out string failureMessage)
        {
            selectedIndex = 0;
            failureMessage = null;
            if (candidateCount <= 0)
            {
                failureMessage = $"Mission draw stage '{stageName}' contains no candidates.";
                return false;
            }

            if (candidateCount == 1)
            {
                return true;
            }

            if (!TryGetRandomValue(out float randomValue, out failureMessage))
            {
                return false;
            }

            selectedIndex = UniformIndexSelector.SelectIndex(candidateCount, randomValue);
            return true;
        }

        private bool TryGetRandomValue(out float randomValue, out string failureMessage)
        {
            randomValue = _randomSource.NextFloat01();
            if (!IsFinite(randomValue) || randomValue < 0f || randomValue >= 1f)
            {
                failureMessage = "Random source returned a value outside the [0, 1) contract.";
                return false;
            }

            failureMessage = null;
            return true;
        }

        private static bool IsValidRandomWeight(float weight)
        {
            return IsFinite(weight) && weight >= 0f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
