using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Current Tier優先と段階別Uniform抽選を行うMission抽選Strategyです。
    /// </summary>
    public sealed class SimpleMissionDrawStrategy : IMissionDrawStrategy
    {
        private readonly MissionDrawSelectionModule _selectionModule;

        /// <summary>
        /// 乱数提供元からStrategyを生成します。
        /// </summary>
        /// <param name="randomSource">0以上1未満の乱数提供元。</param>
        /// <exception cref="ArgumentNullException">乱数提供元がnullの場合。</exception>
        public SimpleMissionDrawStrategy(IRandomSource randomSource)
        {
            _selectionModule = new MissionDrawSelectionModule(
                randomSource ?? throw new ArgumentNullException(nameof(randomSource)));
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

                if (!_selectionModule.TrySelectTierCandidates(
                        catalogCandidates,
                        context.Section,
                        context.SessionContext.Config,
                        shouldSelectSpecificLowerTierWhenCurrentTierIsUnavailable: false,
                        out var selectedTierCandidates,
                        out string tierFailure))
                {
                    return MissionDrawResult.Failure(tierFailure);
                }

                if (!_selectionModule.TrySelectKindCandidates(
                        selectedTierCandidates,
                        out var selectedKindCandidates,
                        out string kindFailure))
                {
                    return MissionDrawResult.Failure(kindFailure);
                }

                if (!_selectionModule.TrySelectCandidate(
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
    }
}
