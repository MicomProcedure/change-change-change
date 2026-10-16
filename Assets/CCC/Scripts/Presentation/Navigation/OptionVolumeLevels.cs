namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// OptionMenuで使用する音量レベルの範囲を定義します。
    /// </summary>
    public static class OptionVolumeLevels
    {
        /// <summary>
        /// 最小音量レベルです。
        /// </summary>
        public const int Min = 0;

        /// <summary>
        /// 最大音量レベルです。
        /// </summary>
        public const int Max = 5;

        /// <summary>
        /// 既定の音量レベルです。保存値がない場合に使用します。
        /// </summary>
        public const int Default = 3;

        /// <summary>
        /// 音量レベルの段階数です。
        /// </summary>
        public const int Count = Max - Min + 1;
    }
}