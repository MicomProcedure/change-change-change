using System.Collections.Generic;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 診断Traceを1つ以上のCSV表の行へ整形します。
    /// </summary>
    /// <typeparam name="TTrace">整形する診断Traceの型。</typeparam>
    public interface ICsvTraceFormatter<in TTrace>
    {
        /// <summary>
        /// 整形結果を書き出すCSV表の定義一覧を取得します。
        /// </summary>
        IReadOnlyList<CsvTableDefinition> Tables { get; }

        /// <summary>
        /// 診断Traceを、書き出し先の表と列順のエスケープ前フィールド値の組からなる行一覧へ整形します。
        /// </summary>
        /// <param name="trace">整形する診断Trace。</param>
        /// <param name="session">Traceが属するSessionの識別情報。</param>
        /// <param name="traceIndex">Session内で1から始まるTraceの通し番号。</param>
        /// <returns><see cref="Tables"/>のいずれかを書き出し先とする行一覧。</returns>
        IReadOnlyList<(CsvTableDefinition Table, IReadOnlyList<string> Fields)> Format(
            TTrace trace,
            DiagnosticsSessionDescriptor session,
            int traceIndex);
    }
}
