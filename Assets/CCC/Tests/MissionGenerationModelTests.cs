using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Model;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// Missionパラメータ自動生成で使用する基本モデルの契約を検証します。
    /// </summary>
    public sealed class MissionGenerationModelTests
    {
        /// <summary>
        /// MissionKindの固定仕様を一元管理するCatalogがCore Modelに存在することを検証します。
        /// </summary>
        [Test]
        public void MissionKindDefinitionCatalog_ShouldExistInCoreModelNamespace()
        {
            System.Type catalogType = typeof(MissionKind).Assembly.GetType(
                "Ccc.Core.Model.MissionKindDefinitionCatalog");

            Assert.That(catalogType, Is.Not.Null);
        }

        /// <summary>
        /// 全MissionKindの数値識別子と固定定義が1対1で対応することを検証します。
        /// </summary>
        [Test]
        public void MissionKindDefinitionCatalog_ShouldCoverAllStableMissionKindValues()
        {
            MissionKind[] kinds = (MissionKind[])Enum.GetValues(typeof(MissionKind));

            Assert.That(kinds.Select(kind => (int)kind), Is.EqualTo(Enumerable.Range(0, 26)));
            Assert.That(MissionKindDefinitionCatalog.Definitions, Has.Count.EqualTo(26));
            Assert.That(
                MissionKindDefinitionCatalog.Definitions.Select(definition => definition.Kind),
                Is.EqualTo(kinds));
        }

        /// <summary>
        /// 固定仕様のパラメータ構造、肯定Tier、Negated可否とTierを検証します。
        /// </summary>
        [Test]
        public void MissionKindDefinitionCatalog_ShouldExposeConfirmedMissionContracts()
        {
            MissionKindDefinition between =
                MissionKindDefinitionCatalog.Get(MissionKind.Between);
            MissionKindDefinition modulo =
                MissionKindDefinitionCatalog.Get(MissionKind.Modulo);
            MissionKindDefinition even =
                MissionKindDefinitionCatalog.Get(MissionKind.Even);

            Assert.That(between.ParameterStructure, Is.EqualTo(MissionParameterStructure.Pair));
            Assert.That(between.PositiveTierRule, Is.EqualTo(MissionPositiveTierRule.MatchRatioThreshold));
            Assert.That(between.PositiveTier, Is.EqualTo(2));
            Assert.That(between.CriterionUnmetPositiveTier, Is.EqualTo(3));
            Assert.That(between.CanGenerateNegated, Is.True);
            Assert.That(between.NegatedTier, Is.EqualTo(4));
            Assert.That(modulo.PositiveTierRule, Is.EqualTo(MissionPositiveTierRule.Fixed));
            Assert.That(modulo.PositiveTier, Is.EqualTo(4));
            Assert.That(modulo.CriterionUnmetPositiveTier, Is.EqualTo(0));
            Assert.That(modulo.CanGenerateNegated, Is.False);
            Assert.That(even.ParameterStructure, Is.EqualTo(MissionParameterStructure.None));
            Assert.That(even.PositiveTier, Is.EqualTo(1));
            Assert.That(even.NegatedTier, Is.EqualTo(2));
        }

        /// <summary>
        /// 約数がパラメータの到達可能性で肯定Tierを切り替える定義を持つことを検証します。
        /// </summary>
        [Test]
        public void MissionKindDefinitionCatalog_Divisor_ShouldResolveTierByParameterReachability()
        {
            MissionKindDefinition divisor = MissionKindDefinitionCatalog.Get(MissionKind.Divisor);

            Assert.That(divisor.ParameterStructure, Is.EqualTo(MissionParameterStructure.Single));
            Assert.That(divisor.PositiveTierRule, Is.EqualTo(MissionPositiveTierRule.ParameterReachability));
            Assert.That(divisor.PositiveTier, Is.EqualTo(3));
            Assert.That(divisor.CriterionUnmetPositiveTier, Is.EqualTo(4));
            Assert.That(divisor.CanGenerateNegated, Is.True);
            Assert.That(divisor.NegatedTier, Is.EqualTo(4));
        }

        /// <summary>
        /// 累乗・桁・無限小数系Kindのパラメータ構造、固定肯定Tier、Negated Tierを検証します。
        /// </summary>
        [TestCase(MissionKind.Power, MissionParameterStructure.Single, 3, 4)]
        [TestCase(MissionKind.BetweenZeroAndOne, MissionParameterStructure.None, 3, 4)]
        [TestCase(MissionKind.TwoDigit, MissionParameterStructure.None, 1, 2)]
        [TestCase(MissionKind.TensDigit, MissionParameterStructure.Single, 2, 3)]
        [TestCase(MissionKind.DigitSum, MissionParameterStructure.Single, 4, 4)]
        [TestCase(MissionKind.RepeatingDecimal, MissionParameterStructure.None, 4, 4)]
        public void MissionKindDefinitionCatalog_ShouldExposeNumberPropertyMissionContracts(
            MissionKind kind,
            MissionParameterStructure expectedStructure,
            int expectedPositiveTier,
            int expectedNegatedTier)
        {
            MissionKindDefinition definition = MissionKindDefinitionCatalog.Get(kind);

            Assert.That(definition.ParameterStructure, Is.EqualTo(expectedStructure));
            Assert.That(definition.PositiveTierRule, Is.EqualTo(MissionPositiveTierRule.Fixed));
            Assert.That(definition.PositiveTier, Is.EqualTo(expectedPositiveTier));
            Assert.That(definition.CanGenerateNegated, Is.True);
            Assert.That(definition.NegatedTier, Is.EqualTo(expectedNegatedTier));
        }

        /// <summary>
        /// 明示値ドメインが重複除去され、昇順へ正規化されることを検証します。
        /// </summary>
        [Test]
        public void IntegerParameterDomain_ExplicitValues_ShouldNormalizeDeterministically()
        {
            IntegerParameterDomain domain =
                IntegerParameterDomain.FromExplicitValues(new[] { 3, -1, 3, 2 });

            Assert.That(domain.Count, Is.EqualTo(3));
            Assert.That(domain.EnumerateValues(), Is.EqualTo(new[] { -1, 2, 3 }));
            Assert.That(domain.Contains(2), Is.True);
            Assert.That(domain.Contains(1), Is.False);
        }
    }

    internal static class MissionTestSettings
    {
        internal static MissionCatalogEvaluationPolicy CreateEvaluationPolicy(
            GameplayConfig config)
        {
            return new MissionCatalogEvaluationPolicy(
                Rational.FromSingle(config.MinimumExpressionMatchRatio),
                Rational.FromSingle(config.MaximumExpressionMatchRatio),
                config.MinimumExpressionMatchCount,
                config.ZeroAndPrimeMinimumExpressionMatchCount,
                ((MissionKind[])Enum.GetValues(typeof(MissionKind))));
        }

        internal static MissionCatalogPreparationSettings CreatePresetPreparationSettings(
            IReadOnlyList<MissionParameterCandidate> candidates,
            GameplayConfig config)
        {
            MissionKindGenerationRule[] rules = candidates
                .Select(candidate => candidate.Condition.Kind)
                .Distinct()
                .Select(kind => new MissionKindGenerationRule(
                    kind,
                    true,
                    true,
                    false,
                    100,
                    null,
                    null))
                .ToArray();
            var generationSettings = new MissionParameterGenerationSettings(
                rules,
                100,
                Rational.FromSingle(0.25f));
            return MissionCatalogPreparationSettings.CreatePresetDebug(
                generationSettings,
                CreateEvaluationPolicy(config),
                candidates);
        }
    }
}
