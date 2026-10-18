namespace Ccc.Core.UseCases
{
    /// <summary>
    /// UseCaseが組み立てた構造化診断Traceを外側の出力先へ渡すポートです。
    /// </summary>
    /// <typeparam name="TTrace">報告する診断Traceの型。</typeparam>
    public interface IDiagnosticsReporter<in TTrace>
    {
        /// <summary>
        /// 診断Traceを受け付ける出力先が有効かどうかを取得します。falseの場合、呼び出し側はTraceの組み立てを省略できます。
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 診断Traceを報告します。
        /// </summary>
        /// <param name="trace">報告する診断Trace。</param>
        void Report(TTrace trace);
    }
}
