#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using NUnit.Framework;
using R3;

namespace Ccc.Tests
{
    /// <summary>
    /// 本実装版Mission抽選の振る舞いを検証します。
    /// </summary>
    public sealed class FullMissionDrawStrategyTests
    {
        /// <summary>
        /// 本実装版StrategyがMission抽選ポートとして提供されることを検証します。
        /// </summary>
        [Test]
        public void FullMissionDrawStrategy_ShouldImplementMissionDrawStrategyPort()
        {
            var strategyType = typeof(IMissionDrawStrategy).Assembly.GetType(
                "Ccc.Core.UseCases.FullMissionDrawStrategy");

            Assert.That(strategyType, Is.Not.Null);
            Assert.That(typeof(IMissionDrawStrategy).IsAssignableFrom(strategyType), Is.True);
        }

        /// <summary>
        /// 初回抽選Contextが直前Mission情報を持たないことを検証します。
        /// </summary>
        [Test]
        public void CreateForInitialMission_ShouldHaveNoPreviousMissionState()
        {
            var context = MissionDrawContext.CreateForInitialMission(
                CreateSessionContext(new[] { CreateCandidate(MissionCondition.Create(MissionKind.Even)) }),
                1);

            Assert.That(context.IsInitialDraw, Is.True);
            Assert.That(context.CurrentExpression, Is.Null);
            Assert.That(context.PreviousMission, Is.Null);
        }

        /// <summary>
        /// 次回抽選Contextが現在式と直前Missionを対で保持することを検証します。
        /// </summary>
        [Test]
        public void CreateForNextMission_ShouldExposeCurrentExpressionAndPreviousMission()
        {
            var currentExpression = new Expression(1, OperatorType.Add, 1);
            var previousMission = CreateMission(MissionCondition.Create(MissionKind.Odd));
            var context = MissionDrawContext.CreateForNextMission(
                CreateSessionContext(new[] { CreateCandidate(MissionCondition.Create(MissionKind.Even)) }),
                2,
                currentExpression,
                previousMission);

            Assert.That(context.IsInitialDraw, Is.False);
            Assert.That(context.CurrentExpression, Is.EqualTo(currentExpression));
            Assert.That(context.PreviousMission, Is.SameAs(previousMission));
        }

        /// <summary>
        /// 次回抽選Contextが直前Missionなしでは生成できないことを検証します。
        /// </summary>
        [Test]
        public void CreateForNextMission_ShouldRejectMissingPreviousMission()
        {
            Assert.Throws<ArgumentNullException>(() =>
                MissionDrawContext.CreateForNextMission(
                    CreateSessionContext(new[]
                    {
                        CreateCandidate(MissionCondition.Create(MissionKind.Even))
                    }),
                    2,
                    new Expression(1, OperatorType.Add, 1),
                    null!));
        }

