namespace Ccc.Presentation
{
    /// <summary>
    /// GameModeとGameDifficultyの組ごとに決まる、Gameplay画面の表示設定です。
    /// </summary>
    public sealed class GameplayDisplaySettingsDto
    {
        /// <summary>
        /// 式を表示するスタイルを取得します。
        /// </summary>
        public ExpressionDisplayStyle ExpressionDisplayStyle { get; }

        /// <summary>
        /// 指定された値から表示設定を生成します。
        /// </summary>
        /// <param name="expressionDisplayStyle">式を表示するスタイル。</param>
        public GameplayDisplaySettingsDto(ExpressionDisplayStyle expressionDisplayStyle)
        {
            ExpressionDisplayStyle = expressionDisplayStyle;
        }
    }
}
