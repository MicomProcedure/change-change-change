#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;

namespace Ccc.Simulation
{
    /// <summary>
    /// シミュレーションで次に抽選するContextを、直前の抽選Contextと結果から決めるシナリオです。
    /// </summary>
    public interface IMissionDrawScenario
    {
        /// <summary>
        /// 次に抽選するContextを生成します。
        /// </summary>
        /// <param name="previousContext">直前に抽選したContext。最初の呼び出しではnullです。</param>
        /// <param name="previousResult">直前の抽選結果。最初の呼び出しではnullです。</param>
        /// <param name="context">次に抽選するContext。</param>
        /// <returns>次の抽選がある場合はtrue、シナリオが終了した場合はfalse。</returns>
        bool TryCreateNextContext(
            MissionDrawContext? previousContext,
            MissionDrawResult? previousResult,
            out MissionDrawContext context);
    }

    /// <summary>
    /// シナリオが抽選Contextを組み立てるために使う、準備済みCatalogと候補式とシナリオ用乱数をまとめた値です。
    /// </summary>
    public sealed class MissionDrawScenarioEnvironment
    {
        /// <summary>
        /// 準備済みCatalogを持つSession Contextを取得します。
        /// </summary>
        public GameplaySessionContext SessionContext { get; }

        /// <summary>
        /// 計算結果が定義される候補式一覧を取得します。
        /// </summary>
        public IReadOnlyList<ExpressionCandidate> DefinedExpressionCandidates { get; }

        /// <summary>
        /// 現在式や直前Missionの選択に使う、抽選用とは独立した乱数を取得します。
        /// </summary>
        public SimulationRandom Random { get; }

        /// <summary>
        /// Session Context、候補式一覧、シナリオ用乱数から環境を生成します。
        /// </summary>
        /// <param name="sessionContext">準備済みCatalogを持つSession Context。</param>
        /// <param name="expressionCandidates">Catalog構築に使った候補式一覧。計算結果が未定義の式は除外して保持します。</param>
        /// <param name="random">シナリオ用乱数。</param>
        /// <exception cref="ArgumentNullException">引数がnullの場合。</exception>
        /// <exception cref="ArgumentException">計算結果が定義される候補式がない場合。</exception>
        public MissionDrawScenarioEnvironment(
            GameplaySessionContext sessionContext,
            IReadOnlyList<ExpressionCandidate> expressionCandidates,
            SimulationRandom random)
        {
            SessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            if (expressionCandidates == null)
            {
                throw new ArgumentNullException(nameof(expressionCandidates));
            }

            DefinedExpressionCandidates = expressionCandidates
                .Where(candidate => candidate.Result.IsDefined)
                .ToArray();
            if (DefinedExpressionCandidates.Count == 0)
            {
                throw new ArgumentException(
                    "Expression candidates must contain at least one defined expression.",
                    nameof(expressionCandidates));
            }

            Random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>
        /// 候補式から1件を一様に選びます。
        /// </summary>
        /// <returns>選ばれた式。</returns>
        public Expression PickExpression()
        {
            return DefinedExpressionCandidates[Random.NextIndex(DefinedExpressionCandidates.Count)].Expression;
        }

        /// <summary>
        /// 指定SectionのCurrent Tier以下で解放済みのCatalog候補から1件を一様に選び、Missionとして返します。
        /// </summary>
        /// <param name="section">解放済みTierを決めるSection。生成するMissionのId値にも使います。</param>
        /// <returns>選ばれた候補の条件とTierを持つMission。</returns>
        /// <exception cref="InvalidOperationException">解放済みの候補がない場合。</exception>
        public Mission PickUnlockedCatalogMission(int section)
        {
            int currentTier = TierResolver.ResolveCurrentTier(section, SessionContext.Config);
            MissionCandidate[] unlockedCandidates = SessionContext.MissionCatalog.Candidates
                .Where(candidate => candidate.Tier <= currentTier)
                .ToArray();
            if (unlockedCandidates.Length == 0)
            {
                throw new InvalidOperationException($"No catalog candidates are unlocked at section {section}.");
            }

            return CreateMission(unlockedCandidates[Random.NextIndex(unlockedCandidates.Length)], section);
        }

        /// <summary>
        /// Catalog候補を指定IdのMissionへ変換します。
        /// </summary>
        /// <param name="candidate">変換する候補。</param>
        /// <param name="missionIdValue">生成するMissionのId値。</param>
        /// <returns>候補の条件とTierを持つMission。</returns>
        public static Mission CreateMission(MissionCandidate candidate, int missionIdValue)
        {
            return new Mission(new MissionId(missionIdValue), candidate.Condition, candidate.Tier);
        }
    }
}
