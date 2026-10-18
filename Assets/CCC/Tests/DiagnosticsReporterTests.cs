#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// 診断ReporterのComposite配信、例外保護Decorator、CSVファイル出力の振る舞いを検証します。
    /// </summary>
    public sealed class DiagnosticsReporterTests
    {
        private string _temporaryDirectoryPath = string.Empty;

        /// <summary>
        /// テストごとに空の一時ディレクトリを用意します。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), "CccDiagnosticsTests", Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// テストで作成した一時ディレクトリを削除します。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_temporaryDirectoryPath))
            {
                Directory.Delete(_temporaryDirectoryPath, recursive: true);
            }
        }

        /// <summary>
        /// Compositeが有効な子Reporterにだけ登録順で配信することを検証します。
        /// </summary>
        [Test]
        public void Composite_ShouldReportOnlyToEnabledReportersInOrder()
        {
            var calls = new List<string>();
            var composite = new CompositeDiagnosticsReporter<string>(new[]
            {
                new RecordingReporter("first", isEnabled: true, calls),
                new RecordingReporter("disabled", isEnabled: false, calls),
                new RecordingReporter("second", isEnabled: true, calls)
            });

            composite.Report("trace");

            Assert.That(composite.IsEnabled, Is.True);
            Assert.That(calls, Is.EqualTo(new[] { "first:trace", "second:trace" }));
        }

        /// <summary>
        /// 有効な子Reporterがない場合にCompositeが無効になることを検証します。
        /// </summary>
        [Test]
        public void Composite_WithoutEnabledReporter_ShouldBeDisabled()
        {
            var composite = new CompositeDiagnosticsReporter<string>(new[]
            {
                new RecordingReporter("disabled", isEnabled: false, new List<string>())
            });

            Assert.That(composite.IsEnabled, Is.False);
        }

        /// <summary>
        /// 保護Decoratorが内側の例外を伝播させず警告Logへ記録することを検証します。
        /// </summary>
        [Test]
        public void Safe_ShouldSwallowInnerExceptionAndLogWarning()
        {
            var failureLogger = new RecordingLogger();
            var reporter = new SafeDiagnosticsReporter<string>(new ThrowingReporter(), failureLogger);

            Assert.DoesNotThrow(() => reporter.Report("trace"));
            Assert.That(failureLogger.Levels, Is.EqualTo(new[] { LogLevel.Warning }));
        }

        /// <summary>
        /// 保護Decoratorが内側の有効判定の例外を無効として扱うことを検証します。
        /// </summary>
        [Test]
        public void Safe_WhenInnerIsEnabledThrows_ShouldBeDisabled()
        {
            var failureLogger = new RecordingLogger();
            var reporter = new SafeDiagnosticsReporter<string>(new ThrowingReporter(), failureLogger);

            Assert.That(reporter.IsEnabled, Is.False);
            Assert.That(failureLogger.Levels, Is.EqualTo(new[] { LogLevel.Warning }));
        }

        /// <summary>
        /// CSV Reporterが最初の報告で表ごとのファイルをヘッダー付きで作成し、報告ごとに行を追記することを検証します。
        /// </summary>
        [Test]
        public void Csv_ShouldCreateFilePerTableAndAppendRows()
        {
            using (var reporter = new CsvDiagnosticsReporter<MissionDrawTrace>(
                       new MissionDrawTraceCsvFormatter(candidateCount: 2),
                       MissionDrawTraceTestFactory.CreateSession(),
                       _temporaryDirectoryPath))
            {
                reporter.Report(MissionDrawTraceTestFactory.CreateTrace());
                reporter.Report(MissionDrawTraceTestFactory.CreateTrace());
            }

            string[] drawLines = ReadLines("20261005_143012_draws.csv");
            string[] candidateLines = ReadLines("20261005_143012_candidates.csv");
            Assert.That(drawLines, Has.Length.EqualTo(3));
            Assert.That(drawLines[0], Does.StartWith("SessionId,ConfigDisplayName,"));
            Assert.That(drawLines[1], Does.StartWith("20261005_143012,\"LifeEndless, Normal\","));
            Assert.That(candidateLines, Has.Length.EqualTo(5));
            Assert.That(candidateLines[3], Does.StartWith("20261005_143012,2,1,"));
        }

        /// <summary>
        /// CSV Reporterが同名ファイルを上書きせず例外にすることを検証します。
        /// </summary>
        [Test]
        public void Csv_WhenFileAlreadyExists_ShouldThrowInsteadOfOverwriting()
        {
            Directory.CreateDirectory(_temporaryDirectoryPath);
            File.WriteAllText(Path.Combine(_temporaryDirectoryPath, "20261005_143012_draws.csv"), "existing");
            using var reporter = new CsvDiagnosticsReporter<MissionDrawTrace>(
                new MissionDrawTraceCsvFormatter(candidateCount: 10),
                MissionDrawTraceTestFactory.CreateSession(),
                _temporaryDirectoryPath);

            Assert.Throws<IOException>(() => reporter.Report(MissionDrawTraceTestFactory.CreateTrace()));
            Assert.That(ReadLines("20261005_143012_draws.csv"), Is.EqualTo(new[] { "existing" }));
        }

        /// <summary>
        /// Dispose後のCSV Reporterが無効になり、報告を拒否することを検証します。
        /// </summary>
        [Test]
        public void Csv_AfterDispose_ShouldBeDisabledAndRejectReport()
        {
            var reporter = new CsvDiagnosticsReporter<MissionDrawTrace>(
                new MissionDrawTraceCsvFormatter(candidateCount: 10),
                MissionDrawTraceTestFactory.CreateSession(),
                _temporaryDirectoryPath);

            reporter.Dispose();

            Assert.That(reporter.IsEnabled, Is.False);
            Assert.Throws<ObjectDisposedException>(() => reporter.Report(MissionDrawTraceTestFactory.CreateTrace()));
        }

        private string[] ReadLines(string fileName)
        {
            return File.ReadAllLines(Path.Combine(_temporaryDirectoryPath, fileName), Encoding.UTF8);
        }

        private sealed class RecordingReporter : IDiagnosticsReporter<string>
        {
            private readonly string _name;
            private readonly List<string> _calls;

            public RecordingReporter(string name, bool isEnabled, List<string> calls)
            {
                _name = name;
                IsEnabled = isEnabled;
                _calls = calls;
            }

            /// <summary>
            /// 生成時に指定された有効状態を取得します。
            /// </summary>
            public bool IsEnabled { get; }

            /// <summary>
            /// 名前とTraceの組を呼び出し記録へ追加します。
            /// </summary>
            /// <param name="trace">報告されたTrace。</param>
            public void Report(string trace)
            {
                _calls.Add(_name + ":" + trace);
            }
        }

        private sealed class ThrowingReporter : IDiagnosticsReporter<string>
        {
            /// <summary>
            /// 常に例外を送出します。
            /// </summary>
            public bool IsEnabled => throw new InvalidOperationException("IsEnabled failed.");

            /// <summary>
            /// 常に例外を送出します。
            /// </summary>
            /// <param name="trace">報告されたTrace。</param>
            public void Report(string trace)
            {
                throw new InvalidOperationException("Report failed.");
            }
        }

        private sealed class RecordingLogger : ILogger
        {
            private readonly List<LogLevel> _levels = new List<LogLevel>();

            /// <summary>
            /// 記録されたLogLevelを記録順に取得します。
            /// </summary>
            public IReadOnlyList<LogLevel> Levels => _levels;

            /// <summary>
            /// Scopeを使用しないためnullを返します。
            /// </summary>
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull
            {
                return null;
            }

            /// <summary>
            /// すべてのLogLevelを有効として扱います。
            /// </summary>
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            /// <summary>
            /// LogLevelを記録します。
            /// </summary>
            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                _levels.Add(logLevel);
            }
        }
    }
}
