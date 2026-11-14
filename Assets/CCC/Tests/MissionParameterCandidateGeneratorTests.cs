using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// 約数・累乗・桁・無限小数系MissionKindの候補自動生成を検証します。
    /// </summary>
    public sealed class MissionParameterCandidateGeneratorTests
    {
        private const int MaximumCandidateCount = 100;
        private const float BetweenTierTwoThreshold = 0.25f;
        private const int NormalMinimumNumber = 1;
        private const int NormalMaximumNumber = 5;
        private const int HardMinimumNumber = -5;
        private const int HardMaximumNumber = 5;

        /// <summary>
        /// 1パラメータの新しいKindが、制約を満たし到達可能な結果を持つパラメータだけを生成することを検証します。
        /// </summary>
        [TestCase(MissionKind.Divisor, new[] { -1, 0, 1, 12 }, new[] { 1, 12 })]
        [TestCase(MissionKind.Power, new[] { 1, 2, 3, 11 }, new[] { 2, 3 })]
        [TestCase(MissionKind.TensDigit, new[] { -1, 0, 1, 2, 3, 10 }, new[] { 0, 1, 2 })]
        [TestCase(MissionKind.DigitSum, new[] { 0, 1, 7, 9, 10 }, new[] { 1, 7, 9 })]
        public void Generate_SingleParameterKindInNormalRange_ShouldKeepOnlyReachableParameters(
            MissionKind kind,
            int[] domainValues,
            int[] expectedParameters)
        {
            MissionParameterGenerationResult result = Generate(
                NormalMinimumNumber,
                NormalMaximumNumber,
                CreateSingleParameterRule(kind, domainValues, generatesNegated: false));

            Assert.That(result.IsSuccess, Is.True, result.FailureMessage);
            Assert.That(
                result.Candidates.Select(candidate => candidate.Condition.ParameterA),
                Is.EqualTo(expectedParameters));
        }

        /// <summary>
        /// 負の数を含む数字範囲で、負の結果からも十の位の到達可能性を判定することを検証します。
        /// </summary>
        [Test]
        public void Generate_TensDigitInHardRange_ShouldTreatNegativeResultsAsUnsigned()
        {
            MissionParameterGenerationResult result = Generate(
                HardMinimumNumber,
                HardMaximumNumber,
                CreateSingleParameterRule(MissionKind.TensDigit, new[] { 0, 1, 2, 3 }, generatesNegated: false));

            Assert.That(result.IsSuccess, Is.True, result.FailureMessage);
            Assert.That(
                result.Candidates.Select(candidate => candidate.Condition.ParameterA),
                Is.EqualTo(new[] { 0, 1, 2 }));
        }

        /// <summary>
        /// 1パラメータの新しいKindが、パラメータごとに肯定・Negatedの順で候補を生成することを検証します。
        /// </summary>
        [Test]
        public void Generate_SingleParameterKindWithNegated_ShouldExpandPolarityPerParameter()
        {
            MissionParameterGenerationResult result = Generate(
                NormalMinimumNumber,
                NormalMaximumNumber,
                CreateSingleParameterRule(MissionKind.Divisor, new[] { 4, 12 }, generatesNegated: true));

            Assert.That(result.IsSuccess, Is.True, result.FailureMessage);
            Assert.That(
                result.Candidates.Select(candidate => candidate.Condition),
                Is.EqualTo(new[]
                {
                    MissionCondition.Create(MissionKind.Divisor, 4),
                    MissionCondition.Create(MissionKind.Divisor, 4, true),
                    MissionCondition.Create(MissionKind.Divisor, 12),
                    MissionCondition.Create(MissionKind.Divisor, 12, true)
                }));
        }

        /// <summary>
        /// パラメータなしの新しいKindが肯定・Negatedの順で候補を生成することを検証します。
        /// </summary>
        [TestCase(MissionKind.BetweenZeroAndOne)]
        [TestCase(MissionKind.TwoDigit)]
        [TestCase(MissionKind.RepeatingDecimal)]
        public void Generate_NoParameterKindWithNegated_ShouldGeneratePositiveAndNegated(MissionKind kind)
        {
            var rule = new MissionKindGenerationRule(
                kind,
                isEnabled: true,
                appliesMatchRatioRange: true,
                generatesNegated: true,
                MaximumCandidateCount,
                null,
                null);

            MissionParameterGenerationResult result = Generate(NormalMinimumNumber, NormalMaximumNumber, rule);

            Assert.That(result.IsSuccess, Is.True, result.FailureMessage);
            Assert.That(
                result.Candidates.Select(candidate => candidate.Condition),
                Is.EqualTo(new[]
                {
                    MissionCondition.Create(kind),
                    MissionCondition.Create(kind, true)
                }));
        }

        private static MissionParameterGenerationResult Generate(
            int minimumNumber,
            int maximumNumber,
            MissionKindGenerationRule rule)
        {
            IReadOnlyList<ExpressionCandidate> expressions =
                ExpressionCandidateGenerator.Generate(minimumNumber, maximumNumber);
            var settings = new MissionParameterGenerationSettings(
                new[] { rule },
                MaximumCandidateCount,
                Rational.FromSingle(BetweenTierTwoThreshold));
            return MissionParameterCandidateGenerator.Generate(expressions, settings);
        }

        private static MissionKindGenerationRule CreateSingleParameterRule(
            MissionKind kind,
            int[] domainValues,
            bool generatesNegated)
        {
            return new MissionKindGenerationRule(
                kind,
                isEnabled: true,
                appliesMatchRatioRange: true,
                generatesNegated,
                MaximumCandidateCount,
                IntegerParameterDomain.FromExplicitValues(domainValues),
                null);
        }
    }
}
