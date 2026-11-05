namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 保存済みのハイスコア記録に対する扱いを表します。
    /// </summary>
    public enum HighScoreCompatibilityAction
    {
        /// <summary>
        /// 記録をそのまま使い続けます。
        /// </summary>
        Keep = 0,

        /// <summary>
        /// 記録を破棄します。
        /// </summary>
        Discard,

        /// <summary>
        /// 記録を変換後の内容へ書き換えます。
        /// </summary>
        Replace
    }
}
