#nullable enable
using System;
using System.Collections.Generic;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;

namespace Ccc.Simulation
{
    /// <summary>
    /// 本番と同じ手順でMissionCatalogを準備し、シナリオが生成するContextで本実装版Strategyを繰り返し抽選します。
    /// </summary>
    public static class MissionDrawSimulation
    {
        private const int ScenarioSeedSalt = 0x5DEECE6;

        /// <summary>
        /// seedから抽選用とシナリオ用の乱数を作り、準備したCatalogと、シナリオが終了するまでの各抽選を診断Reporterへ報告します。
        /// </summary>
        /// <param name="config">抽選に使うGameplay設定。</param>
        /// <param name="preparationSettings">MissionCatalog準備設定。</param>
        /// <param name="seed">抽選用とシナリオ用の乱数を決めるseed。同じseedと入力からは同じ結果を再現します。</param>
        /// <param name="createScenario">準備済み環境からシナリオを生成する関数。</param>
        /// <param name="catalogReporter">準備したMissionCatalogを抽選前に1回受け取る診断Reporter。</param>
        /// <param name="diagnosticsReporter">各抽選のTraceを受け取る診断Reporter。</param>
        /// <returns>抽選回数と失敗内訳。</returns>
        /// <exception cref="ArgumentNullException">引数がnullの場合。</exception>
        /// <exception cref="InvalidOperationException">MissionCatalogの準備に失敗した場合。</exception>
        public static MissionDrawSimulationSummary Run(
            GameplayConfig config,
            MissionCatalogPreparationSettings preparationSettings,
            int seed,
            Func<MissionDrawScenarioEnvironment, IMissionDrawScenario> createScenario,
            IDiagnosticsReporter<MissionCatalog> catalogReporter,
            IDiagnosticsReporter<MissionDrawTrace> diagnosticsReporter)
        {
            if (createScenario == null)
            {
                throw new ArgumentNullException(nameof(createScenario));
            }

            if (catalogReporter == null)
            {
                throw new ArgumentNullException(nameof(catalogReporter));
            }

            if (diagnosticsReporter == null)
            {
                throw new ArgumentNullException(nameof(diagnosticsReporter));
            }

            MissionCatalogPreparationResult preparationResult =
                MissionCatalogPreparer.Prepare(config, preparationSettings);
            if (!preparationResult.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"Mission catalog preparation failed ({preparationResult.FailureKind}): " +
                    preparationResult.FailureMessage);
            }

            if (catalogReporter.IsEnabled)
            {
                catalogReporter.Report(preparationResult.Catalog);
            }

            var environment = new MissionDrawScenarioEnvironment(
                new GameplaySessionContext(config, preparationResult.Catalog),
                preparationResult.ExpressionCandidates,
                new SimulationRandom(seed ^ ScenarioSeedSalt));
            IMissionDrawScenario scenario = createScenario(environment)
                ?? throw new InvalidOperationException("Scenario factory returned null.");
            var strategy = new FullMissionDrawStrategy(new SimulationRandom(seed), diagnosticsReporter);

            int drawCount = 0;
            var failureCounts = new Dictionary<string, int>();
            MissionDrawContext? previousContext = null;
            MissionDrawResult? previousResult = null;
            while (scenario.TryCreateNextContext(previousContext, previousResult, out MissionDrawContext context))
            {
                MissionDrawResult result = strategy.Draw(context);
                drawCount++;
                if (!result.IsSuccess)
                {
                    failureCounts.TryGetValue(result.FailureMessage, out int count);
                    failureCounts[result.FailureMessage] = count + 1;
                }

                previousContext = context;
                previousResult = result;
            }

            return new MissionDrawSimulationSummary(drawCount, failureCounts);
        }
    }

    /// <summary>
    /// シミュレーションの抽選回数と、失敗理由ごとの失敗回数を保持する不変値です。
    /// </summary>
    public sealed class MissionDrawSimulationSummary
    {
        /// <summary>
        /// 抽選回数を取得します。
        /// </summary>
        public int DrawCount { get; }

        /// <summary>
        /// 失敗理由ごとの失敗回数を取得します。
        /// </summary>
        public IReadOnlyDictionary<string, int> FailureCounts { get; }

        /// <summary>
        /// 失敗回数の合計を取得します。
        /// </summary>
        public int FailureCount { get; }

        /// <summary>
        /// 抽選回数と失敗内訳から集計結果を生成します。
        /// </summary>
        /// <param name="drawCount">抽選回数。</param>
        /// <param name="failureCounts">失敗理由ごとの失敗回数。</param>
        /// <exception cref="ArgumentNullException">失敗内訳がnullの場合。</exception>
        public MissionDrawSimulationSummary(int drawCount, IReadOnlyDictionary<string, int> failureCounts)
        {
            if (failureCounts == null)
            {
                throw new ArgumentNullException(nameof(failureCounts));
            }

            DrawCount = drawCount;
            FailureCounts = ToDictionary(failureCounts);
            int failureCount = 0;
            foreach (int count in FailureCounts.Values)
            {
                failureCount += count;
            }

            FailureCount = failureCount;
        }

        private static Dictionary<string, int> ToDictionary(IReadOnlyDictionary<string, int> source)
        {
            var copy = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> entry in source)
            {
                copy.Add(entry.Key, entry.Value);
            }

            return copy;
        }
    }
}
