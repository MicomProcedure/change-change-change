namespace Ccc.Presentation
{
    /// <summary>
    /// Gameplay画面で式の左辺・演算子・右辺を表示するスタイルを表します。
    /// </summary>
    public enum ExpressionDisplayStyle
    {
        /// <summary>
        /// 選択肢の中での位置を表示しないスタイルです。
        /// </summary>
        Plain = 0,

        /// <summary>
        /// 選択肢の中での位置をIndicatorで表示するスタイルです。
        /// </summary>
        ChoiceIndicator
    }
}
