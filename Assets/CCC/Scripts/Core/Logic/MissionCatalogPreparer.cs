#nullable enable
using System;
using System.Collections.Generic;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// Gameplay設定とCatalog準備設定から、候補式生成、Missionパラメータ候補準備、MissionCatalog構築を順に行います。
    /// </summary>
    public static class MissionCatalogPreparer
    {
        /// <summary>
        /// Gameplay設定の数字範囲で候補式を生成し、準備設定に従ってMissionCatalogを構築します。
        /// </summary>
        /// <param name="config">候補式の数字範囲を持つGameplay設定。</param>
        /// <param name="settings">Missionパラメータ候補の供給元と評価規則を持つ準備設定。</param>
        /// <returns>構築したCatalogと候補式一覧、または失敗種別と理由。</returns>
        /// <exception cref="ArgumentNullException">引数がnullの場合。</exception>
        public static MissionCatalogPreparationResult Prepare(
            GameplayConfig config,
            MissionCatalogPreparationSettings settings)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            IReadOnlyList<ExpressionCandidate> expressionCandidates;
            try
            {
                expressionCandidates = ExpressionCandidateGenerator.Generate(
                    config.MinimumNumber,
                    config.MaximumNumber);
            }
            catch (Exception exception)
            {
                return MissionCatalogPreparationResult.Failure(
                    MissionCatalogPreparationFailureKind.InvalidConfiguration,
                    $"Expression candidate generation failed: {exception.Message}");
            }

            IReadOnlyList<MissionParameterCandidate> missionParameterCandidates;
            try
            {
                switch (settings.SourceMode)
                {
                    case MissionParameterSourceMode.Automatic:
                        MissionParameterGenerationResult generationResult =
                            MissionParameterCandidateGenerator.Generate(
                                expressionCandidates,
                                settings.GenerationSettings);
                        if (!generationResult.IsSuccess)
                        {
                            return MissionCatalogPreparationResult.Failure(
                                MissionCatalogPreparationFailureKind.InvalidConfiguration,
                                generationResult.FailureMessage);
                        }

                        missionParameterCandidates = generationResult.Candidates;
                        break;

                    case MissionParameterSourceMode.PresetDebug:
                        missionParameterCandidates = FilterEnabledPresetCandidates(
                            settings.PresetCandidates,
                            settings.GenerationSettings);
                        break;

                    default:
                        return MissionCatalogPreparationResult.Failure(
                            MissionCatalogPreparationFailureKind.InvalidConfiguration,
                            $"Undefined mission parameter source mode: {settings.SourceMode}.");
                }
            }
            catch (Exception exception)
            {
                return MissionCatalogPreparationResult.Failure(
                    MissionCatalogPreparationFailureKind.InvalidConfiguration,
                    $"Mission parameter preparation failed: {exception.Message}");
            }

            MissionCatalogBuildResult catalogBuildResult;
            try
            {
                catalogBuildResult = MissionCatalogBuilder.Build(
                    expressionCandidates,
                    missionParameterCandidates,
                    settings.EvaluationPolicy,
                    settings.GenerationSettings.BetweenTierTwoThreshold);
            }
            catch (Exception exception)
            {
                return MissionCatalogPreparationResult.Failure(
                    MissionCatalogPreparationFailureKind.CatalogBuildFailed,
                    $"Mission catalog build failed: {exception.Message}");
            }

            if (!catalogBuildResult.IsSuccess)
            {
                return MissionCatalogPreparationResult.Failure(
                    MissionCatalogPreparationFailureKind.CatalogBuildFailed,
                    catalogBuildResult.FailureMessage);
            }

            return MissionCatalogPreparationResult.Success(catalogBuildResult.Catalog, expressionCandidates);
        }

        private static IReadOnlyList<MissionParameterCandidate> FilterEnabledPresetCandidates(
            IReadOnlyList<MissionParameterCandidate> presetCandidates,
            MissionParameterGenerationSettings generationSettings)
        {
            var enabledCandidates = new List<MissionParameterCandidate>();
            for (int index = 0; index < presetCandidates.Count; index++)
            {
                MissionParameterCandidate candidate = presetCandidates[index];
                if (generationSettings.TryGetRule(
                        candidate.Condition.Kind,
                        out MissionKindGenerationRule rule) &&
                    rule.IsEnabled &&
                    (!candidate.Condition.IsNegated || rule.GeneratesNegated))
                {
                    enabledCandidates.Add(candidate);
                }
            }

            return enabledCandidates.ToArray();
        }
    }
}
