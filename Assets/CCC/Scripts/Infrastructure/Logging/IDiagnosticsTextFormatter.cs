namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// 診断TraceをLog出力用の人が読むテキストへ整形します。
    /// </summary>
    /// <typeparam name="TTrace">整形する診断Traceの型。</typeparam>
    public interface IDiagnosticsTextFormatter<in TTrace>
    {
        /// <summary>
        /// 診断Traceを複数行のテキストへ整形します。
        /// </summary>
        /// <param name="trace">整形する診断Trace。</param>
        /// <returns>Log出力用のテキスト。</returns>
        string Format(TTrace trace);
    }
}
