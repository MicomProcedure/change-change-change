using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// 固定ExpressionCandidate表を使って、MissionCandidateとMissionCatalogの成立条件を検証します。
    /// </summary>
    public sealed class MissionCandidateCatalogContractTests
    {
        private static readonly IReadOnlyList<ExpressionCandidate> FixedExpressionCandidates =
            new[]
            {
                CreateExpressionCandidate(1, OperatorType.Add, 1), // 2: Even
                CreateExpressionCandidate(1, OperatorType.Add, 2), // 3: Odd
                CreateExpressionCandidate(1, OperatorType.Subtract, 2), // -1: Odd, Negative
                CreateExpressionCandidate(1, OperatorType.Divide, 2), // 1/2: Decimal, Positive
                CreateExpressionCandidate(2, OperatorType.Subtract, 2), // 0: Zero, Even
                CreateExpressionCandidate(2, OperatorType.Add, 2), // 4: Even, Square
                CreateExpressionCandidate(2, OperatorType.Multiply, 2), // 4: Even, Square
                CreateExpressionCandidate(3, OperatorType.Add, 2), // 5: Odd
                CreateExpressionCandidate(3, OperatorType.Subtract, 1), // 2: Even
                CreateExpressionCandidate(3, OperatorType.Divide, 2), // 3/2: Decimal, Positive
                CreateExpressionCandidate(1, OperatorType.Divide, 0) // DivisionByZero: denominator excluded
            };

        /// <summary>
        /// 固定表の全行数と、正常なExpressionCandidateの件数を検証します。
        /// </summary>
        [Test]
        public void FixedExpressionCandidateTable_ShouldContainExpectedRows()
        {
            Assert.That(FixedExpressionCandidates.Count, Is.EqualTo(11));
            Assert.That(FixedExpressionCandidates.Count(candidate => candidate.Result.IsDefined), Is.EqualTo(10));
            Assert.That(FixedExpressionCandidates.Count(candidate => !candidate.Result.IsDefined), Is.EqualTo(1));
        }

        /// <summary>
        /// 固定表を使ったMissionCatalogが、適合件数と正常候補基準の割合を保持することを検証します。
        /// </summary>
        [Test]
        public void Build_ShouldCalculateMatchCountAndRatioFromDefinedCandidates()
        {
            var parameters = new List<MissionParameterCandidate>
            {
                new MissionParameterCandidate(MissionCondition.Create(MissionKind.Even)),
                new MissionParameterCandidate(MissionCondition.Create(MissionKind.Odd)),
                new MissionParameterCandidate(MissionCondition.Create(MissionKind.Decimal)),
                new MissionParameterCandidate(MissionCondition.Create(MissionKind.Zero))
            };

            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionCatalogBuildResult buildResult =
                MissionCatalogBuilder.Build(
                    FixedExpressionCandidates,
                    parameters,
                    MissionTestSettings.CreateEvaluationPolicy(config),
                    Rational.FromSingle(0.25f));

            Assert.That(buildResult.IsSuccess, Is.True);
            Assert.That(buildResult.Catalog.Candidates.Count, Is.EqualTo(4));

            MissionCandidate evenCandidate = FindCandidate(buildResult.Catalog, MissionKind.Even);
            Assert.That(evenCandidate.ExpressionMatchCount, Is.EqualTo(5));
            Assert.That(evenCandidate.EvaluatedExpressionCount, Is.EqualTo(10));
            Assert.That(evenCandidate.ExpressionMatchRatio, Is.EqualTo(0.5f).Within(0.0001f));

            MissionCandidate oddCandidate = FindCandidate(buildResult.Catalog, MissionKind.Odd);
            Assert.That(oddCandidate.ExpressionMatchCount, Is.EqualTo(3));
            Assert.That(oddCandidate.ExpressionMatchRatio, Is.EqualTo(0.3f).Within(0.0001f));

            MissionCandidate decimalCandidate = FindCandidate(buildResult.Catalog, MissionKind.Decimal);
            Assert.That(decimalCandidate.ExpressionMatchCount, Is.EqualTo(2));
            Assert.That(decimalCandidate.ExpressionMatchRatio, Is.EqualTo(0.2f).Within(0.0001f));

            MissionCandidate zeroCandidate = FindCandidate(buildResult.Catalog, MissionKind.Zero);
            Assert.That(zeroCandidate.ExpressionMatchCount, Is.EqualTo(1));
            Assert.That(zeroCandidate.ExpressionMatchRatio, Is.EqualTo(0.1f).Within(0.0001f));
        }

        /// <summary>
        /// 同一条件の重複候補が最初の1件へ正規化されることを検証します。
        /// </summary>
        [Test]
        public void Build_ShouldNormalizeDuplicateMissionParameterCandidates()
        {
            var duplicateCondition = MissionCondition.Create(MissionKind.Even);
            var parameters = new List<MissionParameterCandidate>
            {
                new MissionParameterCandidate(duplicateCondition),
                new MissionParameterCandidate(duplicateCondition)
            };

            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionCatalogBuildResult buildResult = MissionCatalogBuilder.Build(
                FixedExpressionCandidates,
                parameters,
                MissionTestSettings.CreateEvaluationPolicy(config),
                Rational.FromSingle(0.25f));

            Assert.That(buildResult.IsSuccess, Is.True);
            Assert.That(buildResult.Catalog.Candidates, Has.Count.EqualTo(1));
        }

        /// <summary>
        /// 成立条件を満たす候補がない場合に空Catalogとして失敗することを検証します。
        /// </summary>
        [Test]
        public void Build_ShouldReturnEmptyCatalogWhenNoCandidatePasses()
        {
            var parameters = new List<MissionParameterCandidate>
            {
                new MissionParameterCandidate(MissionCondition.Create(MissionKind.Positive))
            };

            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionCatalogBuildResult buildResult =
                MissionCatalogBuilder.Build(
                    FixedExpressionCandidates,
                    parameters,
                    MissionTestSettings.CreateEvaluationPolicy(config),
                    Rational.FromSingle(0.25f));

            Assert.That(buildResult.IsSuccess, Is.False);
            Assert.That(buildResult.FailureKind, Is.EqualTo(MissionCatalogBuildFailureKind.EmptyCatalog));
            Assert.That(buildResult.FailureMessage, Is.Not.Empty);
        }

        private static MissionCandidate FindCandidate(MissionCatalog catalog, MissionKind kind)
        {
            return catalog.Candidates.Single(candidate => candidate.Condition.Kind == kind);
        }

        private static ExpressionCandidate CreateExpressionCandidate(int left, OperatorType operatorType, int right)
        {
            var expression = new Expression(left, operatorType, right);
            return new ExpressionCandidate(expression, ExpressionCalculator.Evaluate(expression));
        }
    }
}
