#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// Sectionに応じて目標式適合割合の範囲を下げるMission抽選と、その範囲を決める部品の振る舞いを検証します。
    /// </summary>
    public sealed class SectionDecliningMatchRatioMissionDrawStrategyTests
    {
        /// <summary>
        /// 目標式適合割合の範囲が、下限が上限を超える値と0以上1以下でない値を拒否することを検証します。
        /// </summary>
        [Test]
        public void TargetExpressionMatchRatioRange_ShouldRejectInvalidBounds()
        {
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => new TargetExpressionMatchRatioRange(0.30f, 0.20f))!
                    .ParamName,
                Is.EqualTo("lowerBound"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => new TargetExpressionMatchRatioRange(-0.01f, 0.20f))!
                    .ParamName,
                Is.EqualTo("lowerBound"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => new TargetExpressionMatchRatioRange(0.10f, float.NaN))!
                    .ParamName,
                Is.EqualTo("upperBound"));
        }

        /// <summary>
        /// 目標式適合割合の範囲が、下限と上限を含む閉区間として値を判定することを検証します。
        /// </summary>
        [Test]
        public void TargetExpressionMatchRatioRange_ShouldContainBothBounds()
        {
            var range = new TargetExpressionMatchRatioRange(0.10f, 0.20f);

            Assert.That(range.IsSingleValue, Is.False);
            Assert.That(range.Contains(0.10f), Is.True);
            Assert.That(range.Contains(0.20f), Is.True);
            Assert.That(range.Contains(0.09f), Is.False);
            Assert.That(range.Contains(0.21f), Is.False);
        }

        /// <summary>
        /// Gameplay設定が、負または有限でない低下量と、目標式適合割合を超える最小値を拒否することを検証します。
        /// </summary>
        [Test]
        public void GameplayConfig_ShouldRejectInvalidTargetRatioReductionSettings()
        {
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => CreateConfig(targetExpressionMatchRatioReductionPerSection: -0.01f))!.ParamName,
                Is.EqualTo("targetExpressionMatchRatioReductionPerSection"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => CreateConfig(targetExpressionMatchRatioReductionPerSection: float.NaN))!.ParamName,
                Is.EqualTo("targetExpressionMatchRatioReductionPerSection"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => CreateConfig(targetExpressionMatchRatio: 0.20f, minimumTargetExpressionMatchRatio: 0.21f))!
                    .ParamName,
                Is.EqualTo("minimumTargetExpressionMatchRatio"));
        }

        /// <summary>
        /// 固定版の部品がSectionにかかわらず設定の目標式適合割合だけを範囲とすることを検証します。
        /// </summary>
        [Test]
        public void FixedResolver_ShouldReturnConfiguredTargetAsSingleValue()
        {
            GameplayConfig config = CreateConfig(
                targetExpressionMatchRatio: 0.50f,
                targetExpressionMatchRatioReductionPerSection: 0.10f,
                minimumTargetExpressionMatchRatio: 0.10f);

            TargetExpressionMatchRatioRange range =
                new FixedTargetExpressionMatchRatioRangeResolver().Resolve(section: 5, config);

            Assert.That(range.IsSingleValue, Is.True);
            Assert.That(range.LowerBound, Is.EqualTo(0.50f));
            Assert.That(range.UpperBound, Is.EqualTo(0.50f));
        }

        /// <summary>
        /// Section低下版の部品が、Section 1では幅0、以降は1 Sectionごとに下限を直線的に下げ、最小値で止めることを検証します。
        /// </summary>
        [TestCase(1, 0.50f)]
        [TestCase(2, 0.40f)]
        [TestCase(3, 0.30f)]
        [TestCase(5, 0.10f)]
        [TestCase(20, 0.10f)]
        public void DecliningResolver_ShouldLowerLowerBoundLinearlyUntilMinimum(int section, float expectedLowerBound)
        {
            GameplayConfig config = CreateConfig(
                targetExpressionMatchRatio: 0.50f,
                targetExpressionMatchRatioReductionPerSection: 0.10f,
                minimumTargetExpressionMatchRatio: 0.10f);

            TargetExpressionMatchRatioRange range =
                new SectionDecliningTargetExpressionMatchRatioRangeResolver().Resolve(section, config);

            Assert.That(range.LowerBound, Is.EqualTo(expectedLowerBound).Within(1e-6f));
            Assert.That(range.UpperBound, Is.EqualTo(0.50f));
            Assert.That(range.IsSingleValue, Is.EqualTo(section == 1));
        }

        /// <summary>
        /// Section低下版の部品が、低下量0ではSectionにかかわらず幅0の範囲を返すことを検証します。
        /// </summary>
        [Test]
        public void DecliningResolver_WhenReductionIsZero_ShouldReturnSingleValue()
        {
            GameplayConfig config = CreateConfig(targetExpressionMatchRatio: 0.20f);

            TargetExpressionMatchRatioRange range =
                new SectionDecliningTargetExpressionMatchRatioRangeResolver().Resolve(section: 10, config);

            Assert.That(range.IsSingleValue, Is.True);
            Assert.That(range.LowerBound, Is.EqualTo(0.20f));
        }

        /// <summary>
        /// 範囲を決める部品が、1未満のSectionとnullの設定を拒否することを検証します。
        /// </summary>
        [Test]
        public void Resolvers_ShouldRejectInvalidArguments()
        {
            var resolvers = new ITargetExpressionMatchRatioRangeResolver[]
            {
                new FixedTargetExpressionMatchRatioRangeResolver(),
                new SectionDecliningTargetExpressionMatchRatioRangeResolver()
            };

            foreach (ITargetExpressionMatchRatioRangeResolver resolver in resolvers)
            {
                Assert.That(
                    Assert.Throws<ArgumentOutOfRangeException>(() => resolver.Resolve(0, CreateConfig()))!.ParamName,
                    Is.EqualTo("section"));
                Assert.That(
                    Assert.Throws<ArgumentNullException>(() => resolver.Resolve(1, null!))!.ParamName,
                    Is.EqualTo("config"));
            }
        }

        /// <summary>
        /// Section低下版StrategyがMission抽選ポートとして提供されることを検証します。
        /// </summary>
        [Test]
        public void Strategy_ShouldImplementMissionDrawStrategyPort()
        {
            IMissionDrawStrategy strategy = CreateStrategy(new SequenceRandomSource());

            Assert.That(strategy, Is.InstanceOf<IMissionDrawStrategy>());
        }

        /// <summary>
        /// Sectionに応じた範囲から一様に選んだ目標式適合割合に近い候補を選ぶことを検証します。
        /// </summary>
        [TestCase(0d, 3)]
        [TestCase(0.99d, 5)]
        public void Draw_ShouldRankCandidatesByTargetSampledFromSectionRange(double targetSample, int expectedParameter)
        {
            var randomSource = new SequenceRandomSource(targetSample);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 3), matchCount: 3, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 4), matchCount: 4, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 5), matchCount: 5, evaluatedCount: 10)
            };
            GameplayConfig config = CreateConfig(
                targetExpressionMatchRatio: 0.50f,
                targetExpressionMatchRatioReductionPerSection: 0.10f,
                minimumTargetExpressionMatchRatio: 0.10f,
                missionParameterSelectionPoolSize: 1);

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates, config), 3));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.ParameterA, Is.EqualTo(expectedParameter));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { targetSample }));
        }

        /// <summary>
        /// 目標式適合割合の乱数をKind段階の後、Pool内の候補選択の前に1回だけ消費し、Traceへ範囲と選んだ値を報告することを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldConsumeTargetSampleBetweenKindAndCandidateStages()
        {
            var randomSource = new SequenceRandomSource(0.20d, 0.10d, 0.99d, 0d, 0.99d);
            var reporter = new RecordingDiagnosticsReporter();
            var strategy = CreateStrategy(randomSource, reporter);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Odd), tier: 1),
                CreateCandidate(MissionCondition.Create(MissionKind.Even), tier: 2),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd, isNegated: true), tier: 2),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 1), tier: 2, matchCount: 2, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 2), tier: 2, matchCount: 3, evaluatedCount: 10)
            };
            GameplayConfig config = CreateConfig(
                targetExpressionMatchRatio: 0.20f,
                targetExpressionMatchRatioReductionPerSection: 0.05f,
                minimumTargetExpressionMatchRatio: 0f);

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates, config), 4));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.ParameterA, Is.EqualTo(2));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.20d, 0.10d, 0.99d, 0d, 0.99d }));
            MissionDrawTrace trace = reporter.Traces.Single();
            Assert.That(trace.TargetExpressionMatchRatioRange.LowerBound, Is.EqualTo(0.05f).Within(1e-6f));
            Assert.That(trace.TargetExpressionMatchRatioRange.UpperBound, Is.EqualTo(0.20f));
            Assert.That(trace.TargetExpressionMatchRatio, Is.EqualTo(0.05f).Within(1e-6f));
            Assert.That(trace.SelectedRank, Is.EqualTo(2));
        }

        /// <summary>
        /// 範囲の幅が0のとき、目標式適合割合の乱数を消費せず本実装版と同じ候補と乱数消費になることを検証します。
        /// </summary>
        [TestCase(1, 0.10f)]
        [TestCase(5, 0f)]
        public void Draw_WhenRangeIsSingleValue_ShouldMatchFullStrategyWithoutTargetSample(
            int section,
            float reductionPerSection)
        {
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 1), matchCount: 1, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 2), matchCount: 2, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 3), matchCount: 3, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 8), matchCount: 8, evaluatedCount: 10)
            };
            GameplayConfig config = CreateConfig(
                targetExpressionMatchRatio: 0.20f,
                targetExpressionMatchRatioReductionPerSection: reductionPerSection,
                minimumTargetExpressionMatchRatio: 0f);
            MissionDrawContext context = MissionDrawContext.CreateForInitialMission(
                CreateSessionContext(candidates, config),
                section);
            var decliningRandomSource = new SequenceRandomSource(0.99d);
            var fullRandomSource = new SequenceRandomSource(0.99d);

            MissionDrawResult decliningResult = CreateStrategy(decliningRandomSource).Draw(context);
            MissionDrawResult fullResult = new FullMissionDrawStrategy(
                fullRandomSource,
                new RecordingDiagnosticsReporter()).Draw(context);

            Assert.That(decliningResult.IsSuccess, Is.True);
            Assert.That(decliningResult.Candidate, Is.SameAs(fullResult.Candidate));
            Assert.That(decliningRandomSource.ConsumedSamples, Is.EqualTo(fullRandomSource.ConsumedSamples));
        }

        /// <summary>
        /// 現在式で達成できる候補群が選ばれた場合も、Sectionに応じた範囲から選んだ目標式適合割合で順位付けすることを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_ShouldApplySectionRangeToSatisfiedCandidates()
        {
            var randomSource = new SequenceRandomSource(0.05d, 0d);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.AtLeast, 2), matchCount: 5, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.AtLeast, 4), matchCount: 3, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.AtLeast, 9), matchCount: 1, evaluatedCount: 10)
            };
            GameplayConfig config = CreateConfig(
                currentExpressionSatisfiedMissionSelectionRatio: 0.10f,
                targetExpressionMatchRatio: 0.50f,
                targetExpressionMatchRatioReductionPerSection: 0.10f,
                minimumTargetExpressionMatchRatio: 0.10f,
                missionParameterSelectionPoolSize: 1);

            MissionDrawResult result = strategy.Draw(MissionDrawContext.CreateForNextMission(
                CreateSessionContext(candidates, config),
                3,
                new Expression(2, OperatorType.Add, 3),
                new Mission(new MissionId(1), MissionCondition.Create(MissionKind.Exact, 9), 1)));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.ParameterA, Is.EqualTo(4));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.05d, 0d }));
        }

        private static SectionDecliningMatchRatioMissionDrawStrategy CreateStrategy(
            IRandomSource randomSource,
            IDiagnosticsReporter<MissionDrawTrace>? diagnosticsReporter = null)
        {
            return new SectionDecliningMatchRatioMissionDrawStrategy(
                randomSource,
                diagnosticsReporter ?? new RecordingDiagnosticsReporter());
        }

        private static GameplaySessionContext CreateSessionContext(
            IReadOnlyList<MissionCandidate> candidates,
            GameplayConfig config)
        {
            return new GameplaySessionContext(config, new MissionCatalog(candidates));
        }

        private static GameplayConfig CreateConfig(
            float currentExpressionSatisfiedMissionSelectionRatio = 0.10f,
            float targetExpressionMatchRatio = 0.20f,
            float targetExpressionMatchRatioReductionPerSection = 0f,
            float minimumTargetExpressionMatchRatio = 0f,
            int missionParameterSelectionPoolSize = 3)
        {
            return new GameplayConfig(
                minimumNumber: 1,
                maximumNumber: 5,
                initialExpression: new Expression(1, OperatorType.Add, 1),
                initialLife: 3,
                timer: new TimerConfig(
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(0.5),
                    TimeSpan.FromSeconds(3)),
                tierProgression: new TierProgression(),
                showCurrentAnswer: false,
                minimumExpressionMatchRatio: 0.10f,
                maximumExpressionMatchRatio: 0.60f,
                minimumExpressionMatchCount: 2,
                zeroAndPrimeMinimumExpressionMatchCount: 1,
                currentTierDrawWeight: 0.60f,
                lowerTierDrawWeight: 0.40f,
                currentExpressionSatisfiedMissionSelectionRatio: currentExpressionSatisfiedMissionSelectionRatio,
                targetExpressionMatchRatio: targetExpressionMatchRatio,
                targetExpressionMatchRatioReductionPerSection: targetExpressionMatchRatioReductionPerSection,
                minimumTargetExpressionMatchRatio: minimumTargetExpressionMatchRatio,
                negatedMissionSelectionWeight: 0.60f,
                missionParameterSelectionPoolSize: missionParameterSelectionPoolSize);
        }

        private static MissionCandidate CreateCandidate(
            MissionCondition condition,
            int tier = 1,
            int matchCount = 1,
            int evaluatedCount = 2)
        {
            var answerExpression = new Expression(1, OperatorType.Add, 1);
            return new MissionCandidate(
                condition,
                tier,
                matchCount,
                evaluatedCount,
                new ExpressionCandidate(
                    answerExpression,
                    ExpressionCalculator.Evaluate(answerExpression)));
        }

        private sealed class SequenceRandomSource : IRandomSource
        {
            private readonly Queue<double> _samples;
            private readonly List<double> _consumedSamples = new List<double>();

            public SequenceRandomSource(params double[] samples)
            {
                _samples = new Queue<double>(samples);
            }

            /// <summary>
            /// 消費されたsample列を取得します。
            /// </summary>
            public IReadOnlyList<double> ConsumedSamples => _consumedSamples;

            /// <summary>
            /// 固定sample列から次の値を返します。
            /// </summary>
            /// <returns>次のsample。</returns>
            public float NextFloat01()
            {
                if (_samples.Count == 0)
                {
                    throw new InvalidOperationException("The fixed sample sequence was exhausted.");
                }

                double sample = _samples.Dequeue();
                _consumedSamples.Add(sample);
                return (float)sample;
            }
        }

        private sealed class RecordingDiagnosticsReporter : IDiagnosticsReporter<MissionDrawTrace>
        {
            private readonly List<MissionDrawTrace> _traces = new List<MissionDrawTrace>();

            /// <summary>
            /// 常にTraceを受け付けます。
            /// </summary>
            public bool IsEnabled => true;

            /// <summary>
            /// 報告されたTraceを報告順に取得します。
            /// </summary>
            public IReadOnlyList<MissionDrawTrace> Traces => _traces;

            /// <summary>
            /// 報告されたTraceを記録します。
            /// </summary>
            /// <param name="trace">記録するTrace。</param>
            public void Report(MissionDrawTrace trace)
            {
                _traces.Add(trace);
            }
        }
    }
}
