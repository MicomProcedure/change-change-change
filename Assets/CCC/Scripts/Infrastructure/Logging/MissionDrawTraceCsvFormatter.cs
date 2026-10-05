using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ccc.Core.Model;
using Ccc.Core.UseCases;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// Mission抽選Traceを、抽選1回を1行とするdraws表と、順位上位候補1件を1行とするcandidates表へ整形します。
    /// </summary>
    public sealed class MissionDrawTraceCsvFormatter : ICsvTraceFormatter<MissionDrawTrace>
    {
        private const string DrawsFileSuffix = "draws";
        private const string CandidatesFileSuffix = "candidates";
        private const string ListSeparator = ";";

        private static readonly CsvColumn<DrawRowSource>[] DrawColumns =
        {
            new CsvColumn<DrawRowSource>("SessionId", row => row.Context.Session.SessionId),
            new CsvColumn<DrawRowSource>("ConfigDisplayName", row => row.Context.Session.ConfigDisplayName),
            new CsvColumn<DrawRowSource>("GameMode", row => row.Context.Session.GameMode.ToString()),
            new CsvColumn<DrawRowSource>("Difficulty", row => row.Context.Session.Difficulty.ToString()),
            new CsvColumn<DrawRowSource>(
                "TargetExpressionMatchRatio",
                row => FormatNumber(row.Trace.Config.TargetExpressionMatchRatio)),
            new CsvColumn<DrawRowSource>(
                "MissionParameterSelectionPoolSize",
                row => FormatNumber(row.Trace.Config.MissionParameterSelectionPoolSize)),
            new CsvColumn<DrawRowSource>(
                "NegatedMissionSelectionWeight",
                row => FormatNumber(row.Trace.Config.NegatedMissionSelectionWeight)),
            new CsvColumn<DrawRowSource>(
                "CurrentExpressionSatisfiedMissionSelectionRatio",
                row => FormatNumber(row.Trace.Config.CurrentExpressionSatisfiedMissionSelectionRatio)),
            new CsvColumn<DrawRowSource>(
                "CurrentTierDrawWeight",
                row => FormatNumber(row.Trace.Config.CurrentTierDrawWeight)),
            new CsvColumn<DrawRowSource>(
                "LowerTierDrawWeight",
                row => FormatNumber(row.Trace.Config.LowerTierDrawWeight)),
            new CsvColumn<DrawRowSource>(
                "TierMinimumSections",
                row => string.Join(
                    ListSeparator,
                    row.Trace.Config.TierProgression.MinimumSectionsByTier.Select(
                        minimumSection => FormatNumber(minimumSection)))),
            new CsvColumn<DrawRowSource>("DrawIndex", row => FormatNumber(row.Context.TraceIndex)),
            new CsvColumn<DrawRowSource>("Section", row => FormatNumber(row.Trace.Section)),
            new CsvColumn<DrawRowSource>("IsInitialDraw", row => row.Trace.IsInitialDraw.ToString()),
            new CsvColumn<DrawRowSource>("CurrentTier", row => FormatNumber(row.Trace.CurrentTier)),
            new CsvColumn<DrawRowSource>("EnabledCount", row => FormatNumber(row.Trace.EnabledCandidateCount)),
            new CsvColumn<DrawRowSource>("UnlockedCount", row => FormatNumber(row.Trace.UnlockedCandidateCount)),
            new CsvColumn<DrawRowSource>("SatisfiedCount", row => FormatNumber(row.Trace.SatisfiedCandidateCount)),
            new CsvColumn<DrawRowSource>("UnmatchedCount", row => FormatNumber(row.Trace.UnmatchedCandidateCount)),
            new CsvColumn<DrawRowSource>(
                "SelectedExpressionGroup",
                row => row.Trace.SelectedExpressionGroup.ToString()),
            new CsvColumn<DrawRowSource>(
                "ExpressionStageCount",
                row => FormatNumber(row.Trace.ExpressionStageCandidateCount)),
            new CsvColumn<DrawRowSource>("SelectedTier", row => FormatNumber(row.Trace.SelectedTier)),
            new CsvColumn<DrawRowSource>("TierStageCount", row => FormatNumber(row.Trace.TierStageCandidateCount)),
            new CsvColumn<DrawRowSource>(
                "SelectedPolarity",
                row => MissionDrawTraceValueFormatter.FormatPolarity(row.Trace.IsNegatedPolaritySelected)),
            new CsvColumn<DrawRowSource>(
                "PolarityStageCount",
                row => FormatNumber(row.Trace.PolarityStageCandidateCount)),
            new CsvColumn<DrawRowSource>(
                "RepeatFilteredCount",
                row => FormatNumber(row.Trace.RepeatFilteredCandidateCount)),
            new CsvColumn<DrawRowSource>("SelectedKind", row => row.Trace.SelectedKind.ToString()),
            new CsvColumn<DrawRowSource>("KindStageCount", row => FormatNumber(row.Trace.RankedCandidates.Count)),
            new CsvColumn<DrawRowSource>("PoolSize", row => FormatNumber(row.Trace.PoolSize)),
            new CsvColumn<DrawRowSource>("SelectedRank", row => FormatNumber(row.Trace.SelectedRank)),
            new CsvColumn<DrawRowSource>(
                "SelectedParameterA",
                row => FormatNumber(row.Trace.SelectedCandidate.Condition.ParameterA)),
            new CsvColumn<DrawRowSource>(
                "SelectedParameterB",
                row => FormatNumber(row.Trace.SelectedCandidate.Condition.ParameterB)),
            new CsvColumn<DrawRowSource>(
                "SelectedMatchCount",
                row => FormatNumber(row.Trace.SelectedCandidate.ExpressionMatchCount)),
            new CsvColumn<DrawRowSource>(
                "SelectedEvaluatedCount",
                row => FormatNumber(row.Trace.SelectedCandidate.EvaluatedExpressionCount)),
            new CsvColumn<DrawRowSource>(
                "SelectedRatio",
                row => MissionDrawTraceValueFormatter.FormatRatio(
                    row.Trace.SelectedCandidate.ExpressionMatchRatio)),
            new CsvColumn<DrawRowSource>(
                "SelectedDifference",
                row => MissionDrawTraceValueFormatter.FormatRatio(
                    MissionDrawTraceValueFormatter.CalculateTargetDifference(
                        row.Trace,
                        row.Trace.SelectedCandidate)))
        };

        private static readonly CsvColumn<CandidateRowSource>[] CandidateColumns =
        {
            new CsvColumn<CandidateRowSource>("SessionId", row => row.Context.Session.SessionId),
            new CsvColumn<CandidateRowSource>("DrawIndex", row => FormatNumber(row.Context.TraceIndex)),
            new CsvColumn<CandidateRowSource>("Rank", row => FormatNumber(row.Rank)),
            new CsvColumn<CandidateRowSource>("Tier", row => FormatNumber(row.Candidate.Tier)),
            new CsvColumn<CandidateRowSource>("Kind", row => row.Candidate.Condition.Kind.ToString()),
            new CsvColumn<CandidateRowSource>(
                "ParameterA",
                row => FormatNumber(row.Candidate.Condition.ParameterA)),
            new CsvColumn<CandidateRowSource>(
                "ParameterB",
                row => FormatNumber(row.Candidate.Condition.ParameterB)),
            new CsvColumn<CandidateRowSource>(
                "IsNegated",
                row => row.Candidate.Condition.IsNegated.ToString()),
            new CsvColumn<CandidateRowSource>(
                "MatchCount",
                row => FormatNumber(row.Candidate.ExpressionMatchCount)),
            new CsvColumn<CandidateRowSource>(
                "EvaluatedCount",
                row => FormatNumber(row.Candidate.EvaluatedExpressionCount)),
            new CsvColumn<CandidateRowSource>(
                "Ratio",
                row => MissionDrawTraceValueFormatter.FormatRatio(row.Candidate.ExpressionMatchRatio)),
            new CsvColumn<CandidateRowSource>(
                "Difference",
                row => MissionDrawTraceValueFormatter.FormatRatio(
                    MissionDrawTraceValueFormatter.CalculateTargetDifference(row.Trace, row.Candidate))),
            new CsvColumn<CandidateRowSource>(
                "IsInPool",
                row => MissionDrawTraceValueFormatter.IsInPool(row.Trace, row.Rank).ToString()),
            new CsvColumn<CandidateRowSource>(
                "IsSelected",
                row => MissionDrawTraceValueFormatter.IsSelected(row.Trace, row.Candidate).ToString())
        };

        private readonly int _candidateCount;
        private readonly CsvTableDefinition _drawsTable;
        private readonly CsvTableDefinition _candidatesTable;

        /// <summary>
        /// Mission抽選診断設定から整形器を生成します。
        /// </summary>
        /// <param name="options">CSV出力に含める候補数を持つMission抽選診断設定。</param>
        /// <exception cref="ArgumentNullException">設定がnullの場合。</exception>
        public MissionDrawTraceCsvFormatter(MissionDrawDiagnosticsOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _candidateCount = options.CsvCandidateCount;
            _drawsTable = new CsvTableDefinition(
                DrawsFileSuffix,
                DrawColumns.Select(column => column.Name).ToArray());
            _candidatesTable = new CsvTableDefinition(
                CandidatesFileSuffix,
                CandidateColumns.Select(column => column.Name).ToArray());
            Tables = new[] { _drawsTable, _candidatesTable };
        }

        /// <summary>
        /// draws表とcandidates表の定義一覧を取得します。
        /// </summary>
        public IReadOnlyList<CsvTableDefinition> Tables { get; }

        /// <summary>
        /// Mission抽選Traceをdraws表1行と、順位上位候補のcandidates表の行へ整形します。
        /// </summary>
        /// <param name="trace">整形する抽選Trace。</param>
        /// <param name="context">Session情報とSession内の抽選通し番号。</param>
        /// <returns>draws表とcandidates表の追記データ。</returns>
        /// <exception cref="ArgumentNullException">引数がnullの場合。</exception>
        public IReadOnlyList<CsvTableRows> Format(MissionDrawTrace trace, DiagnosticsTraceContext context)
        {
            if (trace == null)
            {
                throw new ArgumentNullException(nameof(trace));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var drawSource = new DrawRowSource(trace, context);
            IReadOnlyList<string>[] drawRows =
            {
                DrawColumns.Select(column => column.SelectField(drawSource)).ToArray()
            };

            int rowCount = Math.Min(_candidateCount, trace.RankedCandidates.Count);
            var candidateRows = new IReadOnlyList<string>[rowCount];
            for (int index = 0; index < rowCount; index++)
            {
                var candidateSource = new CandidateRowSource(
                    trace,
                    context,
                    index + 1,
                    trace.RankedCandidates[index]);
                candidateRows[index] = CandidateColumns
                    .Select(column => column.SelectField(candidateSource))
                    .ToArray();
            }

            return new[]
            {
                new CsvTableRows(_drawsTable, drawRows),
                new CsvTableRows(_candidatesTable, candidateRows)
            };
        }

        private static string FormatNumber(int number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatNumber(float number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        private readonly struct DrawRowSource
        {
            public DrawRowSource(MissionDrawTrace trace, DiagnosticsTraceContext context)
            {
                Trace = trace;
                Context = context;
            }

            public MissionDrawTrace Trace { get; }

            public DiagnosticsTraceContext Context { get; }
        }

        private readonly struct CandidateRowSource
        {
            public CandidateRowSource(
                MissionDrawTrace trace,
                DiagnosticsTraceContext context,
                int rank,
                MissionCandidate candidate)
            {
                Trace = trace;
                Context = context;
                Rank = rank;
                Candidate = candidate;
            }

            public MissionDrawTrace Trace { get; }

            public DiagnosticsTraceContext Context { get; }

            public int Rank { get; }

            public MissionCandidate Candidate { get; }
        }
    }
}
