using System;
using System.Collections.Generic;
using System.IO;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Assets;
using Ccc.Infrastructure.Logging;
using Ccc.Simulation;
using UnityEngine;

namespace Ccc.Editor.Simulation
{
    /// <summary>
    /// Mission抽選シミュレーションの方式を表します。
    /// </summary>
    public enum MissionDrawSimulationMode
    {
        /// <summary>
        /// 毎回正解したものとしてSectionを進め、実プレイの抽選の流れを再現します。
        /// </summary>
        SequentialFlow,

        /// <summary>
        /// 抽選条件を固定し、同じ条件で独立に繰り返し抽選します。
        /// </summary>
        FixedCondition
    }

    /// <summary>
    /// Mission抽選シミュレーションで使うMission抽選Strategyを表します。
    /// </summary>
    public enum MissionDrawSimulationStrategy
    {
        /// <summary>
        /// 設定の目標式適合割合に近い候補から選ぶ <see cref="FullMissionDrawStrategy"/> を使います。
        /// </summary>
        Full,

        /// <summary>
        /// Sectionが進むほど下限が下がる範囲から目標式適合割合を選ぶ <see cref="SectionDecliningMatchRatioMissionDrawStrategy"/> を使います。
        /// </summary>
        SectionDecliningMatchRatio
    }

    /// <summary>
    /// Mission抽選シミュレーションの対象Config、Strategy、出力先、seed、方式別パラメータをInspectorで編集するScriptableObjectです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Mission Draw Simulation Settings")]
    public sealed class MissionDrawSimulationSettingsAsset : ScriptableObject
    {
        private const string DefaultOutputDirectoryName = "Simulation";
        private const int DefaultCsvCandidateCount = 10;
        private const int DefaultSessionCount = 100;
        private const int DefaultMaximumSection = 20;
        private const int DefaultSection = 1;
        private const int DefaultDrawCount = 1000;
        private const int DefaultExpressionNumber = 1;

        [Header("Target")]
        [SerializeField]
        private GameplayConfigEntry configEntry;

        [SerializeField]
        private LogSettingsAsset logSettings;

        [SerializeField]
        private MissionDrawSimulationStrategy strategy = MissionDrawSimulationStrategy.SectionDecliningMatchRatio;

        [Header("Output")]
        [SerializeField]
        private string outputDirectoryName = DefaultOutputDirectoryName;

        [SerializeField]
        private int csvCandidateCount = DefaultCsvCandidateCount;

        [Header("Run")]
        [SerializeField]
        private int seed;

        [SerializeField]
        private MissionDrawSimulationMode mode = MissionDrawSimulationMode.SequentialFlow;

        [Header("Sequential Flow")]
        [SerializeField]
        private int sessionCount = DefaultSessionCount;

        [SerializeField]
        private int maximumSection = DefaultMaximumSection;

        [Header("Fixed Condition")]
        [SerializeField]
        private int section = DefaultSection;

        [SerializeField]
        private int drawCount = DefaultDrawCount;

        [SerializeField]
        private FixedConditionDrawKind drawKind = FixedConditionDrawKind.Next;

        [SerializeField]
        private ScenarioValueSource expressionSource = ScenarioValueSource.RandomPerDraw;

        [SerializeField]
        private int expressionLeft = DefaultExpressionNumber;

        [SerializeField]
        private OperatorType expressionOperator = OperatorType.Add;

        [SerializeField]
        private int expressionRight = DefaultExpressionNumber;

        [SerializeField]
        private ScenarioValueSource previousMissionSource = ScenarioValueSource.RandomPerDraw;

        [SerializeField]
        private MissionKind previousMissionKind = MissionKind.Even;

        [SerializeField]
        private int previousMissionParameterA;

        [SerializeField]
        private int previousMissionParameterB;

        [SerializeField]
        private bool isPreviousMissionNegated;

        /// <summary>
        /// シミュレーション対象のGameplay設定Entryを取得します。
        /// </summary>
        public GameplayConfigEntry ConfigEntry => configEntry;

        /// <summary>
        /// 出力ルートとファイル名の日時書式を持つLog設定を取得します。
        /// </summary>
        public LogSettingsAsset LogSettings => logSettings;

        /// <summary>
        /// CSVファイルを配置する種類別ディレクトリ名を取得します。
        /// </summary>
        public string OutputDirectoryName => outputDirectoryName;

        /// <summary>
        /// candidates表に含める順位上位の候補数を取得します。
        /// </summary>
        public int CsvCandidateCount => csvCandidateCount;

        /// <summary>
        /// 抽選用とシナリオ用の乱数を決めるseedを取得します。
        /// </summary>
        public int Seed => seed;

