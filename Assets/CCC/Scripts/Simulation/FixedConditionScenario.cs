#nullable enable
using System;
using System.Linq;
using Ccc.Core.Model;
using Ccc.Core.UseCases;

namespace Ccc.Simulation
{
    /// <summary>
    /// 固定条件シナリオで行う抽選の種類を表します。
    /// </summary>
    public enum FixedConditionDrawKind
    {
        /// <summary>
        /// 現在式と直前Missionを持たない初回抽選です。
        /// </summary>
        Initial,

        /// <summary>
        /// 現在式と直前Missionを持つ次回抽選です。
        /// </summary>
        Next
    }

    /// <summary>
    /// 固定条件シナリオで、抽選入力の値をどう決めるかを表します。
    /// </summary>
    public enum ScenarioValueSource
    {
        /// <summary>
        /// 指定した値を全抽選で使います。
        /// </summary>
        Fixed,

        /// <summary>
        /// 抽選のたびに候補から一様に選びます。直前MissionはSectionで解放済みのCatalog候補から選びます。
        /// </summary>
        RandomPerDraw
    }

    /// <summary>
    /// Section、現在式、直前Missionを固定または毎回乱数で決め、同じ条件で指定回数だけ独立に抽選するシナリオです。
    /// </summary>
    public sealed class FixedConditionScenario : IMissionDrawScenario
    {
        private readonly MissionDrawScenarioEnvironment _environment;
        private readonly int _section;
        private readonly int _drawCount;
        private readonly FixedConditionDrawKind _drawKind;
        private readonly ScenarioValueSource _expressionSource;
        private readonly Expression _fixedExpression;
        private readonly ScenarioValueSource _previousMissionSource;
        private readonly Mission? _fixedPreviousMission;
        private int _createdContextCount;

        /// <summary>
        /// 抽選条件からシナリオを生成します。
        /// </summary>
        /// <param name="environment">準備済みCatalogと候補式とシナリオ用乱数。</param>
        /// <param name="section">全抽選で使うSection。</param>
        /// <param name="drawCount">抽選回数。</param>
        /// <param name="drawKind">初回抽選か次回抽選か。</param>
        /// <param name="expressionSource">次回抽選の現在式を固定するか毎回選ぶか。</param>
        /// <param name="fixedExpression">固定する現在式。<paramref name="expressionSource"/>が<see cref="ScenarioValueSource.Fixed"/>の場合だけ使います。</param>
        /// <param name="previousMissionSource">次回抽選の直前Missionを固定するか、毎回Sectionで解放済みのCatalog候補から選ぶか。</param>
        /// <param name="fixedPreviousMission">固定する直前Missionの条件。<paramref name="previousMissionSource"/>が<see cref="ScenarioValueSource.Fixed"/>の場合だけ使います。</param>
        /// <exception cref="ArgumentNullException">環境がnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Sectionまたは抽選回数が1未満の場合。</exception>
        /// <exception cref="ArgumentException">固定する現在式が候補式にない場合、または固定する直前MissionがCatalogにない場合。</exception>
        public FixedConditionScenario(
            MissionDrawScenarioEnvironment environment,
            int section,
            int drawCount,
            FixedConditionDrawKind drawKind,
            ScenarioValueSource expressionSource,
            Expression fixedExpression,
            ScenarioValueSource previousMissionSource,
            MissionConditionSpec fixedPreviousMission)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            if (section < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, "Section must be at least one.");
            }

            if (drawCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(drawCount), drawCount, "Draw count must be at least one.");
            }

            _section = section;
            _drawCount = drawCount;
            _drawKind = drawKind;
            _expressionSource = expressionSource;
            _fixedExpression = fixedExpression;
            _previousMissionSource = previousMissionSource;
            if (drawKind != FixedConditionDrawKind.Next)
            {
                return;
            }

            if (expressionSource == ScenarioValueSource.Fixed &&
                !environment.DefinedExpressionCandidates.Any(candidate => candidate.Expression == fixedExpression))
            {
                throw new ArgumentException(
                    $"Fixed expression {fixedExpression.Left} {fixedExpression.Operator} {fixedExpression.Right} " +
                    "is not a defined expression candidate.",
                    nameof(fixedExpression));
            }

            if (previousMissionSource == ScenarioValueSource.Fixed)
            {
                _fixedPreviousMission = MissionDrawScenarioEnvironment.CreateMission(
                    fixedPreviousMission.FindCandidate(environment.SessionContext.MissionCatalog.Candidates),
                    section);
            }
        }

        /// <summary>
        /// 抽選回数に達するまで、同じ条件のContextを生成します。
        /// </summary>
        /// <param name="previousContext">直前に抽選したContext。使用しません。</param>
        /// <param name="previousResult">直前の抽選結果。使用しません。</param>
        /// <param name="context">次に抽選するContext。</param>
        /// <returns>次の抽選がある場合はtrue、抽選回数に達した場合はfalse。</returns>
        public bool TryCreateNextContext(
            MissionDrawContext? previousContext,
            MissionDrawResult? previousResult,
            out MissionDrawContext context)
        {
            if (_createdContextCount >= _drawCount)
            {
                context = null!;
                return false;
            }

            _createdContextCount++;
            if (_drawKind == FixedConditionDrawKind.Initial)
            {
                context = MissionDrawContext.CreateForInitialMission(_environment.SessionContext, _section);
                return true;
            }

            Expression currentExpression = _expressionSource == ScenarioValueSource.Fixed
                ? _fixedExpression
                : _environment.PickExpression();
            Mission previousMission = _previousMissionSource == ScenarioValueSource.Fixed
                ? _fixedPreviousMission!
                : _environment.PickUnlockedCatalogMission(_section);
            context = MissionDrawContext.CreateForNextMission(
                _environment.SessionContext,
                _section,
                currentExpression,
                previousMission);
            return true;
        }
    }
}