        /// <summary>
        /// 初回抽選では初期式で達成済みのMissionも抽選対象になることを検証します。
        /// </summary>
        [Test]
        public void DrawInitialMission_ShouldNotFilterSatisfiedCandidate()
        {
            var randomSource = new SequenceRandomSource(0d);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even)),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd))
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.Kind, Is.EqualTo(MissionKind.Even));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0d }));
        }

        /// <summary>
        /// 次回抽選で設定割合以上のsampleなら現在式で未達成のMissionを選択することを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_ShouldSelectUnmatchedCandidateOutsideConfiguredRatio()
        {
            var randomSource = new SequenceRandomSource(0.10d);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even)),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd))
            };

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                candidates,
                new Expression(1, OperatorType.Add, 1),
                CreateMission(MissionCondition.Create(MissionKind.Exact, 9))));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.Kind, Is.EqualTo(MissionKind.Odd));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.10d }));
        }

        /// <summary>
        /// 次回抽選で設定割合内のsampleなら現在式で達成済みのMissionを選択することを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_ShouldSelectSatisfiedCandidateWithinConfiguredRatio()
        {
            var randomSource = new SequenceRandomSource(0.09d);
            var strategy = CreateStrategy(randomSource);
            var satisfiedCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Even));
            var candidates = new[]
            {
                satisfiedCandidate,
                CreateCandidate(MissionCondition.Create(MissionKind.Odd))
            };

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                candidates,
                new Expression(1, OperatorType.Add, 1),
                CreateMission(MissionCondition.Create(MissionKind.Exact, 9))));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(satisfiedCandidate));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.09d }));
        }

        /// <summary>
        /// 設定した割合が現在式で達成済みのMission候補群を選ぶ境界として使われることを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_ShouldUseConfiguredSatisfiedMissionSelectionRatio()
        {
            var randomSource = new SequenceRandomSource(0.24d);
            var strategy = CreateStrategy(randomSource);
            var satisfiedCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Even));
            GameplayConfig config = CreateConfig(
                currentExpressionSatisfiedMissionSelectionRatio: 0.25f);

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                new[]
                {
                    satisfiedCandidate,
                    CreateCandidate(MissionCondition.Create(MissionKind.Odd))
                },
                new Expression(1, OperatorType.Add, 1),
                CreateMission(MissionCondition.Create(MissionKind.Exact, 9)),
                config));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(satisfiedCandidate));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.24d }));
        }

        /// <summary>
        /// 現在式で達成済みのMission選択割合が0なら未達成候補を乱数消費なしで選ぶことを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_WhenSatisfiedSelectionRatioIsZero_ShouldSelectUnmatchedWithoutRandom()
        {
            var randomSource = new SequenceRandomSource();
            var strategy = CreateStrategy(randomSource);
            var unmatchedCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Odd));
            GameplayConfig config = CreateConfig(
                currentExpressionSatisfiedMissionSelectionRatio: 0f);

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                new[]
                {
                    CreateCandidate(MissionCondition.Create(MissionKind.Even)),
                    unmatchedCandidate
                },
                new Expression(1, OperatorType.Add, 1),
                CreateMission(MissionCondition.Create(MissionKind.Exact, 9)),
                config));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(unmatchedCandidate));
            Assert.That(randomSource.ConsumedSamples, Is.Empty);
        }

        /// <summary>
        /// 現在式で達成済みのMission選択割合が1なら達成済み候補を乱数消費なしで選ぶことを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_WhenSatisfiedSelectionRatioIsOne_ShouldSelectSatisfiedWithoutRandom()
        {
            var randomSource = new SequenceRandomSource();
            var strategy = CreateStrategy(randomSource);
            var satisfiedCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Even));
            GameplayConfig config = CreateConfig(
                currentExpressionSatisfiedMissionSelectionRatio: 1f);

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                new[]
                {
                    satisfiedCandidate,
                    CreateCandidate(MissionCondition.Create(MissionKind.Odd))
                },
                new Expression(1, OperatorType.Add, 1),
                CreateMission(MissionCondition.Create(MissionKind.Exact, 9)),
                config));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(satisfiedCandidate));
            Assert.That(randomSource.ConsumedSamples, Is.Empty);
        }

        /// <summary>
        /// 解放済み候補が現在式で達成できるMissionだけなら乱数消費なしで選択することを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_WhenOnlySatisfiedCandidateExists_ShouldSelectItWithoutRandom()
        {
            var strategy = CreateStrategy(new SequenceRandomSource());
            var candidate = CreateCandidate(MissionCondition.Create(MissionKind.Even));

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                new[] { candidate },
                new Expression(1, OperatorType.Add, 1),
                CreateMission(MissionCondition.Create(MissionKind.Odd))));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(candidate));
        }

        /// <summary>
        /// 未解放Tierの未達成候補を候補群の有無の判定に含めないことを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_ShouldIgnoreLockedTierWhenPartitioningByCurrentExpression()
        {
            var strategy = CreateStrategy(new SequenceRandomSource());
            var unlockedSatisfiedCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Even),
                tier: 1);
            var lockedUnmatchedCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Odd),
                tier: 2);

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                new[] { unlockedSatisfiedCandidate, lockedUnmatchedCandidate },
                new Expression(1, OperatorType.Add, 1),
                CreateMission(MissionCondition.Create(MissionKind.Exact, 9)),
                section: 1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(unlockedSatisfiedCandidate));
        }

        /// <summary>
        /// 未定義の現在式はいかなるMissionも達成済みとして扱わないことを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_ShouldTreatUndefinedExpressionAsUnmatchedForNegatedCandidate()
        {
            var strategy = CreateStrategy(new SequenceRandomSource());
            var negatedCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Even, isNegated: true));

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                new[] { negatedCandidate },
                new Expression(1, OperatorType.Divide, 0),
                CreateMission(MissionCondition.Create(MissionKind.Odd))));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(negatedCandidate));
        }

        /// <summary>
        /// 肯定MissionとNegatedMissionを相対重みで選択することを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldSelectNegatedCandidateByConfiguredWeight()
        {
            var randomSource = new SequenceRandomSource(0.70d);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even)),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd, isNegated: true))
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.IsNegated, Is.True);
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.70d }));
        }

        /// <summary>
        /// 現在Tierと下位TierをConfigの相対重みで選択することを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldSelectLowerTierByConfiguredWeight()
        {
            var randomSource = new SequenceRandomSource(0.70d);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Odd), tier: 1),
                CreateCandidate(MissionCondition.Create(MissionKind.Even), tier: 2)
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 4));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Tier, Is.EqualTo(1));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.70d }));
        }

        /// <summary>
        /// 現在Tierに候補がない場合は全下位Tierの候補を1つの候補群とし、Kindを一様抽選することを検証します。
        /// </summary>
        [Test]
        public void Draw_WhenCurrentTierHasNoCandidate_ShouldSelectKindUniformlyAcrossAllLowerTiers()
        {
            var randomSource = new SequenceRandomSource(0.40d);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even), tier: 1),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd), tier: 2),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 1), tier: 2)
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 8));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.Kind, Is.EqualTo(MissionKind.Odd));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.40d }));
        }

        /// <summary>
        /// 重み抽選で下位Tierが選ばれた場合は全下位Tierの候補を1つの候補群とし、Kindを一様抽選することを検証します。
        /// </summary>
        [Test]
        public void Draw_WhenLowerTierIsSelected_ShouldSelectKindUniformlyAcrossAllLowerTiers()
        {
            var randomSource = new SequenceRandomSource(0.70d, 0.99d);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even), tier: 2),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd), tier: 1),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 1), tier: 3)
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 8));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.Kind, Is.EqualTo(MissionKind.Odd));
            Assert.That(result.Candidate.Tier, Is.EqualTo(1));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.70d, 0.99d }));
        }

        /// <summary>
        /// 下位Tierが選ばれた場合、TraceのSelectedTierに最終的に選出された候補のTierを記録することを検証します。
        /// </summary>
        [Test]
        public void Draw_WhenLowerTierIsSelected_ShouldReportSelectedCandidateTier()
        {
            var randomSource = new SequenceRandomSource(0.70d, 0.99d);
            var reporter = new RecordingDiagnosticsReporter(isEnabled: true);
            var strategy = CreateStrategy(randomSource, reporter);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even), tier: 2),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd), tier: 1),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 1), tier: 3)
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 8));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(reporter.Traces, Has.Count.EqualTo(1));
            Assert.That(reporter.Traces[0].SelectedTier, Is.EqualTo(1));
            Assert.That(reporter.Traces[0].TierStageCandidateCount, Is.EqualTo(2));
        }

        /// <summary>
        /// Negated抽選重み0ではNegated候補を完全に無効化することを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldFailWhenOnlyNegatedCandidatesExistAndWeightIsZero()
        {
            var strategy = CreateStrategy(new SequenceRandomSource());
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even, isNegated: true))
            };
            GameplayConfig config = CreateConfig(negatedMissionSelectionWeight: 0f);

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates, config), 1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.FailureMessage, Does.Contain("Negated"));
        }

        /// <summary>
        /// Negated抽選重み0では肯定候補がある限りNegated候補を選択対象から除外することを検証します。
        /// </summary>
        [Test]
        public void Draw_WhenNegatedWeightIsZero_ShouldSelectOnlyPositiveCandidate()
        {
            var randomSource = new SequenceRandomSource();
            var strategy = CreateStrategy(randomSource);
            var positiveCandidate = CreateCandidate(MissionCondition.Create(MissionKind.Even));
            var candidates = new[]
            {
                positiveCandidate,
                CreateCandidate(MissionCondition.Create(MissionKind.Odd, isNegated: true))
            };
            GameplayConfig config = CreateConfig(negatedMissionSelectionWeight: 0f);

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates, config), 1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(positiveCandidate));
            Assert.That(randomSource.ConsumedSamples, Is.Empty);
        }

        /// <summary>
        /// 代替候補がある場合は直前と同一条件の連続出題を回避することを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_ShouldExcludePreviousConditionWhenAlternativeExists()
        {
            var strategy = CreateStrategy(new SequenceRandomSource());
            var evenCandidate = CreateCandidate(MissionCondition.Create(MissionKind.Even));
            var oddCandidate = CreateCandidate(MissionCondition.Create(MissionKind.Odd));

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                new[] { evenCandidate, oddCandidate },
                new Expression(1, OperatorType.Divide, 2),
                CreateMission(evenCandidate.Condition)));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(oddCandidate));
        }

        /// <summary>
        /// 直前と同一条件しかない場合は連続出題回避を緩和することを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_ShouldKeepPreviousConditionWhenNoAlternativeExists()
        {
            var strategy = CreateStrategy(new SequenceRandomSource());
            var candidate = CreateCandidate(MissionCondition.Create(MissionKind.Even));

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                new[] { candidate },
                new Expression(1, OperatorType.Divide, 2),
                CreateMission(candidate.Condition)));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(candidate));
        }

        /// <summary>
        /// Negatedの有無が異なる条件は連続出題回避上の別Missionとして扱うことを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_ShouldTreatDifferentNegatedStateAsDifferentCondition()
        {
            var strategy = CreateStrategy(new SequenceRandomSource());
            var negatedCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Even, isNegated: true));

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                new[] { negatedCandidate },
                new Expression(1, OperatorType.Divide, 2),
                CreateMission(MissionCondition.Create(MissionKind.Even))));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate, Is.SameAs(negatedCandidate));
        }

        /// <summary>
        /// Target適合率に近い上位Config件数だけを最終抽選プールにすることを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldSelectFromCandidatesClosestToTargetMatchRatio()
        {
            var randomSource = new SequenceRandomSource(0.99d);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 1), matchCount: 1, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 2), matchCount: 2, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 3), matchCount: 3, evaluatedCount: 10),
                CreateCandidate(MissionCondition.Create(MissionKind.Exact, 8), matchCount: 8, evaluatedCount: 10)
            };
            GameplayConfig config = CreateConfig(
                targetExpressionMatchRatio: 0.20f,
                missionParameterSelectionPoolSize: 3);

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates, config), 1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.ParameterA, Is.EqualTo(3));
            Assert.That(randomSource.ConsumedSamples, Is.EqualTo(new[] { 0.99d }));
        }

        /// <summary>
        /// 乱数源が契約範囲外の値を返した場合に失敗Resultとなることを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldFailWhenRandomSourceReturnsOutOfRangeValue()
        {
            var strategy = CreateStrategy(new SequenceRandomSource(1d));
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even)),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd))
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.FailureMessage, Does.Contain("[0, 1)"));
        }

        /// <summary>
        /// 解放済みTierに使用可能候補がない場合に失敗Resultとなることを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldFailWhenNoCandidateExistsInUnlockedTier()
        {
            var strategy = CreateStrategy(new SequenceRandomSource());
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even), tier: 2)
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.FailureMessage, Does.Contain("No mission candidates"));
        }

        /// <summary>
        /// 本実装版抽選がTier、極性、Kind、候補の順に乱数を消費することを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldConsumeSamplesInFullSelectionOrder()
        {
            var randomSource = new SequenceRandomSource(0.20d, 0.10d, 0.99d, 0.99d);
            var strategy = CreateStrategy(randomSource);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Odd), tier: 1),
                CreateCandidate(MissionCondition.Create(MissionKind.Even), tier: 2),
                CreateCandidate(
                    MissionCondition.Create(MissionKind.Odd, isNegated: true),
                    tier: 2),
                CreateCandidate(
                    MissionCondition.Create(MissionKind.Exact, 1),
                    tier: 2,
                    matchCount: 2,
                    evaluatedCount: 10),
                CreateCandidate(
                    MissionCondition.Create(MissionKind.Exact, 2),
                    tier: 2,
                    matchCount: 3,
                    evaluatedCount: 10)
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 4));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Candidate.Condition.Kind, Is.EqualTo(MissionKind.Exact));
            Assert.That(result.Candidate.Condition.ParameterA, Is.EqualTo(2));
            Assert.That(
                randomSource.ConsumedSamples,
                Is.EqualTo(new[] { 0.20d, 0.10d, 0.99d, 0.99d }));
        }

        /// <summary>
        /// 同一Catalog、Context、sample列から同一結果と同一乱数消費順を再現することを検証します。
        /// </summary>
        [Test]
        public void Draw_ShouldBeReproducibleForTheSameContextAndSampleSequence()
        {
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even)),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd))
            };
            MissionDrawContext context = MissionDrawContext.CreateForInitialMission(
                CreateSessionContext(candidates),
                1);
            var firstRandomSource = new SequenceRandomSource(0.75d);
            var secondRandomSource = new SequenceRandomSource(0.75d);

            MissionDrawResult firstResult = CreateStrategy(firstRandomSource).Draw(context);
            MissionDrawResult secondResult = CreateStrategy(secondRandomSource).Draw(context);

            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(secondResult.IsSuccess, Is.True);
            Assert.That(secondResult.Candidate.Condition, Is.EqualTo(firstResult.Candidate.Condition));
            Assert.That(secondResult.Candidate.Tier, Is.EqualTo(firstResult.Candidate.Tier));
            Assert.That(secondRandomSource.ConsumedSamples, Is.EqualTo(firstRandomSource.ConsumedSamples));
        }

        /// <summary>
        /// Controllerが初回は全候補、2問目以降は引き継いだ回答式を使って抽選することを検証します。
        /// </summary>
        [Test]
        public void GameplayController_ShouldUseInitialAndNextMissionDrawContexts()
        {
            GameplayConfig config = CreateConfig();
            var randomSource = new SequenceRandomSource(0d, 0.10d);
            using var timer = new FakeSectionTimer();
            var states = new List<GameplayState>();
            using var controller = new GameplayController(
                config,
                new ExpressionInput(config),
                timer,
                CreateStrategy(randomSource));
            using IDisposable subscription = controller.States.Subscribe(states.Add);
            MissionParameterCandidate[] candidates =
            {
                new MissionParameterCandidate(MissionCondition.Create(MissionKind.Even)),
                new MissionParameterCandidate(MissionCondition.Create(MissionKind.Odd))
            };
            var request = new GameplayStartRequest(
                GameMode.LifeEndless,
                GameDifficulty.Normal,
                MissionTestSettings.CreatePresetPreparationSettings(candidates, config));

            GameplayStartResult startResult = controller.StartSession(request);
            Assert.That(startResult.IsSuccess, Is.True);
            MissionId initialMissionId = startResult.InitialState.CurrentMission.Id;
            controller.Submit(initialMissionId);
            controller.NotifyFeedbackCompleted(initialMissionId);

            Assert.That(startResult.InitialState.CurrentMission.Condition.Kind, Is.EqualTo(MissionKind.Even));
            Assert.That(states.Last().CurrentMission.Condition.Kind, Is.EqualTo(MissionKind.Odd));
            Assert.That(states.Last().CurrentMission.Id.Value, Is.EqualTo(2));
        }

        /// <summary>
        /// Controllerが回答の成否にかかわらず提出式と直前Missionを次回抽選Contextへ渡すことを検証します。
        /// </summary>
        /// <param name="shouldSatisfyMission">提出式がMissionを達成するかどうか。</param>
        [TestCase(false)]
        [TestCase(true)]
        public void GameplayController_AfterFeedback_ShouldPassSubmittedExpressionAndPreviousMission(
            bool shouldSatisfyMission)
        {
            GameplayConfig config = CreateConfig();
            var expressionInput = new ExpressionInput(config);
            var candidate = CreateCandidate(MissionCondition.Create(MissionKind.Odd));
            var recordingStrategy = new RecordingMissionDrawStrategy(
                new FixedMissionDrawStrategy(candidate));
            using var timer = new FakeSectionTimer();
            using var controller = new GameplayController(
                config,
                expressionInput,
                timer,
                recordingStrategy);
            MissionParameterCandidate[] candidates =
            {
                new MissionParameterCandidate(candidate.Condition)
            };
            var request = new GameplayStartRequest(
                GameMode.LifeEndless,
                GameDifficulty.Normal,
                MissionTestSettings.CreatePresetPreparationSettings(candidates, config));
            Expression submittedExpression = shouldSatisfyMission
                ? new Expression(1, OperatorType.Add, 2)
                : config.InitialExpression;

            GameplayStartResult startResult = controller.StartSession(request);
            Assert.That(startResult.IsSuccess, Is.True);
            Assert.That(recordingStrategy.Contexts, Has.Count.EqualTo(1));
            Assert.That(recordingStrategy.Contexts[0].IsInitialDraw, Is.True);
            expressionInput.Reset(submittedExpression);
            Mission previousMission = startResult.InitialState.CurrentMission;
            controller.Submit(previousMission.Id);
            controller.NotifyFeedbackCompleted(previousMission.Id);

            Assert.That(recordingStrategy.Contexts, Has.Count.EqualTo(2));
            MissionDrawContext nextContext = recordingStrategy.Contexts[1];
            Assert.That(nextContext.IsInitialDraw, Is.False);
            Assert.That(nextContext.CurrentExpression, Is.EqualTo(submittedExpression));
            Assert.That(nextContext.PreviousMission, Is.SameAs(previousMission));
        }

        /// <summary>
        /// Target適合率が有効範囲外の場合にConfig生成を拒否することを検証します。
        /// </summary>
        [Test]
        public void GameplayConfig_ShouldRejectInvalidTargetExpressionMatchRatio()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateConfig(targetExpressionMatchRatio: -0.01f));
        }

        /// <summary>
        /// 現在式で達成済みのMission選択割合が有効範囲外の場合にConfig生成を拒否することを検証します。
        /// </summary>
        [Test]
        public void GameplayConfig_ShouldRejectInvalidCurrentExpressionSatisfiedMissionSelectionRatio()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateConfig(
                currentExpressionSatisfiedMissionSelectionRatio: -0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateConfig(
                currentExpressionSatisfiedMissionSelectionRatio: 1.01f));
        }

        /// <summary>
        /// 現在式で達成済みのMission選択割合が非有限値の場合にConfig生成を拒否することを検証します。
        /// </summary>
        [Test]
        public void GameplayConfig_ShouldRejectNonFiniteCurrentExpressionSatisfiedMissionSelectionRatio()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateConfig(
                currentExpressionSatisfiedMissionSelectionRatio: float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateConfig(
                currentExpressionSatisfiedMissionSelectionRatio: float.PositiveInfinity));
        }

        /// <summary>
        /// Normal設定の現在式で達成済みのMission選択割合が0.10であることを検証します。
        /// </summary>
        [Test]
        public void GameplayConfigDefaults_ShouldUseTenPercentSatisfiedMissionSelectionRatio()
        {
            GameplayConfig config = GameplayConfigDefaults.CreateNormal();

            Assert.That(config.CurrentExpressionSatisfiedMissionSelectionRatio, Is.EqualTo(0.10f));
        }

        /// <summary>
        /// Negated抽選重みが負の場合にConfig生成を拒否することを検証します。
        /// </summary>
        [Test]
        public void GameplayConfig_ShouldRejectNegativeNegatedMissionSelectionWeight()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateConfig(negatedMissionSelectionWeight: -0.01f));
        }

        /// <summary>
        /// 本実装版抽選の割合と重みが非有限値の場合にConfig生成を拒否することを検証します。
        /// </summary>
        [Test]
        public void GameplayConfig_ShouldRejectNonFiniteFullMissionDrawSettings()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateConfig(targetExpressionMatchRatio: float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateConfig(targetExpressionMatchRatio: float.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateConfig(negatedMissionSelectionWeight: float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateConfig(negatedMissionSelectionWeight: float.PositiveInfinity));
        }

        /// <summary>
        /// 最終抽選プール数が0の場合にConfig生成を拒否することを検証します。
        /// </summary>
        [Test]
        public void GameplayConfig_ShouldRejectEmptyMissionParameterSelectionPool()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CreateConfig(missionParameterSelectionPoolSize: 0));
        }

        /// <summary>
        /// 診断Reporterが有効な場合に、各段階の選定結果と順位をTraceとして1回報告することを検証します。
        /// </summary>
        [Test]
        public void Draw_WhenDiagnosticsEnabled_ShouldReportEveryStageOfTheDraw()
        {
            var randomSource = new SequenceRandomSource(0.20d, 0.10d, 0.99d, 0.99d);
            var reporter = new RecordingDiagnosticsReporter(isEnabled: true);
            var strategy = CreateStrategy(randomSource, reporter);
            MissionCandidate closestCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Exact, 1),
                tier: 2,
                matchCount: 2,
                evaluatedCount: 10);
            MissionCandidate secondCandidate = CreateCandidate(
                MissionCondition.Create(MissionKind.Exact, 2),
                tier: 2,
                matchCount: 3,
                evaluatedCount: 10);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Odd), tier: 1),
                CreateCandidate(MissionCondition.Create(MissionKind.Even), tier: 2),
                CreateCandidate(
                    MissionCondition.Create(MissionKind.Odd, isNegated: true),
                    tier: 2),
                closestCandidate,
                secondCandidate
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 4));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(reporter.Traces, Has.Count.EqualTo(1));
            MissionDrawTrace trace = reporter.Traces[0];
            Assert.That(trace.Section, Is.EqualTo(4));
            Assert.That(trace.IsInitialDraw, Is.True);
            Assert.That(trace.CurrentTier, Is.EqualTo(2));
            Assert.That(trace.EnabledCandidateCount, Is.EqualTo(5));
            Assert.That(trace.UnlockedCandidateCount, Is.EqualTo(5));
            Assert.That(trace.SelectedExpressionGroup, Is.EqualTo(CurrentExpressionCandidateGroup.AllUnlocked));
            Assert.That(trace.ExpressionStageCandidateCount, Is.EqualTo(5));
            Assert.That(trace.SelectedTier, Is.EqualTo(2));
            Assert.That(trace.TierStageCandidateCount, Is.EqualTo(4));
            Assert.That(trace.IsNegatedPolaritySelected, Is.False);
            Assert.That(trace.PolarityStageCandidateCount, Is.EqualTo(3));
            Assert.That(trace.RepeatFilteredCandidateCount, Is.EqualTo(3));
            Assert.That(trace.SelectedKind, Is.EqualTo(MissionKind.Exact));
            Assert.That(trace.RankedCandidates, Is.EqualTo(new[] { closestCandidate, secondCandidate }));
            Assert.That(trace.PoolSize, Is.EqualTo(2));
            Assert.That(trace.SelectedCandidate, Is.SameAs(result.Candidate));
            Assert.That(trace.SelectedRank, Is.EqualTo(2));
        }

        /// <summary>
        /// 診断Reporterの有効状態が抽選結果と乱数消費順を変えないことを検証します。
        /// </summary>
        [Test]
        public void Draw_WhenDiagnosticsEnabled_ShouldKeepResultAndSampleConsumption()
        {
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even)),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd))
            };
            MissionDrawContext context = MissionDrawContext.CreateForInitialMission(
                CreateSessionContext(candidates),
                1);
            var disabledRandomSource = new SequenceRandomSource(0.75d);
            var enabledRandomSource = new SequenceRandomSource(0.75d);

            MissionDrawResult disabledResult = CreateStrategy(disabledRandomSource).Draw(context);
            MissionDrawResult enabledResult = CreateStrategy(
                enabledRandomSource,
                new RecordingDiagnosticsReporter(isEnabled: true)).Draw(context);

            Assert.That(enabledResult.Candidate, Is.SameAs(disabledResult.Candidate));
            Assert.That(enabledRandomSource.ConsumedSamples, Is.EqualTo(disabledRandomSource.ConsumedSamples));
        }

        /// <summary>
        /// 診断Reporterが無効な場合にTraceを報告しないことを検証します。
        /// </summary>
        [Test]
        public void Draw_WhenDiagnosticsDisabled_ShouldNotReportTrace()
        {
            var reporter = new RecordingDiagnosticsReporter(isEnabled: false);
            var strategy = CreateStrategy(new SequenceRandomSource(0d), reporter);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even)),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd))
            };

            MissionDrawResult result = strategy.Draw(
                MissionDrawContext.CreateForInitialMission(CreateSessionContext(candidates), 1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(reporter.Traces, Is.Empty);
        }

        /// <summary>
        /// 次回抽選のTraceが現在式による達成群と未達成群の件数、および選ばれた群を保持することを検証します。
        /// </summary>
        [Test]
        public void DrawNextMission_WhenDiagnosticsEnabled_ShouldReportCurrentExpressionGroups()
        {
            var reporter = new RecordingDiagnosticsReporter(isEnabled: true);
            var strategy = CreateStrategy(new SequenceRandomSource(0.09d), reporter);
            var candidates = new[]
            {
                CreateCandidate(MissionCondition.Create(MissionKind.Even)),
                CreateCandidate(MissionCondition.Create(MissionKind.Odd)),
                CreateCandidate(MissionCondition.Create(MissionKind.Prime))
            };

            MissionDrawResult result = strategy.Draw(CreateNextContext(
                candidates,
                new Expression(1, OperatorType.Add, 3),
                CreateMission(MissionCondition.Create(MissionKind.Exact, 9))));

            Assert.That(result.IsSuccess, Is.True);
            MissionDrawTrace trace = reporter.Traces.Single();
            Assert.That(trace.IsInitialDraw, Is.False);
            Assert.That(trace.SatisfiedCandidateCount, Is.EqualTo(1));
            Assert.That(trace.UnmatchedCandidateCount, Is.EqualTo(2));
            Assert.That(trace.SelectedExpressionGroup, Is.EqualTo(CurrentExpressionCandidateGroup.Satisfied));
            Assert.That(trace.ExpressionStageCandidateCount, Is.EqualTo(1));
        }

        /// <summary>
        /// 抽選TraceがPool外の候補を選出候補として受け付けないことを検証します。
        /// </summary>
        [Test]
        public void MissionDrawTrace_ShouldRejectSelectedCandidateOutsidePool()
        {
            MissionCandidate pooledCandidate = CreateCandidate(MissionCondition.Create(MissionKind.Exact, 1));
            MissionCandidate outsideCandidate = CreateCandidate(MissionCondition.Create(MissionKind.Exact, 2));

            var exception = Assert.Throws<ArgumentException>(() => new MissionDrawTrace(
                section: 1,
                isInitialDraw: true,
                currentTier: 1,
                config: CreateConfig(),
                enabledCandidateCount: 2,
                unlockedCandidateCount: 2,
                satisfiedCandidateCount: 0,
                unmatchedCandidateCount: 0,
                selectedExpressionGroup: CurrentExpressionCandidateGroup.AllUnlocked,
                tierStageCandidateCount: 2,
                isNegatedPolaritySelected: false,
                polarityStageCandidateCount: 2,
                repeatFilteredCandidateCount: 2,
                selectedKind: MissionKind.Exact,
                rankedCandidates: new[] { pooledCandidate, outsideCandidate },
                poolSize: 1,
                selectedCandidate: outsideCandidate));

            Assert.That(exception!.ParamName, Is.EqualTo("selectedCandidate"));
        }

        private static FullMissionDrawStrategy CreateStrategy(
            IRandomSource randomSource,
            IDiagnosticsReporter<MissionDrawTrace>? diagnosticsReporter = null)
        {
            return new FullMissionDrawStrategy(
                randomSource,
                diagnosticsReporter ?? new RecordingDiagnosticsReporter(isEnabled: false));
        }

        private static MissionDrawContext CreateNextContext(
            IReadOnlyList<MissionCandidate> candidates,
            Expression currentExpression,
            Mission previousMission,
            GameplayConfig? config = null,
            int section = 1)
        {
            return MissionDrawContext.CreateForNextMission(
                CreateSessionContext(candidates, config),
                section,
                currentExpression,
                previousMission);
        }

        private static GameplaySessionContext CreateSessionContext(
            IReadOnlyList<MissionCandidate> candidates,
            GameplayConfig? config = null)
        {
            return new GameplaySessionContext(
                config ?? CreateConfig(),
                new MissionCatalog(candidates));
        }

        private static GameplayConfig CreateConfig(
            float currentExpressionSatisfiedMissionSelectionRatio = 0.10f,
            float targetExpressionMatchRatio = 0.20f,
            float negatedMissionSelectionWeight = 0.60f,
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
                currentExpressionSatisfiedMissionSelectionRatio:
                    currentExpressionSatisfiedMissionSelectionRatio,
                targetExpressionMatchRatio: targetExpressionMatchRatio,
                negatedMissionSelectionWeight: negatedMissionSelectionWeight,
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

        private static Mission CreateMission(MissionCondition condition)
        {
            return new Mission(new MissionId(1), condition, 1);
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
            /// 消費されたsample列を取得します。
            /// </summary>
            public IReadOnlyList<double> ConsumedSamples { get; }

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
                ((List<double>)ConsumedSamples).Add(sample);
                return (float)sample;
            }
        }

        private sealed class RecordingDiagnosticsReporter : IDiagnosticsReporter<MissionDrawTrace>
        {
            private readonly List<MissionDrawTrace> _traces = new List<MissionDrawTrace>();

            public RecordingDiagnosticsReporter(bool isEnabled)
            {
                IsEnabled = isEnabled;
            }

            /// <summary>
            /// 生成時に指定された有効状態を取得します。
            /// </summary>
            public bool IsEnabled { get; }

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

        private sealed class RecordingMissionDrawStrategy : IMissionDrawStrategy
        {
            private readonly IMissionDrawStrategy _innerStrategy;

            public RecordingMissionDrawStrategy(IMissionDrawStrategy innerStrategy)
            {
                _innerStrategy = innerStrategy ?? throw new ArgumentNullException(nameof(innerStrategy));
                Contexts = new List<MissionDrawContext>();
            }

            /// <summary>
            /// 渡された抽選Contextを呼び出し順に取得します。
            /// </summary>
            public IReadOnlyList<MissionDrawContext> Contexts { get; }

            /// <summary>
            /// 抽選Contextを記録して内部Strategyへ委譲します。
            /// </summary>
            /// <param name="context">記録する抽選Context。</param>
            /// <returns>内部Strategyの抽選結果。</returns>
            public MissionDrawResult Draw(MissionDrawContext context)
            {
                ((List<MissionDrawContext>)Contexts).Add(context);
                return _innerStrategy.Draw(context);
            }
        }

        private sealed class FakeSectionTimer : ISectionTimer
        {
            private readonly Subject<SectionTimerSnapshot> _snapshotsSubject =
                new Subject<SectionTimerSnapshot>();
            private readonly Subject<SectionTimerExpiredSignal> _expiredSignalsSubject =
                new Subject<SectionTimerExpiredSignal>();

            /// <summary>
            /// 空のTimer Snapshotストリームを取得します。
            /// </summary>
            public Observable<SectionTimerSnapshot> Snapshots => _snapshotsSubject;

            /// <summary>
            /// 空のTimer満了ストリームを取得します。
            /// </summary>
            public Observable<SectionTimerExpiredSignal> ExpiredSignals => _expiredSignalsSubject;

            /// <summary>
            /// テスト用のためTimer開始を記録しません。
            /// </summary>
            public void Start(MissionId missionId, TimeSpan duration)
            {
            }

            /// <summary>
            /// テスト用のためTimer停止を記録しません。
            /// </summary>
            public void Stop(MissionId missionId)
            {
            }

            /// <summary>
            /// テスト用のためTimer全停止を記録しません。
            /// </summary>
            public void StopAll()
            {
            }

            /// <summary>
            /// テスト用Subjectを破棄します。
            /// </summary>
            public void Dispose()
            {
                _snapshotsSubject.Dispose();
                _expiredSignalsSubject.Dispose();
            }
        }
    }
}
