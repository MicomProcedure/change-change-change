#nullable enable
using System.Collections.Generic;
using System.Linq;
using Ccc.Infrastructure.Logging;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// Mission抽選TraceのテキストLog整形、CSV整形、CSV行エスケープの振る舞いを検証します。
    /// </summary>
    public sealed class MissionDrawTraceFormatterTests
    {
        /// <summary>
        /// テキスト整形が見出し3行の後に設定件数分の候補行を出力することを検証します。
        /// </summary>
        [Test]
        public void TextFormatter_ShouldListConfiguredNumberOfRankedCandidates()
        {
            var formatter = new MissionDrawTraceTextFormatter(
                MissionDrawTraceTestFactory.CreateOptions(logCandidateCount: 2));

            string[] lines = formatter.Format(MissionDrawTraceTestFactory.CreateTrace())
                .Split('\n')
                .Select(line => line.TrimEnd('\r'))
                .ToArray();

            Assert.That(lines, Has.Length.EqualTo(5));
            Assert.That(lines[0], Does.StartWith("Mission draw: Section=5"));
            Assert.That(lines[1], Does.Contain("Expression=Unmatched(18; Satisfied=12, Unmatched=18)"));
            Assert.That(lines[2], Does.Contain("SelectedRank=2"));
            Assert.That(lines[3], Does.StartWith("#1 ").And.EndWith("[Pool]"));
            Assert.That(lines[4], Does.StartWith("#2 ").And.EndWith("[Pool] [Selected]"));
        }

        /// <summary>
        /// テキスト整形が順位候補数を超える設定件数を候補数で打ち切ることを検証します。
        /// </summary>
        [Test]
        public void TextFormatter_ShouldNotExceedRankedCandidateCount()
        {
            var formatter = new MissionDrawTraceTextFormatter(
                MissionDrawTraceTestFactory.CreateOptions(logCandidateCount: 100));

            string[] lines = formatter.Format(MissionDrawTraceTestFactory.CreateTrace()).Split('\n');

            Assert.That(lines, Has.Length.EqualTo(6));
            Assert.That(lines[5], Does.StartWith("#3 ").And.Not.Contain("[Pool]"));
        }

        /// <summary>
        /// CSV整形がdraws表とcandidates表を定義し、全行のフィールド数が列数と一致することを検証します。
        /// </summary>
        [Test]
        public void CsvFormatter_ShouldDeclareDrawsAndCandidatesTables()
        {
            var formatter = new MissionDrawTraceCsvFormatter(MissionDrawTraceTestFactory.CreateOptions());

            IReadOnlyList<CsvTableRows> tableRows = formatter.Format(
                MissionDrawTraceTestFactory.CreateTrace(),
                MissionDrawTraceTestFactory.CreateContext());

            Assert.That(formatter.Tables.Select(table => table.FileSuffix), Is.EqualTo(new[] { "draws", "candidates" }));
            Assert.That(tableRows.Select(rows => rows.Table), Is.EqualTo(formatter.Tables));
            Assert.That(
                tableRows.SelectMany(rows => rows.Rows.Select(row => row.Count == rows.Table.Columns.Count)),
                Is.All.True);
        }

        /// <summary>
        /// draws表の1行がSession情報、抽選番号、段階別の選定結果、選出候補の順位を持つことを検証します。
        /// </summary>
        [Test]
        public void CsvFormatter_ShouldWriteOneDrawRowWithSessionAndStageValues()
        {
            var formatter = new MissionDrawTraceCsvFormatter(MissionDrawTraceTestFactory.CreateOptions());

            CsvTableRows drawRows = formatter.Format(
                    MissionDrawTraceTestFactory.CreateTrace(),
                    MissionDrawTraceTestFactory.CreateContext(traceIndex: 7))
                .Single(rows => rows.Table.FileSuffix == "draws");

            Assert.That(drawRows.Rows, Has.Count.EqualTo(1));
            Dictionary<string, string> fields = ToFields(drawRows.Table, drawRows.Rows[0]);
            Assert.That(fields["SessionId"], Is.EqualTo("20261005_143012"));
            Assert.That(fields["GameMode"], Is.EqualTo("LifeEndless"));
            Assert.That(fields["TargetExpressionMatchRatio"], Is.EqualTo("0.2"));
            Assert.That(fields["TierMinimumSections"], Is.EqualTo("1;4;8;12"));
            Assert.That(fields["DrawIndex"], Is.EqualTo("7"));
            Assert.That(fields["SelectedExpressionGroup"], Is.EqualTo("Unmatched"));
            Assert.That(fields["ExpressionStageCount"], Is.EqualTo("18"));
            Assert.That(fields["SelectedPolarity"], Is.EqualTo("Positive"));
            Assert.That(fields["KindStageCount"], Is.EqualTo("3"));
            Assert.That(fields["SelectedRank"], Is.EqualTo("2"));
            Assert.That(fields["SelectedRatio"], Is.EqualTo("0.3000"));
            Assert.That(fields["SelectedDifference"], Is.EqualTo("0.1000"));
        }

        /// <summary>
        /// candidates表が設定件数分の上位候補だけを出力することを検証します。
        /// </summary>
        [Test]
        public void CsvFormatter_ShouldWriteConfiguredNumberOfCandidateRows()
        {
            var formatter = new MissionDrawTraceCsvFormatter(
                MissionDrawTraceTestFactory.CreateOptions(csvCandidateCount: 2));

            CsvTableRows candidateRows = formatter.Format(
                    MissionDrawTraceTestFactory.CreateTrace(),
                    MissionDrawTraceTestFactory.CreateContext())
                .Single(rows => rows.Table.FileSuffix == "candidates");

            Assert.That(candidateRows.Rows, Has.Count.EqualTo(2));
        }

        /// <summary>
        /// candidates表の各行が順位、Pool内かどうか、選出候補かどうかを持つことを検証します。
        /// </summary>
        [Test]
        public void CsvFormatter_ShouldMarkPoolAndSelectedCandidateRows()
        {
            var formatter = new MissionDrawTraceCsvFormatter(
                MissionDrawTraceTestFactory.CreateOptions(csvCandidateCount: 3));

            CsvTableRows candidateRows = formatter.Format(
                    MissionDrawTraceTestFactory.CreateTrace(poolSize: 2),
                    MissionDrawTraceTestFactory.CreateContext())
                .Single(rows => rows.Table.FileSuffix == "candidates");

            Dictionary<string, string>[] rows = candidateRows.Rows
                .Select(row => ToFields(candidateRows.Table, row))
                .ToArray();
            Assert.That(rows.Select(row => row["Rank"]), Is.EqualTo(new[] { "1", "2", "3" }));
            Assert.That(rows.Select(row => row["IsInPool"]), Is.EqualTo(new[] { "True", "True", "False" }));
            Assert.That(rows.Select(row => row["IsSelected"]), Is.EqualTo(new[] { "False", "True", "False" }));
        }

        /// <summary>
        /// CSV行の連結が区切り文字、引用符、改行を含むフィールドだけを引用符で囲むことを検証します。
        /// </summary>
        [Test]
        public void CsvLineFormatter_ShouldQuoteOnlyFieldsRequiringEscape()
        {
            string line = CsvLineFormatter.FormatLine(new[] { "plain", "a,b", "say \"hi\"", "line\nbreak", "" });

            Assert.That(line, Is.EqualTo("plain,\"a,b\",\"say \"\"hi\"\"\",\"line\nbreak\","));
        }

        private static Dictionary<string, string> ToFields(CsvTableDefinition table, IReadOnlyList<string> row)
        {
            return table.Columns
                .Select((column, index) => new { column, field = row[index] })
                .ToDictionary(entry => entry.column, entry => entry.field);
        }
    }
}
