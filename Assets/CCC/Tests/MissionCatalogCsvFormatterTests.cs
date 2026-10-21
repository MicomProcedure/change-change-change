#nullable enable
using System.Collections.Generic;
using System.Linq;
using Ccc.Infrastructure.Logging;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// MissionCatalogのCSV整形の振る舞いを検証します。
    /// </summary>
    public sealed class MissionCatalogCsvFormatterTests
    {
        private const string CatalogFileSuffix = "catalog";

        /// <summary>
        /// CSV整形がcatalog表だけを定義し、全行のフィールド数が列数と一致することを検証します。
        /// </summary>
        [Test]
        public void Format_ShouldDeclareCatalogTableOnly()
        {
            var formatter = new MissionCatalogCsvFormatter();

            IReadOnlyList<(CsvTableDefinition Table, IReadOnlyList<string> Fields)> rows = Format(formatter);

            Assert.That(formatter.Tables.Select(table => table.FileSuffix), Is.EqualTo(new[] { CatalogFileSuffix }));
            Assert.That(rows.Select(row => row.Table), Is.All.SameAs(formatter.Tables[0]));
            Assert.That(rows.Select(row => row.Fields.Count == row.Table.Columns.Count), Is.All.True);
        }

        /// <summary>
        /// Catalogの全候補をCatalog内の順序で、1始まりの番号、条件、適合率とともに1行ずつ出力することを検証します。
        /// </summary>
        [Test]
        public void Format_ShouldWriteEveryCandidateInCatalogOrder()
        {
            var formatter = new MissionCatalogCsvFormatter();

            Dictionary<string, string>[] rows = Format(formatter)
                .Select(row => row.Table.Columns
                    .Select((column, index) => new { column, field = row.Fields[index] })
                    .ToDictionary(entry => entry.column, entry => entry.field))
                .ToArray();

            Assert.That(rows, Has.Length.EqualTo(3));
            Assert.That(rows.Select(row => row["SessionId"]), Is.All.EqualTo("20261005_143012"));
            Assert.That(rows.Select(row => row["CatalogIndex"]), Is.EqualTo(new[] { "1", "2", "3" }));
            Assert.That(rows.Select(row => row["Kind"]), Is.EqualTo(new[] { "Exact", "Exact", "Between" }));
            Assert.That(rows.Select(row => row["IsNegated"]), Is.EqualTo(new[] { "False", "True", "False" }));
            Assert.That(rows[2]["ParameterA"], Is.EqualTo("3"));
            Assert.That(rows[2]["ParameterB"], Is.EqualTo("4"));
            Assert.That(rows[1]["MatchCount"], Is.EqualTo("7"));
            Assert.That(rows[1]["EvaluatedCount"], Is.EqualTo("10"));
            Assert.That(rows[1]["Ratio"], Is.EqualTo("0.7000"));
        }

        private static IReadOnlyList<(CsvTableDefinition Table, IReadOnlyList<string> Fields)> Format(
            MissionCatalogCsvFormatter formatter)
        {
            return formatter.Format(
                MissionDrawTraceTestFactory.CreateCatalog(),
                MissionDrawTraceTestFactory.CreateSession(),
                traceIndex: 1);
        }
    }
}
