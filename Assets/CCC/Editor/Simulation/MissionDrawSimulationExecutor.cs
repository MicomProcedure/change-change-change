using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Assets;
using Ccc.Infrastructure.Logging;
using Ccc.Simulation;

namespace Ccc.Editor.Simulation
{
    /// <summary>
    /// シミュレーション設定から本番と同じConfigとCatalog準備設定、および選択中のStrategyを作り、結果をCSVへ書き出しながらシミュレーションを実行します。
    /// </summary>
    public static class MissionDrawSimulationExecutor
    {
        private const string RunIdPrefix = "sim_";
        private const string SeedLabel = "_seed";

        /// <summary>
        /// 設定を検証してシミュレーションを実行し、catalog表、draws表、candidates表のCSVを出力します。
        /// </summary>
        /// <param name="settings">シミュレーション設定。</param>
        /// <returns>抽選回数と失敗内訳、およびCSVの出力先ディレクトリ。</returns>
        /// <exception cref="ArgumentNullException">設定がnullの場合。</exception>
        /// <exception cref="InvalidOperationException">設定が不正な場合、またはCatalogの準備に失敗した場合。</exception>
        public static (MissionDrawSimulationSummary Summary, string OutputDirectoryPath) Execute(
            MissionDrawSimulationSettingsAsset settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            IReadOnlyList<string> errors = settings.Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Mission draw simulation settings validation failed: " + string.Join(" | ", errors));
            }

            GameplayConfigEntry entry = settings.ConfigEntry;
            LogSettingsAsset logSettings = settings.LogSettings;
            GameplayConfig config = entry.GameplayConfigAsset.CreateRuntimeConfig();
            MissionCatalogPreparationSettings preparationSettings = entry.CreateCatalogPreparationSettings(config);
            var session = new DiagnosticsSessionDescriptor(
                RunIdPrefix + logSettings.FormatTimestamp(DateTime.Now) + SeedLabel + settings.Seed,
                entry.DisplayName,
                entry.GameMode,
                entry.Difficulty);
            string outputDirectoryPath = logSettings.ResolveCategoryDirectoryPath(settings.OutputDirectoryName);
            using (var catalogReporter = new CsvDiagnosticsReporter<MissionCatalog>(
                       new MissionCatalogCsvFormatter(),
                       session,
                       outputDirectoryPath))
            using (var reporter = new CsvDiagnosticsReporter<MissionDrawTrace>(
                       new MissionDrawTraceCsvFormatter(settings.CsvCandidateCount),
                       session,
                       outputDirectoryPath))
            {
                MissionDrawSimulationSummary summary = MissionDrawSimulation.Run(
                    config,
                    preparationSettings,
                    settings.Seed,
                    settings.CreateScenario,
                    settings.CreateStrategy,
                    catalogReporter,
                    reporter);
                return (summary, outputDirectoryPath);
            }
        }
    }
}
