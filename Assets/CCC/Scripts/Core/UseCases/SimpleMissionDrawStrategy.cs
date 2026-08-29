using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Current Tier優先と段階別Uniform抽選を行うMission抽選Strategyです。
    /// </summary>
    public sealed class SimpleMissionDrawStrategy : IMissionDrawStrategy
    {
        private readonly IRandomSource _randomSource;

        /// <summary>
        /// 乱数提供元からStrategyを生成します。
        /// </summary>
        /// <param name="randomSource">0以上1未満の乱数提供元。</param>
        /// <exception cref="ArgumentNullException">乱数提供元がnullの場合。</exception>
        public SimpleMissionDrawStrategy(IRandomSource randomSource)
        {
            _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        }

        /// <summary>
        /// Current／Lower、Tier、Kind、Candidateの順に候補を抽選します。
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
                var catalogCandidates = context.SessionContext.MissionCatalog.Candidates;
                if (catalogCandidates == null || catalogCandidates.Count == 0)
                {
                    return MissionDrawResult.Failure("Mission catalog contains no candidates.");
                }

                int currentTier = context.SessionContext.Config.TierProgression.GetTier(context.Section);
                var currentTierCandidates = catalogCandidates
                    .Where(candidate => candidate.Tier == currentTier)
                    .ToArray();
                var lowerTierCandidates = catalogCandidates
                    .Where(candidate => candidate.Tier < currentTier)
                    .ToArray();

                IReadOnlyList<MissionCandidate> selectedTierCandidates;
                if (currentTierCandidates.Length == 0 && lowerTierCandidates.Length == 0)
                {
                    return MissionDrawResult.Failure(
                        $"No mission candidates are available for tier {currentTier} at section {context.Section}.");
                }

                if (currentTierCandidates.Length == 0)
                {
                    selectedTierCandidates = lowerTierCandidates;
                }
                else if (lowerTierCandidates.Length == 0)
                {
                    selectedTierCandidates = currentTierCandidates;
                }
                else
                {
                    bool selectCurrentTier;
                    if (!TrySelectWeighted(
                            context.SessionContext.Config.CurrentTierDrawWeight,
                            context.SessionContext.Config.LowerTierDrawWeight,
                            out selectCurrentTier,
                            out string weightedFailure))
                    {
                        return MissionDrawResult.Failure(weightedFailure);
                    }
                    if (selectCurrentTier)
                    {
                        selectedTierCandidates = currentTierCandidates;
                    }
                    else
                    {
                        selectedTierCandidates = SelectLowerTierCandidates(
                            lowerTierCandidates,
                            out string lowerTierFailure);
                        if (selectedTierCandidates == null)
                        {
                            return MissionDrawResult.Failure(lowerTierFailure);
                        }
                    }
                }

                var selectedKindCandidates = SelectKindCandidates(
                    selectedTierCandidates,
                    out string kindFailure);
                if (selectedKindCandidates == null)
                {
                    return MissionDrawResult.Failure(kindFailure);
                }

                if (!TrySelectUniform(
                        selectedKindCandidates,
                        "candidate",
                        out MissionCandidate selectedCandidate,
                        out string candidateFailure))
                {
                    return MissionDrawResult.Failure(candidateFailure);
                }

                return MissionDrawResult.Success(selectedCandidate);
            }
            catch (Exception exception)
            {
                return MissionDrawResult.Failure(
                    $"Mission draw failed because the session context is invalid: {exception.Message}");
            }
        }

        private bool TrySelectWeighted(
            float currentTierWeight,
            float lowerTierWeight,
            out bool selectCurrentTier,
            out string failureMessage)
        {
            selectCurrentTier = false;
            failureMessage = null;
            if (!IsValidRandomWeight(currentTierWeight) || !IsValidRandomWeight(lowerTierWeight))
            {
                failureMessage = "Mission draw weights must be finite and non-negative.";
                return false;
            }

            float totalWeight = currentTierWeight + lowerTierWeight;
            if (!IsFinite(totalWeight) || totalWeight <= 0f)
            {
                failureMessage = "Mission draw weights must contain a positive total.";
                return false;
            }

            if (!TryGetRandomValue(out float randomValue, out failureMessage))
            {
                return false;
            }

            selectCurrentTier = randomValue < currentTierWeight / totalWeight;
            return true;
        }

        private IReadOnlyList<MissionCandidate> SelectLowerTierCandidates(
            IReadOnlyList<MissionCandidate> lowerTierCandidates,
            out string failureMessage)
        {
            failureMessage = null;
            var tierValues = lowerTierCandidates
                .Select(candidate => candidate.Tier)
                .Distinct()
                .OrderBy(tier => tier)
                .ToArray();
            if (tierValues.Length == 0)
            {
                failureMessage = "Lower-tier mission candidates contain no tier.";
                return null;
            }

            if (!TrySelectUniformIndex(tierValues.Length, "lower tier", out int tierIndex, out failureMessage))
            {
                return null;
            }

            int selectedTier = tierValues[tierIndex];
            return lowerTierCandidates
                .Where(candidate => candidate.Tier == selectedTier)
                .ToArray();
        }

        private IReadOnlyList<MissionCandidate> SelectKindCandidates(
            IReadOnlyList<MissionCandidate> tierCandidates,
            out string failureMessage)
        {
            failureMessage = null;
            var kindValues = tierCandidates
                .Select(candidate => candidate.Condition.Kind)
                .Distinct()
                .OrderBy(kind => kind)
                .ToArray();
            if (kindValues.Length == 0)
            {
                failureMessage = "Mission candidates contain no MissionKind.";
                return null;
            }

            if (!TrySelectUniformIndex(kindValues.Length, "kind", out int kindIndex, out failureMessage))
            {
                return null;
            }

            MissionKind selectedKind = kindValues[kindIndex];
            return tierCandidates
                .Where(candidate => candidate.Condition.Kind == selectedKind)
                .ToArray();
        }

        private bool TrySelectUniform(
            IReadOnlyList<MissionCandidate> candidates,
            string stageName,
            out MissionCandidate selectedCandidate,
            out string failureMessage)
        {
            selectedCandidate = null;
            if (!TrySelectUniformIndex(candidates.Count, stageName, out int candidateIndex, out failureMessage))
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

            selectedIndex = Math.Min((int)(randomValue * candidateCount), candidateCount - 1);
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
