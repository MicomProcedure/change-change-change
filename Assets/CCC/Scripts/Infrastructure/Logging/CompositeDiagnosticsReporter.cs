using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.UseCases;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 1つの診断Traceを対等な複数の出力先Reporterへ登録順に配信するCompositeです。
    /// </summary>
    /// <typeparam name="TTrace">報告する診断Traceの型。</typeparam>
    public sealed class CompositeDiagnosticsReporter<TTrace> : IDiagnosticsReporter<TTrace>
    {
        private readonly IDiagnosticsReporter<TTrace>[] _reporters;

        /// <summary>
        /// 配信先Reporter一覧からCompositeを生成します。
        /// </summary>
        /// <param name="reporters">登録順に配信する出力先Reporter一覧。</param>
        /// <exception cref="ArgumentNullException">一覧がnull、またはnull要素を含む場合。</exception>
        public CompositeDiagnosticsReporter(IReadOnlyList<IDiagnosticsReporter<TTrace>> reporters)
        {
            if (reporters == null || reporters.Any(reporter => reporter == null))
            {
                throw new ArgumentNullException(nameof(reporters));
            }

            _reporters = reporters.ToArray();
        }

        /// <summary>
        /// 有効な出力先Reporterが1つ以上あるかどうかを取得します。
        /// </summary>
        public bool IsEnabled => _reporters.Any(reporter => reporter.IsEnabled);

        /// <summary>
        /// 有効な出力先Reporterへ登録順に診断Traceを配信します。
        /// </summary>
        /// <param name="trace">報告する診断Trace。</param>
        public void Report(TTrace trace)
        {
            for (int index = 0; index < _reporters.Length; index++)
            {
                IDiagnosticsReporter<TTrace> reporter = _reporters[index];
                if (reporter.IsEnabled)
                {
                    reporter.Report(trace);
                }
            }
        }
    }
}
