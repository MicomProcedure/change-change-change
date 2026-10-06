using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ccc.Core.UseCases;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 診断TraceをSession単位のCSVファイル群へ追記するReporterです。ファイルは最初の報告時に作成し、Dispose時に閉じます。
    /// </summary>
    /// <typeparam name="TTrace">報告する診断Traceの型。</typeparam>
    public sealed class CsvDiagnosticsReporter<TTrace> : IDiagnosticsReporter<TTrace>, IDisposable
    {
        private const string FileNameSeparator = "_";
        private const string FileExtension = ".csv";
        private const string LineSeparator = "\r\n";

        private readonly ICsvTraceFormatter<TTrace> _formatter;
        private readonly DiagnosticsSessionDescriptor _session;
        private readonly string _directoryPath;
        private readonly Dictionary<string, StreamWriter> _writersByFileSuffix =
            new Dictionary<string, StreamWriter>();

        private int _reportedTraceCount;
        private bool _isDisposed;

        /// <summary>
        /// CSV整形器、Session識別情報、出力ディレクトリからReporterを生成します。
        /// </summary>
        /// <param name="formatter">診断TraceのCSV整形器。</param>
        /// <param name="session">ファイル名と各行に使うSession識別情報。</param>
        /// <param name="directoryPath">CSVファイルを作成するディレクトリの絶対パス。</param>
        /// <exception cref="ArgumentNullException">整形器またはSession識別情報がnullの場合。</exception>
        /// <exception cref="ArgumentException">ディレクトリパスが空白の場合。</exception>
        public CsvDiagnosticsReporter(
            ICsvTraceFormatter<TTrace> formatter,
            DiagnosticsSessionDescriptor session,
            string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("Directory path must not be blank.", nameof(directoryPath));
            }

            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _directoryPath = directoryPath;
        }

        /// <summary>
        /// Dispose前であればtrueを取得します。
        /// </summary>
        public bool IsEnabled => !_isDisposed;

        /// <summary>
        /// 診断TraceをCSV行へ整形し、対応するファイルへ追記してFlushします。
        /// </summary>
        /// <param name="trace">報告する診断Trace。</param>
        /// <exception cref="ObjectDisposedException">Dispose後に呼び出した場合。</exception>
        /// <exception cref="IOException">同名のCSVファイルが既に存在する場合、または書き込みに失敗した場合。</exception>
        /// <exception cref="InvalidOperationException">整形器が未定義の表へ行を返した場合、または行のフィールド数が列数と一致しない場合。</exception>
        public void Report(TTrace trace)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            EnsureWritersOpened();
            _reportedTraceCount++;
            IReadOnlyList<(CsvTableDefinition Table, IReadOnlyList<string> Fields)> rows =
                _formatter.Format(trace, _session, _reportedTraceCount);
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                (CsvTableDefinition table, IReadOnlyList<string> fields) = rows[rowIndex];
                if (!_writersByFileSuffix.TryGetValue(table.FileSuffix, out StreamWriter writer))
                {
                    throw new InvalidOperationException(
                        $"CSV formatter returned a row for an undeclared table '{table.FileSuffix}'.");
                }

                if (fields.Count != table.Columns.Count)
                {
                    throw new InvalidOperationException(
                        $"CSV row for table '{table.FileSuffix}' must contain {table.Columns.Count} fields.");
                }

                writer.Write(CsvLineFormatter.FormatLine(fields));
                writer.Write(LineSeparator);
            }

            foreach (StreamWriter writer in _writersByFileSuffix.Values)
            {
                writer.Flush();
            }
        }

        /// <summary>
        /// 開いているCSVファイルをすべて閉じます。2回目以降の呼び出しは何もしません。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            foreach (StreamWriter writer in _writersByFileSuffix.Values)
            {
                writer.Dispose();
            }

            _writersByFileSuffix.Clear();
        }

        private void EnsureWritersOpened()
        {
            if (_writersByFileSuffix.Count > 0)
            {
                return;
            }

            Directory.CreateDirectory(_directoryPath);
            var openedWriters = new Dictionary<string, StreamWriter>();
            try
            {
                IReadOnlyList<CsvTableDefinition> tables = _formatter.Tables;
                for (int index = 0; index < tables.Count; index++)
                {
                    CsvTableDefinition table = tables[index];
                    StreamWriter writer = CreateWriter(table);
                    openedWriters.Add(table.FileSuffix, writer);
                    writer.Write(CsvLineFormatter.FormatLine(table.Columns));
                    writer.Write(LineSeparator);
                    writer.Flush();
                }
            }
            catch (Exception)
            {
                foreach (StreamWriter writer in openedWriters.Values)
                {
                    writer.Dispose();
                }

                throw;
            }

            foreach (KeyValuePair<string, StreamWriter> entry in openedWriters)
            {
                _writersByFileSuffix.Add(entry.Key, entry.Value);
            }
        }

        private StreamWriter CreateWriter(CsvTableDefinition table)
        {
            string filePath = Path.Combine(
                _directoryPath,
                _session.SessionId + FileNameSeparator + table.FileSuffix + FileExtension);
            var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            return new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    }
}
