using System;
using Ccc.Core.UseCases;
using Microsoft.Extensions.Logging;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 内側の診断Reporterで発生した例外を呼び出し元へ伝播させず、警告Logへ置き換えるDecoratorです。
    /// </summary>
    /// <typeparam name="TTrace">報告する診断Traceの型。</typeparam>
    public sealed class SafeDiagnosticsReporter<TTrace> : IDiagnosticsReporter<TTrace>
    {
        private readonly IDiagnosticsReporter<TTrace> _innerReporter;
        private readonly ILogger _failureLogger;

        /// <summary>
        /// 保護対象のReporterと失敗通知用Loggerから保護Decoratorを生成します。
        /// </summary>
        /// <param name="innerReporter">例外から保護する内側のReporter。</param>
        /// <param name="failureLogger">内側のReporterの失敗を警告として記録するLogger。</param>
        /// <exception cref="ArgumentNullException">引数がnullの場合。</exception>
        public SafeDiagnosticsReporter(IDiagnosticsReporter<TTrace> innerReporter, ILogger failureLogger)
        {
            _innerReporter = innerReporter ?? throw new ArgumentNullException(nameof(innerReporter));
            _failureLogger = failureLogger ?? throw new ArgumentNullException(nameof(failureLogger));
        }

        /// <summary>
        /// 内側のReporterが有効かどうかを取得します。判定中に例外が発生した場合はfalseです。
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                try
                {
                    return _innerReporter.IsEnabled;
                }
                catch (Exception exception)
                {
                    LogFailure(exception);
                    return false;
                }
            }
        }

        /// <summary>
        /// 内側のReporterへ診断Traceを渡し、発生した例外を警告Logへ置き換えます。
        /// </summary>
        /// <param name="trace">報告する診断Trace。</param>
        public void Report(TTrace trace)
        {
            try
            {
                _innerReporter.Report(trace);
            }
            catch (Exception exception)
            {
                LogFailure(exception);
            }
        }

        private void LogFailure(Exception exception)
        {
            try
            {
                _failureLogger.LogWarning(
                    exception,
                    "Diagnostics reporter {DiagnosticsReporterType} failed.",
                    _innerReporter.GetType().Name);
            }
            catch (Exception)
            {
                // 診断出力の失敗通知まで失敗しても、呼び出し元のUseCaseへ影響させない。
            }
        }
    }
}
