using System;

namespace Ccc.Presentation
{
    /// <summary>
    /// アプリケーションの実行モードを保持する不変実装です。
    /// </summary>
    public sealed class RuntimeMode : IRuntimeMode
    {
        /// <summary>
        /// RuntimeModeを生成します。
        /// </summary>
        /// <param name="isDebugMode">DebugModeで実行するかどうか。</param>
        public RuntimeMode(bool isDebugMode)
        {
            IsDebugMode = isDebugMode;
        }

        /// <summary>
        /// DebugModeで実行しているかどうかを取得します。
        /// </summary>
        public bool IsDebugMode { get; }
    }
}
