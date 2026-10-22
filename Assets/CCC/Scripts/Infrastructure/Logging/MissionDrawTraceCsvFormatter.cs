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
        private const string RatioFormat = "0.0000";
        private const string PositivePolarityName = "Positive";
        private const string NegatedPolarityName = "Negated";

        private static readonly Column<DrawRowSource>[] DrawColumns =
        {
            new Column<DrawRowSource>("SessionId", row => row.Session.SessionId),
            new Column<DrawRowSource>("ConfigDisplayName", row => row.Session.ConfigDisplayName),
            new Column<DrawRowSource>("GameMode", row => row.Session.GameMode.ToString()),
            new Column<DrawRowSource>("Difficulty", row => row.Session.Difficulty.ToString()),
            new Column<DrawRowSource>(
                "TargetExpressionMatchRatio",
                row => FormatNumber(row.Trace.Config.TargetExpressionMatchRatio)),
            new Column<DrawRowSource>(
                "TargetExpressionMatchRatioReductionPerSection",
                row => FormatNumber(row.Trace.Config.TargetExpressionMatchRatioReductionPerSection)),
            new Column<DrawRowSource>(
                "MinimumTargetExpressionMatchRatio",
                row => FormatNumber(row.Trace.Config.MinimumTargetExpressionMatchRatio)),
            new Column<DrawRowSource>(
                "MissionParameterSelectionPoolSize",
                row => FormatNumber(row.Trace.Config.MissionParameterSelectionPoolSize)),
            new Column<DrawRowSource>(
                "NegatedMissionSelectionWeight",
                row => FormatNumber(row.Trace.Config.NegatedMissionSelectionWeight)),
            new Column<DrawRowSource>(
                "CurrentExpressionSatisfiedMissionSelectionRatio",
                row => FormatNumber(row.Trace.Config.CurrentExpressionSatisfiedMissionSelectionRatio)),
            new Column<DrawRowSource>(
                "CurrentTierDrawWeight",
                row => FormatNumber(row.Trace.Config.CurrentTierDrawWeight)),
            new Column<DrawRowSource>(
                "LowerTierDrawWeight",
                row => FormatNumber(row.Trace.Config.LowerTierDrawWeight)),
            new Column<DrawRowSource>(
                "TierMinimumSections",
                row => string.Join(
                    ListSeparator,
                    row.Trace.Config.TierProgression.MinimumSectionsByTier.Select(
                        minimumSection => FormatNumber(minimumSection)))),
            new Column<DrawRowSource>("DrawIndex", row => FormatNumber(row.TraceIndex)),
            new Column<DrawRowSource>("Section", row => FormatNumber(row.Trace.Section)),
            new Column<DrawRowSource>("IsInitialDraw", row => row.Trace.IsInitialDraw.ToString()),
            new Column<DrawRowSource>("CurrentTier", row => FormatNumber(row.Trace.CurrentTier)),
            new Column<DrawRowSource>("EnabledCount", row => FormatNumber(row.Trace.EnabledCandidateCount)),
            new Column<DrawRowSource>("UnlockedCount", row => FormatNumber(row.Trace.UnlockedCandidateCount)),
            new Column<DrawRowSource>("SatisfiedCount", row => FormatNumber(row.Trace.SatisfiedCandidateCount)),
            new Column<DrawRowSource>("UnmatchedCount", row => FormatNumber(row.Trace.UnmatchedCandidateCount)),
            new Column<DrawRowSource>(
                "SelectedExpressionGroup",
                row => row.Trace.SelectedExpressionGroup.ToString()),
            new Column<DrawRowSource>(
                "ExpressionStageCount",
                row => FormatNumber(row.Trace.ExpressionStageCandidateCount)),
            new Column<DrawRowSource>("SelectedTier", row => FormatNumber(row.Trace.SelectedTier)),
            new Column<DrawRowSource>("TierStageCount", row => FormatNumber(row.Trace.TierStageCandidateCount)),
            new Column<DrawRowSource>(
                "SelectedPolarity",
                row => FormatPolarity(row.Trace.IsNegatedPolaritySelected)),
            new Column<DrawRowSource>(
                "PolarityStageCount",
                row => FormatNumber(row.Trace.PolarityStageCandidateCount)),
            new Column<DrawRowSource>(
                "RepeatFilteredCount",
                row => FormatNumber(row.Trace.RepeatFilteredCandidateCount)),
            new Column<DrawRowSource>("SelectedKind", row => row.Trace.SelectedKind.ToString()),
            new Column<DrawRowSource>("KindStageCount", row => FormatNumber(row.Trace.RankedCandidates.Count)),
            new Column<DrawRowSource>(
                "TargetRatioLowerBound",
                row => FormatRatio(row.Trace.TargetExpressionMatchRatioRange.LowerBound)),
            new Column<DrawRowSource>(
                "TargetRatioUpperBound",
                row => FormatRatio(row.Trace.TargetExpressionMatchRatioRange.UpperBound)),
            new Column<DrawRowSource>("TargetRatio", row => FormatRatio(row.Trace.TargetExpressionMatchRatio)),
            new Column<DrawRowSource>("PoolSize", row => FormatNumber(row.Trace.PoolSize)),
            new Column<DrawRowSource>("SelectedRank", row => FormatNumber(row.Trace.SelectedRank)),
            new Column<DrawRowSource>(
                "SelectedParameterA",
                row => FormatNumber(row.Trace.SelectedCandidate.Condition.ParameterA)),
            new Column<DrawRowSource>(
                "SelectedParameterB",
                row => FormatNumber(row.Trace.SelectedCandidate.Condition.ParameterB)),
            new Column<DrawRowSource>(
                "SelectedMatchCount",
                row => FormatNumber(row.Trace.SelectedCandidate.ExpressionMatchCount)),
            new Column<DrawRowSource>(
                "SelectedEvaluatedCount",
                row => FormatNumber(row.Trace.SelectedCandidate.EvaluatedExpressionCount)),
            new Column<DrawRowSource>(
                "SelectedRatio",
                row => FormatRatio(row.Trace.SelectedCandidate.ExpressionMatchRatio)),
            new Column<DrawRowSource>(
                "SelectedDifference",
                row => FormatRatio(CalculateTargetDifference(row.Trace, row.Trace.SelectedCandidate)))
        };

        private static readonly Column<CandidateRowSource>[] CandidateColumns =
        {
            new Column<CandidateRowSource>("SessionId", row => row.Session.SessionId),
            new Column<CandidateRowSource>("DrawIndex", row => FormatNumber(row.TraceIndex)),
            new Column<CandidateRowSource>("Rank", row => FormatNumber(row.Rank)),
            new Column<CandidateRowSource>("Tier", row => FormatNumber(row.Candidate.Tier)),
            new Column<CandidateRowSource>("Kind", row => row.Candidate.Condition.Kind.ToString()),
            new Column<CandidateRowSource>(
                "ParameterA",
                row => FormatNumber(row.Candidate.Condition.ParameterA)),
            new Column<CandidateRowSource>(
                "ParameterB",
                row => FormatNumber(row.Candidate.Condition.ParameterB)),
            new Column<CandidateRowSource>(
                "IsNegated",
                row => row.Candidate.Condition.IsNegated.ToString()),
            new Column<CandidateRowSource>(
                "MatchCount",
                row => FormatNumber(row.Candidate.ExpressionMatchCount)),
            new Column<CandidateRowSource>(
                "EvaluatedCount",
                row => FormatNumber(row.Candidate.EvaluatedExpressionCount)),
            new Column<CandidateRowSource>(
                "Ratio",
                row => FormatRatio(row.Candidate.ExpressionMatchRatio)),
            new Column<CandidateRowSource>(
                "Difference",
                row => FormatRatio(CalculateTargetDifference(row.Trace, row.Candidate))),
            new Column<CandidateRowSource>(
                "IsInPool",
                row => (row.Rank <= row.Trace.PoolSize).ToString()),
            new Column<CandidateRowSource>(
                "IsSelected",
                row => ReferenceEquals(row.Candidate, row.Trace.SelectedCandidate).ToString())
        };

        private readonly int _candidateCount;
        private readonly CsvTableDefinition _drawsTable;
        private readonly CsvTableDefinition _candidatesTable;

        /// <summary>
        /// candidates表に含める順位上位の候補数から整形器を生成します。
        /// </summary>
        /// <param name="candidateCount">candidates表に含める順位上位の候補数。</param>
        /// <exception cref="ArgumentOutOfRangeException">候補数が負の場合。</exception>
        public MissionDrawTraceCsvFormatter(int candidateCount)
        {
            if (candidateCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidateCount),
                    candidateCount,
                    "Candidate count must not be negative.");
            }

            _candidateCount = candidateCount;
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
        /// <param name="session">Traceが属するSessionの識別情報。</param>
        /// <param name="traceIndex">Session内で1から始まる抽選の通し番号。</param>
        /// <returns>draws表1行と、続くcandidates表の行。</returns>
        /// <exception cref="ArgumentNullException">Traceまたは識別情報がnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">通し番号が1未満の場合。</exception>
        public IReadOnlyList<(CsvTableDefinition Table, IReadOnlyList<string> Fields)> Format(
            MissionDrawTrace trace,
            DiagnosticsSessionDescriptor session,
            int traceIndex)
        {
            if (trace == null)
            {
                throw new ArgumentNullException(nameof(trace));
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (traceIndex < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(traceIndex), traceIndex, "Trace index must start from one.");
            }

            var drawSource = new DrawRowSource(trace, session, traceIndex);
            var rows = new List<(CsvTableDefinition Table, IReadOnlyList<string> Fields)>
            {
                (_drawsTable, DrawColumns.Select(column => column.SelectField(drawSource)).ToArray())
            };

            int candidateRowCount = Math.Min(_candidateCount, trace.RankedCandidates.Count);
            for (int index = 0; index < candidateRowCount; index++)
            {
                var candidateSource = new CandidateRowSource(
                    trace,
                    session,
                    traceIndex,
                    index + 1,
                    trace.RankedCandidates[index]);
                rows.Add((
                    _candidatesTable,
                    CandidateColumns.Select(column => column.SelectField(candidateSource)).ToArray()));
            }

            return rows;
        }

        private static string FormatNumber(int number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatNumber(float number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatRatio(float ratio)
        {
            return ratio.ToString(RatioFormat, CultureInfo.InvariantCulture);
        }

        private static string FormatPolarity(bool isNegated)
        {
            return isNegated ? NegatedPolarityName : PositivePolarityName;
        }

        private static float CalculateTargetDifference(MissionDrawTrace trace, MissionCandidate candidate)
        {
            return Math.Abs(candidate.ExpressionMatchRatio - trace.TargetExpressionMatchRatio);
        }

        private sealed class Column<TRow>
        {
            private readonly Func<TRow, string> _selectField;

            public Column(string name, Func<TRow, string> selectField)
            {
                Name = name;
                _selectField = selectField;
            }

            public string Name { get; }

            public string SelectField(TRow row)
            {
                return _selectField(row);
            }
        }

        private readonly struct DrawRowSource
        {
            public DrawRowSource(MissionDrawTrace trace, DiagnosticsSessionDescriptor session, int traceIndex)
            {
                Trace = trace;
                Session = session;
                TraceIndex = traceIndex;
            }

            public MissionDrawTrace Trace { get; }

            public DiagnosticsSessionDescriptor Session { get; }

            public int TraceIndex { get; }
        }

        private readonly struct CandidateRowSource
        {
            public CandidateRowSource(
                MissionDrawTrace trace,
                DiagnosticsSessionDescriptor session,
                int traceIndex,
                int rank,
                MissionCandidate candidate)
            {
                Trace = trace;
                Session = session;
                TraceIndex = traceIndex;
                Rank = rank;
                Candidate = candidate;
            }

            public MissionDrawTrace Trace { get; }

            public DiagnosticsSessionDescriptor Session { get; }

            public int TraceIndex { get; }

            public int Rank { get; }

            public MissionCandidate Candidate { get; }
        }
    }
}
