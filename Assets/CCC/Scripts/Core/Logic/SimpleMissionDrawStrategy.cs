#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 簡易版の規則（Tier、Kind、パラメータの順）でMissionを抽選する戦略。
    /// </summary>
    public sealed class SimpleMissionDrawStrategy : IMissionDrawStrategy
    {
        /// <summary>
        /// 指定されたカタログから次のMissionを抽選します。
        /// </summary>
        /// <param name="context">Mission抽選に必要な文脈。</param>
        /// <returns>抽選結果。</returns>
        public MissionDrawResult Draw(MissionDrawContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var candidates = context.Catalog.Candidates;
            if (candidates.Count == 0)
            {
                return MissionDrawResult.Failure(MissionDrawFailureKind.EmptyCatalog, "Mission catalog is empty.");
            }

            int currentTier = context.Config.TierProgression.GetTier(context.Section);
            var currentTierCandidates = candidates.Where(candidate => candidate.Tier == currentTier).ToArray();
            var lowerTierCandidates = candidates.Where(candidate => candidate.Tier < currentTier).ToArray();

            IReadOnlyList<MissionCandidate> tierCandidates;
            if (currentTierCandidates.Length == 0)
            {
                tierCandidates = lowerTierCandidates;
            }
            else if (lowerTierCandidates.Length == 0 || currentTier <= 1)
            {
                tierCandidates = currentTierCandidates;
            }
            else
            {
                var selectedTierGroup = WeightedIndexSelector.SelectIndex(
                    new[] { context.Config.CurrentTierDrawWeight, context.Config.LowerTierDrawWeight },
                    context.RandomSource.NextSample());
                tierCandidates = selectedTierGroup == 0 ? currentTierCandidates : lowerTierCandidates;
            }

            if (tierCandidates.Count == 0)
            {
                return MissionDrawResult.Failure(MissionDrawFailureKind.NoCandidateForTier, "No mission candidate is available for the resolved tier range.");
            }

            var kindGroups = tierCandidates.GroupBy(candidate => candidate.Condition.Kind).OrderBy(group => group.Key).ToArray();
            int kindIndex = SelectIndex(kindGroups.Length, context.RandomSource);
            var parameterCandidates = kindGroups[kindIndex].OrderBy(candidate => candidate.Condition.ParameterA).ThenBy(candidate => candidate.Condition.ParameterB).ToArray();
            int parameterIndex = SelectIndex(parameterCandidates.Length, context.RandomSource);
            MissionCandidate selectedCandidate = parameterCandidates[parameterIndex];
            var selectedMission = new Mission(context.NextMissionId, selectedCandidate.Condition, selectedCandidate.Tier);
            return MissionDrawResult.Success(selectedMission, selectedCandidate);
        }

        private static int SelectIndex(int count, IRandomSource randomSource)
        {
            return count == 1 ? 0 : UniformIndexSelector.SelectIndex(count, randomSource.NextSample());
        }
    }
}
