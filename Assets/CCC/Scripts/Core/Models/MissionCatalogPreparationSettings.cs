using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ccc.Core.Model
{
    /// <summary>
    /// Session開始時のMission候補取得とCatalog評価に必要な純C#設定を保持します。
    /// </summary>
    public sealed class MissionCatalogPreparationSettings
    {
        private readonly IReadOnlyList<MissionParameterCandidate> _presetCandidates;

        /// <summary>候補取得経路を取得します。</summary>
        public MissionParameterSourceMode SourceMode { get; }

        /// <summary>自動生成およびProfile有効Kind判定に使用する設定を取得します。</summary>
        public MissionParameterGenerationSettings GenerationSettings { get; }

        /// <summary>Catalog採用判定に使用する評価規則を取得します。</summary>
        public MissionCatalogEvaluationPolicy EvaluationPolicy { get; }

        /// <summary>PresetDebugで使用するTier未確定候補を取得します。</summary>
        public IReadOnlyList<MissionParameterCandidate> PresetCandidates => _presetCandidates;

        private MissionCatalogPreparationSettings(
            MissionParameterSourceMode sourceMode,
            MissionParameterGenerationSettings generationSettings,
            MissionCatalogEvaluationPolicy evaluationPolicy,
            IReadOnlyList<MissionParameterCandidate> presetCandidates)
        {
            SourceMode = sourceMode;
            GenerationSettings = generationSettings ??
                throw new ArgumentNullException(nameof(generationSettings));
            EvaluationPolicy = evaluationPolicy ??
                throw new ArgumentNullException(nameof(evaluationPolicy));
            _presetCandidates = presetCandidates;
        }

        /// <summary>
        /// 自動生成経路の準備設定を生成します。
        /// </summary>
        /// <param name="generationSettings">候補生成設定。</param>
        /// <param name="evaluationPolicy">Catalog評価規則。</param>
        /// <returns>Automatic準備設定。</returns>
        public static MissionCatalogPreparationSettings CreateAutomatic(
            MissionParameterGenerationSettings generationSettings,
            MissionCatalogEvaluationPolicy evaluationPolicy)
        {
            return new MissionCatalogPreparationSettings(
                MissionParameterSourceMode.Automatic,
                generationSettings,
                evaluationPolicy,
                Array.Empty<MissionParameterCandidate>());
        }

        /// <summary>
        /// PresetDebug経路の準備設定を生成します。
        /// </summary>
        /// <param name="generationSettings">Profile有効KindとTier閾値を保持する設定。</param>
        /// <param name="evaluationPolicy">Catalog評価規則。</param>
        /// <param name="presetCandidates">Tier未確定Preset候補。</param>
        /// <returns>PresetDebug準備設定。</returns>
        public static MissionCatalogPreparationSettings CreatePresetDebug(
            MissionParameterGenerationSettings generationSettings,
            MissionCatalogEvaluationPolicy evaluationPolicy,
            IReadOnlyList<MissionParameterCandidate> presetCandidates)
        {
            if (presetCandidates == null)
            {
                throw new ArgumentNullException(nameof(presetCandidates));
            }

            if (presetCandidates.Count == 0)
            {
                throw new ArgumentException(
                    "Preset candidates cannot be empty.",
                    nameof(presetCandidates));
            }

            var snapshot = new MissionParameterCandidate[presetCandidates.Count];
            for (int index = 0; index < presetCandidates.Count; index++)
            {
                snapshot[index] = presetCandidates[index];
            }

            return new MissionCatalogPreparationSettings(
                MissionParameterSourceMode.PresetDebug,
                generationSettings,
                evaluationPolicy,
                new ReadOnlyCollection<MissionParameterCandidate>(snapshot));
        }
    }
}
