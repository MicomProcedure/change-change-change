namespace Ccc.Core.UseCases
{
    /// <summary>
    /// SectionTimerの現在状態を表します。
    /// </summary>
    public enum SectionTimerStatus
    {
        /// <summary>
        /// タイマーが進行中です。
        /// </summary>
        Running = 0,

        /// <summary>
        /// タイマーが停止しています。
        /// </summary>
        Stopped,

        /// <summary>
        /// タイマーが満了しています。
        /// </summary>
        Expired
    }
}
