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
    /// 固定sampleと順序を記録する乱数源で、SelectorとMission抽選の決定性を検証します。
    /// </summary>
    public sealed class SelectorAndMissionDrawTests
    {
        /// <summary>
        /// 固定sampleが一様Selectorと重み付きSelectorの境界を正しく選ぶことを検証します。
        /// </summary>
        [Test]
        public void Selectors_ShouldUseFixedSample()
        {
            Assert.That(UniformIndexSelector.SelectIndex(4, 0.00d), Is.EqualTo(0));
            Assert.That(UniformIndexSelector.SelectIndex(4, 0.99d), Is.EqualTo(3));
            Assert.That(WeightedIndexSelector.SelectIndex(new[] { 0.60f, 0.40f }, 0.599999d), Is.EqualTo(0));
            Assert.That(WeightedIndexSelector.SelectIndex(new[] { 0.60f, 0.40f }, 0.60d), Is.EqualTo(1));
        }

        /// <summary>
        /// 選択肢が1件だけの抽選ではsampleを消費しないことを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldNotConsumeRandomWhenEveryChoiceHasOneOption()
        {
            var randomSource = new SequenceRandomSource();
            var strategy = new SimpleMissionDrawStrategy(randomSource);
            var result = strategy.Draw(CreateContext(new[] { CreateCandidate(MissionKind.Even, 1, 1) }));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.Kind, Is.EqualTo(MissionKind.Even));
            Assert.That(randomSource.ConsumedSamples, Is.Empty);
        }

        /// <summary>
        /// Mission抽選がTier、Kind、パラメータの順にsampleを消費することを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldConsumeSamplesInTierKindParameterOrder()
        {
            var randomSource = new SequenceRandomSource(0.50d, 0.99d, 0.99d);
            var candidates = new[]
            {
                CreateCandidate(MissionKind.Odd, 1, 1),
                CreateCandidate(MissionKind.Even, 2, 1),
                CreateCandidate(MissionKind.Exact, 2, 1),
                CreateCandidate(MissionKind.Exact, 2, 2)
            };

            var result = new SimpleMissionDrawStrategy(randomSource).Draw(CreateContext(candidates, section: 4));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.Kind, Is.EqualTo(MissionKind.Exact));
            Assert.That(result.Candidate.Condition.ParameterA, Is.EqualTo(2));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.50d, 0.99d, 0.99d }));
        }

        /// <summary>
        /// 現在Tierに候補がない場合は、全下位Tierの候補を1つの候補群としてKindを一様抽選することを検証します。
        /// </summary>
        [Test]
        public void Draw_WhenCurrentTierHasNoCandidate_ShouldSelectKindUniformlyAcrossAllLowerTiers()
        {
            var randomSource = new SequenceRandomSource(0.99d);
            var candidates = new[]
            {
                CreateCandidate(MissionKind.Even, 1, 1),
                CreateCandidate(MissionKind.Exact, 1, 1),
                CreateCandidate(MissionKind.Odd, 2, 1)
            };

            MissionDrawResult result = new SimpleMissionDrawStrategy(randomSource)
                .Draw(CreateContext(candidates, section: 8));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.Kind, Is.EqualTo(MissionKind.Exact));
            Assert.That(result.Candidate.Tier, Is.EqualTo(1));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.99d }));
        }

        /// <summary>
        /// 重み抽選で下位Tierが選ばれた場合は、全下位Tierの候補を1つの候補群としてKindを一様抽選することを検証します。
        /// </summary>
        [Test]
        public void Draw_WhenLowerTierIsSelected_ShouldSelectKindUniformlyAcrossAllLowerTiers()
        {
            var randomSource = new SequenceRandomSource(0.70d, 0.99d);
            var candidates = new[]
            {
                CreateCandidate(MissionKind.Even, 2, 1),
                CreateCandidate(MissionKind.Odd, 1, 1),
                CreateCandidate(MissionKind.Exact, 3, 1)
            };

            MissionDrawResult result = new SimpleMissionDrawStrategy(randomSource)
                .Draw(CreateContext(candidates, section: 8));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.Kind, Is.EqualTo(MissionKind.Odd));
            Assert.That(result.Candidate.Tier, Is.EqualTo(1));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.70d, 0.99d }));
        }

        /// <summary>
        /// 同一のsample列が同一のMission抽選結果を再現することを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldBeReproducibleForTheSameSampleSequence()
        {
            var candidates = new[]
            {
                CreateCandidate(MissionKind.Odd, 1, 1),
                CreateCandidate(MissionKind.Even, 2, 1),
                CreateCandidate(MissionKind.Exact, 2, 1),
                CreateCandidate(MissionKind.Exact, 2, 2)
            };
            var first = new SimpleMissionDrawStrategy(new SequenceRandomSource(0.20d, 0.10d, 0.90d))
                .Draw(CreateContext(candidates, section: 4));
            var second = new SimpleMissionDrawStrategy(new SequenceRandomSource(0.20d, 0.10d, 0.90d))
                .Draw(CreateContext(candidates, section: 4));

            Assert.That(second.Candidate.Condition, Is.EqualTo(first.Candidate.Condition));
            Assert.That(second.Candidate.Tier, Is.EqualTo(first.Candidate.Tier));
        }

        private static MissionDrawContext CreateContext(
            IReadOnlyList<MissionCandidate> candidates,
            int section = 1)
        {
            var config = GameplayConfigDefaults.CreateNormal();
            var catalog = new MissionCatalog(candidates);
            return MissionDrawContext.CreateForInitialMission(
                new GameplaySessionContext(config, catalog),
                section);
        }

        private static MissionCandidate CreateCandidate(MissionKind kind, int tier, int parameter)
        {
            var condition = kind == MissionKind.Exact
                ? MissionCondition.Create(kind, parameter)
                : MissionCondition.Create(kind);
            var expression = new Expression(1, OperatorType.Add, 1);
            return new MissionCandidate(condition, tier, 1, 2, new ExpressionCandidate(expression, ExpressionCalculator.Evaluate(expression)));
        }

        private sealed class SequenceRandomSource : IRandomSource
        {
            private readonly Queue<double> _samples;

            public SequenceRandomSource(params double[] samples)
            {
                _samples = new Queue<double>(samples);
                ConsumedSamples = new List<double>();
            }

            /// <summary>
            /// 抽選処理が取得したsampleの順序を取得します。
            /// </summary>
            public IReadOnlyList<double> ConsumedSamples { get; }

            /// <summary>
            /// 固定sample列から次のsampleを取得します。
            /// </summary>
            /// <returns>次のsample。</returns>
            public float NextFloat01()
            {
                if (_samples.Count == 0)
                {
                    throw new InvalidOperationException("The fixed sample sequence was exhausted.");
                }

                var sample = _samples.Dequeue();
                ((List<double>)ConsumedSamples).Add(sample);
                return (float)sample;
            }
        }
    }
}
