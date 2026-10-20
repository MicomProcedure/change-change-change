using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ccc.Core.Model;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// MissionCatalogを、Catalog候補1件を1行とするcatalog表へ整形します。
    /// </summary>
    public sealed class MissionCatalogCsvFormatter : ICsvTraceFormatter<MissionCatalog>
    {
        private const string CatalogFileSuffix = "catalog";
        private const string RatioFormat = "0.0000";

        private static readonly Column<CatalogRowSource>[] CatalogColumns =
        {
            new Column<CatalogRowSource>("SessionId", row => row.Session.SessionId),
            new Column<CatalogRowSource>("CatalogIndex", row => FormatNumber(row.CatalogIndex)),
            new Column<CatalogRowSource>("Tier", row => FormatNumber(row.Candidate.Tier)),
            new Column<CatalogRowSource>("Kind", row => row.Candidate.Condition.Kind.ToString()),
            new Column<CatalogRowSource>(
                "ParameterA",
                row => FormatNumber(row.Candidate.Condition.ParameterA)),
            new Column<CatalogRowSource>(
                "ParameterB",
                row => FormatNumber(row.Candidate.Condition.ParameterB)),
            new Column<CatalogRowSource>(
                "IsNegated",
                row => row.Candidate.Condition.IsNegated.ToString()),
            new Column<CatalogRowSource>(
                "MatchCount",
                row => FormatNumber(row.Candidate.ExpressionMatchCount)),
            new Column<CatalogRowSource>(
                "EvaluatedCount",
                row => FormatNumber(row.Candidate.EvaluatedExpressionCount)),
            new Column<CatalogRowSource>(
                "Ratio",
                row => FormatRatio(row.Candidate.ExpressionMatchRatio))
        };

        private readonly CsvTableDefinition _catalogTable;

        /// <summary>
        /// catalog表の定義を持つ整形器を生成します。
        /// </summary>
        public MissionCatalogCsvFormatter()
        {
            _catalogTable = new CsvTableDefinition(
                CatalogFileSuffix,
                CatalogColumns.Select(column => column.Name).ToArray());
            Tables = new[] { _catalogTable };
        }

        /// <summary>
        /// catalog表の定義一覧を取得します。
        /// </summary>
        public IReadOnlyList<CsvTableDefinition> Tables { get; }

        /// <summary>
        /// MissionCatalogの全候補を、Catalog内の順序でcatalog表の行へ整形します。
        /// </summary>
        /// <param name="trace">整形するMissionCatalog。</param>
        /// <param name="session">Catalogが属するSessionの識別情報。</param>
        /// <param name="traceIndex">Session内の報告の通し番号。Catalogは1 Sessionで1回だけ報告されるため行には含めません。</param>
        /// <returns>Catalog候補1件ごとのcatalog表の行。</returns>
        /// <exception cref="ArgumentNullException">Catalogまたは識別情報がnullの場合。</exception>
        public IReadOnlyList<(CsvTableDefinition Table, IReadOnlyList<string> Fields)> Format(
            MissionCatalog trace,
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

            var rows = new List<(CsvTableDefinition Table, IReadOnlyList<string> Fields)>(trace.Candidates.Count);
            for (int index = 0; index < trace.Candidates.Count; index++)
            {
                var source = new CatalogRowSource(session, index + 1, trace.Candidates[index]);
                rows.Add((_catalogTable, CatalogColumns.Select(column => column.SelectField(source)).ToArray()));
            }

            return rows;
        }

        private static string FormatNumber(int number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatRatio(float ratio)
        {
            return ratio.ToString(RatioFormat, CultureInfo.InvariantCulture);
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

        private readonly struct CatalogRowSource
        {
            public CatalogRowSource(DiagnosticsSessionDescriptor session, int catalogIndex, MissionCandidate candidate)
            {
                Session = session;
                CatalogIndex = catalogIndex;
                Candidate = candidate;
            }

            public DiagnosticsSessionDescriptor Session { get; }

            public int CatalogIndex { get; }

            public MissionCandidate Candidate { get; }
        }
    }
}
