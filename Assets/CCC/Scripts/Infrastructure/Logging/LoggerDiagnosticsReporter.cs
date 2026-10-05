using System;
using Ccc.Core.UseCases;
using Microsoft.Extensions.Logging;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 診断Traceをテキストへ整形し、Trace型をカテゴリとするLoggerへ書き込むReporterです。
    /// </summary>
    /// <typeparam name="TTrace">報告する診断Traceの型。</typeparam>
    public sealed class LoggerDiagnosticsReporter<TTrace> : IDiagnosticsReporter<TTrace>
    {
        private readonly ILogger<TTrace> _logger;
        private readonly IDiagnosticsTextFormatter<TTrace> _formatter;
        private readonly LogLevel _logLevel;

        /// <summary>
        /// 書き込み先Logger、テキスト整形器、出力LogLevelからReporterを生成します。
        /// </summary>
        /// <param name="logger">Trace型をカテゴリとする書き込み先Logger。</param>
        /// <param name="formatter">診断Traceのテキスト整形器。</param>
        /// <param name="logLevel">書き込みに使うLogLevel。</param>
        /// <exception cref="ArgumentNullException">Loggerまたは整形器がnullの場合。</exception>
        public LoggerDiagnosticsReporter(
            ILogger<TTrace> logger,
            IDiagnosticsTextFormatter<TTrace> formatter,
            LogLevel logLevel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _logLevel = logLevel;
        }

        /// <summary>
        /// 出力LogLevelがLoggerで有効かどうかを取得します。
        /// </summary>
        public bool IsEnabled => _logger.IsEnabled(_logLevel);

        /// <summary>
        /// 診断Traceをテキストへ整形してLoggerへ書き込みます。
        /// </summary>
        /// <param name="trace">報告する診断Trace。</param>
        public void Report(TTrace trace)
        {
            _logger.Log(_logLevel, "{DiagnosticsText}", _formatter.Format(trace));
        }
    }
}
