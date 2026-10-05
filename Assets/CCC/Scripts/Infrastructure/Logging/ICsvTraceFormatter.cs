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
        /// 診断Traceを表ごとの行一覧へ整形します。
        /// </summary>
        /// <param name="trace">整形する診断Trace。</param>
        /// <param name="context">Session情報とSession内通し番号。</param>
        /// <returns><see cref="Tables"/>のいずれかを対象とする追記データ一覧。</returns>
        IReadOnlyList<CsvTableRows> Format(TTrace trace, DiagnosticsTraceContext context);
    }
}
