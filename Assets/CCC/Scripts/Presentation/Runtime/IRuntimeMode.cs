namespace Ccc.Presentation
{
    /// <summary>
    /// アプリケーションの実行モードを提供する契約です。
    /// </summary>
    public interface IRuntimeMode
    {
        /// <summary>
        /// DebugModeで実行しているかどうかを取得します。
        /// </summary>
        bool IsDebugMode { get; }
    }
}
