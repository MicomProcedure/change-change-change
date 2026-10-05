using System;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 診断TraceをCSV行へ整形するときに付与するSession情報とSession内通し番号を保持する不変値です。
    /// </summary>
    public sealed class DiagnosticsTraceContext
    {
        /// <summary>
        /// Traceが属するSessionの識別情報を取得します。
        /// </summary>
        public DiagnosticsSessionDescriptor Session { get; }

        /// <summary>
        /// Session内で1から始まるTraceの通し番号を取得します。
        /// </summary>
        public int TraceIndex { get; }

        /// <summary>
        /// Session識別情報と通し番号から整形Contextを生成します。
        /// </summary>
        /// <param name="session">Traceが属するSessionの識別情報。</param>
        /// <param name="traceIndex">Session内で1から始まるTraceの通し番号。</param>
        /// <exception cref="ArgumentNullException">Session識別情報がnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">通し番号が1未満の場合。</exception>
        public DiagnosticsTraceContext(DiagnosticsSessionDescriptor session, int traceIndex)
        {
            if (traceIndex < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(traceIndex),
                    traceIndex,
                    "Trace index must start from one.");
            }

            Session = session ?? throw new ArgumentNullException(nameof(session));
            TraceIndex = traceIndex;
        }
    }
}
