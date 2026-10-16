#nullable enable
using System;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// 候補式生成からMissionCatalog構築までを行うCatalog準備処理の振る舞いを検証します。
    /// </summary>
    public sealed class MissionCatalogPreparerTests
    {
        private const int MaximumCandidateCount = 100;
        private const float BetweenTierTwoThreshold = 0.25f;

        /// <summary>
        /// Preset設定からCatalogと、Catalog構築に使った候補式一覧を返すことを検証します。
        /// </summary>
        [Test]
        public void Prepare_WithPresetSettings_ShouldReturnCatalogAndExpressionCandidates()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionParameterCandidate[] presets =
            {
                new MissionParameterCandidate(MissionCondition.Create(MissionKind.Even)),
                new MissionParameterCandidate(MissionCondition.Create(MissionKind.Odd))
            };

            MissionCatalogPreparationResult result = MissionCatalogPreparer.Prepare(
                config,
                MissionTestSettings.CreatePresetPreparationSettings(presets, config));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Catalog.Candidates.Select(candidate => candidate.Condition.Kind),
                Is.EquivalentTo(new[] { MissionKind.Even, MissionKind.Odd }));
            Assert.That(
                result.ExpressionCandidates.Count,
                Is.EqualTo(ExpressionCandidateGenerator.Generate(config.MinimumNumber, config.MaximumNumber).Count));
        }

        /// <summary>
        /// Preset候補のうち生成規則で無効なKindを除外することを検証します。
        /// </summary>
        [Test]
        public void Prepare_WithPresetSettings_ShouldExcludeCandidatesOfDisabledKind()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionCatalogPreparationSettings settings = CreatePresetSettings(
                config,
                new[]
                {
                    CreateRule(MissionKind.Even, isEnabled: true, generatesNegated: false),
                    CreateRule(MissionKind.Odd, isEnabled: false, generatesNegated: false)
                },
                MissionCondition.Create(MissionKind.Even),
                MissionCondition.Create(MissionKind.Odd));

            MissionCatalogPreparationResult result = MissionCatalogPreparer.Prepare(config, settings);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Catalog.Candidates.Select(candidate => candidate.Condition),
                Is.EqualTo(new[] { MissionCondition.Create(MissionKind.Even) }));
        }

        /// <summary>
        /// 生成規則がNegatedを生成しないKindのNegated Preset候補を除外することを検証します。
        /// </summary>
        [Test]
        public void Prepare_WithPresetSettings_ShouldExcludeNegatedCandidatesWhenRuleDoesNotGenerateNegated()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionCatalogPreparationSettings settings = CreatePresetSettings(
                config,
                new[] { CreateRule(MissionKind.Even, isEnabled: true, generatesNegated: false) },
                MissionCondition.Create(MissionKind.Even),
                MissionCondition.Create(MissionKind.Even, isNegated: true));

            MissionCatalogPreparationResult result = MissionCatalogPreparer.Prepare(config, settings);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                result.Catalog.Candidates.Select(candidate => candidate.Condition.IsNegated),
                Is.EqualTo(new[] { false }));
        }

        /// <summary>
        /// 有効な候補が残らない場合にCatalog構築失敗として返すことを検証します。
        /// </summary>
        [Test]
        public void Prepare_WhenNoPresetCandidateIsEnabled_ShouldFailAsCatalogBuildFailed()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionCatalogPreparationSettings settings = CreatePresetSettings(
                config,
                new[]
                {
                    CreateRule(MissionKind.Even, isEnabled: true, generatesNegated: false),
                    CreateRule(MissionKind.Odd, isEnabled: false, generatesNegated: false)
                },
                MissionCondition.Create(MissionKind.Odd));

            MissionCatalogPreparationResult result = MissionCatalogPreparer.Prepare(config, settings);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.FailureKind, Is.EqualTo(MissionCatalogPreparationFailureKind.CatalogBuildFailed));
            Assert.That(result.FailureMessage, Is.Not.Empty);
        }

        /// <summary>
        /// 失敗ResultからCatalogと候補式一覧を取得できないことを検証します。
        /// </summary>
        [Test]
        public void FailureResult_ShouldRejectCatalogAndExpressionCandidatesAccess()
        {
            MissionCatalogPreparationResult result = MissionCatalogPreparationResult.Failure(
                MissionCatalogPreparationFailureKind.InvalidConfiguration,
                "invalid");

            Assert.Throws<InvalidOperationException>(() => _ = result.Catalog);
            Assert.Throws<InvalidOperationException>(() => _ = result.ExpressionCandidates);
        }

        /// <summary>
        /// 失敗種別にNoneを指定した失敗Resultを拒否することを検証します。
        /// </summary>
        [Test]
        public void FailureResult_ShouldRejectNoneKind()
        {
            Assert.Throws<ArgumentException>(() => MissionCatalogPreparationResult.Failure(
                MissionCatalogPreparationFailureKind.None,
                "invalid"));
        }

        /// <summary>
        /// 設定がnullの場合に例外を送出することを検証します。
        /// </summary>
        [Test]
        public void Prepare_ShouldRejectNullArguments()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionCatalogPreparationSettings settings = MissionTestSettings.CreatePresetPreparationSettings(
                new[] { new MissionParameterCandidate(MissionCondition.Create(MissionKind.Even)) },
                config);

            Assert.Throws<ArgumentNullException>(() => MissionCatalogPreparer.Prepare(null!, settings));
            Assert.Throws<ArgumentNullException>(() => MissionCatalogPreparer.Prepare(config, null!));
        }

        private static MissionCatalogPreparationSettings CreatePresetSettings(
            GameplayConfig config,
            MissionKindGenerationRule[] rules,
            params MissionCondition[] presetConditions)
        {
            return MissionCatalogPreparationSettings.CreatePresetDebug(
                new MissionParameterGenerationSettings(
                    rules,
                    MaximumCandidateCount,
                    Rational.FromSingle(BetweenTierTwoThreshold)),
                MissionTestSettings.CreateEvaluationPolicy(config),
                presetConditions.Select(condition => new MissionParameterCandidate(condition)).ToArray());
        }

        private static MissionKindGenerationRule CreateRule(MissionKind kind, bool isEnabled, bool generatesNegated)
        {
            return new MissionKindGenerationRule(
                kind,
                isEnabled,
                appliesMatchRatioRange: true,
                generatesNegated,
                MaximumCandidateCount,
                null,
                null);
        }
    }
}
