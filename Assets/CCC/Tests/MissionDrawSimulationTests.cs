#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Ccc.Simulation;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// Mission抽選シミュレーションの乱数、シナリオ、実行処理の振る舞いを検証します。
    /// </summary>
    public sealed class MissionDrawSimulationTests
    {
        private const int Seed = 12345;
        private const int SampleCount = 100;

        /// <summary>
        /// 同じseedの乱数が同じ乱数列を0以上1未満で返すことを検証します。
        /// </summary>
        [Test]
        public void SimulationRandom_WithSameSeed_ShouldReproduceSequenceWithinRange()
        {
            var first = new SimulationRandom(Seed);
            var second = new SimulationRandom(Seed);

            float[] firstValues = Enumerable.Range(0, SampleCount).Select(_ => first.NextFloat01()).ToArray();
            float[] secondValues = Enumerable.Range(0, SampleCount).Select(_ => second.NextFloat01()).ToArray();

            Assert.That(secondValues, Is.EqualTo(firstValues));
            Assert.That(firstValues, Is.All.GreaterThanOrEqualTo(0f).And.LessThan(1f));
        }

        /// <summary>
        /// 整数選択が選択肢数1未満を拒否することを検証します。
        /// </summary>
        [Test]
        public void SimulationRandom_NextIndex_ShouldRejectNonPositiveCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationRandom(Seed).NextIndex(0));
        }

        /// <summary>
        /// 流れのシナリオが各Sessionを初回抽選から始め、Sectionを最大まで1ずつ進めることを検証します。
        /// </summary>
        [Test]
        public void SequentialFlow_ShouldStartEachSessionWithInitialDrawAndAdvanceSections()
        {
            MissionDrawScenarioEnvironment environment = CreateEnvironment();
            var scenario = new SequentialFlowScenario(environment, sessionCount: 2, maximumSection: 3);

            List<(MissionDrawContext Context, MissionDrawResult Result)> draws = DrawAll(scenario);

            Assert.That(draws.Select(draw => draw.Context.Section), Is.EqualTo(new[] { 1, 2, 3, 1, 2, 3 }));
            Assert.That(
                draws.Select(draw => draw.Context.IsInitialDraw),
                Is.EqualTo(new[] { true, false, false, true, false, false }));
        }

        /// <summary>
        /// 流れのシナリオの次回抽選が、直前の選出候補を直前Missionとし、それを満たす式を現在式にすることを検証します。
        /// </summary>
        [Test]
        public void SequentialFlow_NextDraw_ShouldUsePreviousCandidateAndSatisfyingExpression()
        {
            MissionDrawScenarioEnvironment environment = CreateEnvironment();
            var scenario = new SequentialFlowScenario(environment, sessionCount: 1, maximumSection: 5);

            List<(MissionDrawContext Context, MissionDrawResult Result)> draws = DrawAll(scenario);

            for (int index = 1; index < draws.Count; index++)
            {
                MissionCondition previousCondition = draws[index - 1].Result.Candidate.Condition;
                MissionDrawContext context = draws[index].Context;
                Assert.That(context.PreviousMission!.Condition, Is.EqualTo(previousCondition));
                Assert.That(
                    MissionConditionMatcher.IsMatch(
                        previousCondition,
                        ExpressionCalculator.Evaluate(context.CurrentExpression!.Value)),
                    Is.True);
            }
        }

        /// <summary>
        /// 流れのシナリオが抽選失敗後に次Sessionの初回抽選へ移ることを検証します。
        /// </summary>
        [Test]
        public void SequentialFlow_WhenDrawFails_ShouldStartNextSession()
        {
            MissionDrawScenarioEnvironment environment = CreateEnvironment();
            var scenario = new SequentialFlowScenario(environment, sessionCount: 2, maximumSection: 5);
            scenario.TryCreateNextContext(null, null, out MissionDrawContext firstContext);
            MissionDrawResult failure = CreateStrategy().Draw(null!);

            bool hasNext = scenario.TryCreateNextContext(firstContext, failure, out MissionDrawContext nextContext);

            Assert.That(failure.IsSuccess, Is.False);
            Assert.That(hasNext, Is.True);
            Assert.That(nextContext.IsInitialDraw, Is.True);
            Assert.That(nextContext.Section, Is.EqualTo(1));
        }

        /// <summary>
        /// 固定条件シナリオの初回抽選が指定Sectionで指定回数の初回抽選Contextを生成することを検証します。
        /// </summary>
        [Test]
        public void FixedCondition_InitialDraw_ShouldCreateInitialContextsAtSection()
        {
            var scenario = CreateFixedScenario(
                FixedConditionDrawKind.Initial,
                ScenarioValueSource.Fixed,
                ScenarioValueSource.Fixed,
                section: 4,
                drawCount: 3);

            List<(MissionDrawContext Context, MissionDrawResult Result)> draws = DrawAll(scenario);

            Assert.That(draws, Has.Count.EqualTo(3));
            Assert.That(draws.Select(draw => draw.Context.IsInitialDraw), Is.All.True);
            Assert.That(draws.Select(draw => draw.Context.Section), Is.All.EqualTo(4));
        }

        /// <summary>
        /// 固定条件シナリオの次回抽選が、固定した現在式と直前Missionを全抽選で使うことを検証します。
        /// </summary>
        [Test]
        public void FixedCondition_NextDrawWithFixedValues_ShouldReuseSameInputs()
        {
            var expression = new Expression(1, OperatorType.Add, 1);
            var scenario = CreateFixedScenario(
                FixedConditionDrawKind.Next,
                ScenarioValueSource.Fixed,
                ScenarioValueSource.Fixed,
                fixedExpression: expression,
                fixedPreviousMission: new MissionConditionSpec(MissionKind.Odd, 0, 0, false));

            List<(MissionDrawContext Context, MissionDrawResult Result)> draws = DrawAll(scenario);

            Assert.That(draws.Select(draw => draw.Context.CurrentExpression), Is.All.EqualTo(expression));
            Assert.That(
                draws.Select(draw => draw.Context.PreviousMission!.Condition),
                Is.All.EqualTo(MissionCondition.Create(MissionKind.Odd)));
        }

        /// <summary>
        /// 固定条件シナリオの次回抽選が、毎回選ぶ現在式と直前Missionを候補式とCatalogの中から選ぶことを検証します。
        /// </summary>
        [Test]
        public void FixedCondition_NextDrawWithRandomValues_ShouldPickFromCandidates()
        {
            MissionDrawScenarioEnvironment environment = CreateEnvironment();
            var scenario = new FixedConditionScenario(
                environment,
                section: 1,
                drawCount: SampleCount,
                FixedConditionDrawKind.Next,
                ScenarioValueSource.RandomPerDraw,
                default,
                ScenarioValueSource.RandomPerDraw,
                default);

            List<(MissionDrawContext Context, MissionDrawResult Result)> draws = DrawAll(scenario);

            Expression[] candidateExpressions = environment.DefinedExpressionCandidates
                .Select(candidate => candidate.Expression)
                .ToArray();
            MissionCondition[] catalogConditions = environment.SessionContext.MissionCatalog.Candidates
                .Select(candidate => candidate.Condition)
                .ToArray();
            Assert.That(
                draws.Select(draw => draw.Context.CurrentExpression!.Value),
                Is.All.Matches<Expression>(expression => candidateExpressions.Contains(expression)));
            Assert.That(
                draws.Select(draw => draw.Context.PreviousMission!.Condition),
                Is.All.Matches<MissionCondition>(condition => catalogConditions.Contains(condition)));
            Assert.That(draws.Select(draw => draw.Context.CurrentExpression).Distinct().Count(), Is.GreaterThan(1));
        }

        /// <summary>
        /// 固定条件シナリオが毎回選ぶ直前Missionを、SectionのCurrent Tier以下で解放済みの候補に限ることを検証します。
        /// </summary>
        [Test]
        public void FixedCondition_RandomPreviousMission_ShouldPickOnlyUnlockedTierCandidates()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            var rules = new[]
            {
                new MissionKindGenerationRule(MissionKind.Even, true, true, false, SampleCount, null, null),
                new MissionKindGenerationRule(MissionKind.Positive, true, false, true, SampleCount, null, null)
            };
            MissionCatalogPreparationResult preparation = MissionCatalogPreparer.Prepare(
                config,
                MissionCatalogPreparationSettings.CreatePresetDebug(
                    new MissionParameterGenerationSettings(rules, SampleCount, Rational.FromSingle(0.25f)),
                    MissionTestSettings.CreateEvaluationPolicy(config),
                    new[]
                    {
                        new MissionParameterCandidate(MissionCondition.Create(MissionKind.Even)),
                        new MissionParameterCandidate(MissionCondition.Create(MissionKind.Positive, isNegated: true))
                    }));
            var environment = new MissionDrawScenarioEnvironment(
                new GameplaySessionContext(config, preparation.Catalog),
                preparation.ExpressionCandidates,
                new SimulationRandom(Seed));
            var scenario = new FixedConditionScenario(
                environment,
                section: 1,
                drawCount: SampleCount,
                FixedConditionDrawKind.Next,
                ScenarioValueSource.RandomPerDraw,
                default,
                ScenarioValueSource.RandomPerDraw,
                default);

            List<(MissionDrawContext Context, MissionDrawResult Result)> draws = DrawAll(scenario);

            Assert.That(preparation.Catalog.Candidates.Select(candidate => candidate.Tier), Has.Some.GreaterThan(1));
            Assert.That(draws.Select(draw => draw.Context.PreviousMission!.Tier), Is.All.EqualTo(1));
        }

        /// <summary>
        /// 固定条件シナリオが候補式にない現在式と、Catalogにない直前Missionを拒否することを検証します。
        /// </summary>
        [Test]
        public void FixedCondition_ShouldRejectInputsOutsideCandidates()
        {
            Assert.Throws<ArgumentException>(() => CreateFixedScenario(
                FixedConditionDrawKind.Next,
                ScenarioValueSource.Fixed,
                ScenarioValueSource.RandomPerDraw,
                fixedExpression: new Expression(99, OperatorType.Add, 99)));
            Assert.Throws<ArgumentException>(() => CreateFixedScenario(
                FixedConditionDrawKind.Next,
                ScenarioValueSource.RandomPerDraw,
                ScenarioValueSource.Fixed,
                fixedPreviousMission: new MissionConditionSpec(MissionKind.Prime, 0, 0, false)));
        }

        /// <summary>
        /// 同じseedと入力のシミュレーションが同じ選出列を再現し、全抽選を診断Reporterへ報告することを検証します。
        /// </summary>
        [Test]
        public void Run_WithSameSeed_ShouldReproduceSelectionsAndReportEveryDraw()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionCatalogPreparationSettings settings = CreatePreparationSettings(config);
            var firstReporter = new RecordingReporter();
            var secondReporter = new RecordingReporter();

            MissionDrawSimulationSummary firstSummary = MissionDrawSimulation.Run(
                config,
                settings,
                Seed,
                environment => new SequentialFlowScenario(environment, sessionCount: 3, maximumSection: 4),
                firstReporter);
            MissionDrawSimulation.Run(
                config,
                settings,
                Seed,
                environment => new SequentialFlowScenario(environment, sessionCount: 3, maximumSection: 4),
                secondReporter);

            Assert.That(firstSummary.DrawCount, Is.EqualTo(12));
            Assert.That(firstSummary.FailureCount, Is.EqualTo(0));
            Assert.That(firstReporter.Traces, Has.Count.EqualTo(12));
            Assert.That(
                secondReporter.Traces.Select(trace => trace.SelectedCandidate.Condition),
                Is.EqualTo(firstReporter.Traces.Select(trace => trace.SelectedCandidate.Condition)));
        }

        /// <summary>
        /// MissionCatalogの準備に失敗した場合に例外を送出することを検証します。
        /// </summary>
        [Test]
        public void Run_WhenCatalogPreparationFails_ShouldThrow()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            var rules = new[]
            {
                new MissionKindGenerationRule(MissionKind.Even, true, true, false, SampleCount, null, null),
                new MissionKindGenerationRule(MissionKind.Odd, false, true, false, SampleCount, null, null)
            };
            MissionCatalogPreparationSettings settings = MissionCatalogPreparationSettings.CreatePresetDebug(
                new MissionParameterGenerationSettings(rules, SampleCount, Rational.FromSingle(0.25f)),
                MissionTestSettings.CreateEvaluationPolicy(config),
                new[] { new MissionParameterCandidate(MissionCondition.Create(MissionKind.Odd)) });

            Assert.Throws<InvalidOperationException>(() => MissionDrawSimulation.Run(
                config,
                settings,
                Seed,
                environment => new SequentialFlowScenario(environment, sessionCount: 1, maximumSection: 1),
                new RecordingReporter()));
        }

        private static MissionDrawScenarioEnvironment CreateEnvironment()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();
            MissionCatalogPreparationResult preparation = MissionCatalogPreparer.Prepare(
                config,
                CreatePreparationSettings(config));
            return new MissionDrawScenarioEnvironment(
                new GameplaySessionContext(config, preparation.Catalog),
                preparation.ExpressionCandidates,
                new SimulationRandom(Seed));
        }

        private static MissionCatalogPreparationSettings CreatePreparationSettings(GameplayConfig config)
        {
            return MissionTestSettings.CreatePresetPreparationSettings(
                new[]
                {
                    new MissionParameterCandidate(MissionCondition.Create(MissionKind.Even)),
                    new MissionParameterCandidate(MissionCondition.Create(MissionKind.Odd))
                },
                config);
        }

        private static FixedConditionScenario CreateFixedScenario(
            FixedConditionDrawKind drawKind,
            ScenarioValueSource expressionSource,
            ScenarioValueSource previousMissionSource,
            int section = 1,
            int drawCount = 5,
            Expression fixedExpression = default,
            MissionConditionSpec fixedPreviousMission = default)
        {
            return new FixedConditionScenario(
                CreateEnvironment(),
                section,
                drawCount,
                drawKind,
                expressionSource,
                fixedExpression,
                previousMissionSource,
                fixedPreviousMission);
        }

        private static FullMissionDrawStrategy CreateStrategy()
        {
            return new FullMissionDrawStrategy(new SimulationRandom(Seed), new RecordingReporter());
        }

        private static List<(MissionDrawContext Context, MissionDrawResult Result)> DrawAll(IMissionDrawScenario scenario)
        {
            FullMissionDrawStrategy strategy = CreateStrategy();
            var draws = new List<(MissionDrawContext Context, MissionDrawResult Result)>();
            MissionDrawContext? previousContext = null;
            MissionDrawResult? previousResult = null;
            while (scenario.TryCreateNextContext(previousContext, previousResult, out MissionDrawContext context))
            {
                MissionDrawResult result = strategy.Draw(context);
                Assert.That(result.IsSuccess, Is.True, result.IsSuccess ? string.Empty : result.FailureMessage);
                draws.Add((context, result));
                previousContext = context;
                previousResult = result;
            }

            return draws;
        }

        private sealed class RecordingReporter : IDiagnosticsReporter<MissionDrawTrace>
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