        /// <summary>
        /// 未設定の参照へ値を設定します。設定済みの参照は変更しません。
        /// </summary>
        /// <param name="defaultConfigEntry">未設定時に使うGameplay設定Entry。</param>
        /// <param name="defaultLogSettings">未設定時に使うLog設定。</param>
        public void AssignMissingReferences(GameplayConfigEntry defaultConfigEntry, LogSettingsAsset defaultLogSettings)
        {
            if (configEntry == null)
            {
                configEntry = defaultConfigEntry;
            }

            if (logSettings == null)
            {
                logSettings = defaultLogSettings;
            }
        }

        /// <summary>
        /// Inspector設定と参照先Assetを検証し、検出されたエラー一覧を返します。
        /// </summary>
        /// <returns>技術的な設定エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (configEntry == null)
            {
                errors.Add("Gameplay config entry is missing.");
            }
            else
            {
                AddPrefixed(errors, "Gameplay config entry", configEntry.Validate());
                if (configEntry.GameplayConfigAsset != null)
                {
                    AddPrefixed(errors, "Gameplay config", configEntry.GameplayConfigAsset.Validate());
                }
            }

            if (logSettings == null)
            {
                errors.Add("Log settings asset is missing.");
            }
            else
            {
                AddPrefixed(errors, "Log settings", logSettings.Validate());
            }

            if (string.IsNullOrWhiteSpace(outputDirectoryName) ||
                outputDirectoryName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errors.Add($"Output directory name is invalid: {outputDirectoryName}.");
            }

            if (csvCandidateCount < 0)
            {
                errors.Add("CSV candidate count must not be negative.");
            }

            if (mode == MissionDrawSimulationMode.SequentialFlow)
            {
                AddIfLessThanOne(errors, sessionCount, "Session count");
                AddIfLessThanOne(errors, maximumSection, "Maximum section");
            }
            else
            {
                AddIfLessThanOne(errors, section, "Section");
                AddIfLessThanOne(errors, drawCount, "Draw count");
            }

            return errors;
        }

        /// <summary>
        /// 選択中のStrategyを、抽選用乱数と診断Reporterから生成します。
        /// </summary>
        /// <param name="randomSource">抽選用の乱数提供元。</param>
        /// <param name="diagnosticsReporter">各抽選のTraceを受け取る診断Reporter。</param>
        /// <returns>Mission抽選Strategy。</returns>
        /// <exception cref="ArgumentOutOfRangeException">Strategyが未定義の場合。</exception>
        public IMissionDrawStrategy CreateStrategy(
            IRandomSource randomSource,
            IDiagnosticsReporter<MissionDrawTrace> diagnosticsReporter)
        {
            switch (strategy)
            {
                case MissionDrawSimulationStrategy.Full:
                    return new FullMissionDrawStrategy(randomSource, diagnosticsReporter);

                case MissionDrawSimulationStrategy.SectionDecliningMatchRatio:
                    return new SectionDecliningMatchRatioMissionDrawStrategy(randomSource, diagnosticsReporter);

                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }
        }

        /// <summary>
        /// 選択中の方式とパラメータから、準備済み環境で動くシナリオを生成します。
        /// </summary>
        /// <param name="environment">準備済みCatalogと候補式とシナリオ用乱数。</param>
        /// <returns>抽選シナリオ。</returns>
        /// <exception cref="ArgumentOutOfRangeException">方式が未定義の場合。</exception>
        public IMissionDrawScenario CreateScenario(MissionDrawScenarioEnvironment environment)
        {
            switch (mode)
            {
                case MissionDrawSimulationMode.SequentialFlow:
                    return new SequentialFlowScenario(environment, sessionCount, maximumSection);

                case MissionDrawSimulationMode.FixedCondition:
                    return new FixedConditionScenario(
                        environment,
                        section,
                        drawCount,
                        drawKind,
                        expressionSource,
                        expressionSource == ScenarioValueSource.Fixed
                            ? new Expression(expressionLeft, expressionOperator, expressionRight)
                            : default,
                        previousMissionSource,
                        new MissionConditionSpec(
                            previousMissionKind,
                            previousMissionParameterA,
                            previousMissionParameterB,
                            isPreviousMissionNegated));

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private static void AddPrefixed(List<string> errors, string prefix, IReadOnlyList<string> sourceErrors)
        {
            for (int index = 0; index < sourceErrors.Count; index++)
            {
                errors.Add($"{prefix}: {sourceErrors[index]}");
            }
        }

        private static void AddIfLessThanOne(List<string> errors, int value, string settingName)
        {
            if (value < 1)
            {
                errors.Add($"{settingName} must be at least one.");
            }
        }
    }
}
