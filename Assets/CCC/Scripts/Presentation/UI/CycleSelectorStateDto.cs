namespace Ccc.Presentation.UI
{
    /// <summary>
    ///     CycleSelectorView に渡す完成済みの表示状態。
    ///     ValueText は Presenter または画面 Model で生成され、View 側では数値・enum・ひらがなを文字列へ変換しない。
    /// </summary>
    public readonly struct CycleSelectorStateDto
    {
        /// <summary>
        ///     表示状態を生成する。
        /// </summary>
        /// <param name="valueText">表示用の文字列。</param>
        /// <param name="isInteractable">入力を受け付けるかどうか。</param>
        /// <param name="selectedIndex">循環順で0始まりの現在位置。負の値の場合はどの位置も選択しない。</param>
        /// <param name="choiceCount">選択肢の総数。0の場合は位置を表示しない。</param>
        public CycleSelectorStateDto(
            string valueText,
            bool isInteractable,
            int selectedIndex = 0,
            int choiceCount = 0)
        {
            ValueText = valueText ?? throw new System.ArgumentNullException(nameof(valueText));
            IsInteractable = isInteractable;
            SelectedIndex = selectedIndex;
            ChoiceCount = choiceCount;
        }

        /// <summary>
        ///     表示用の文字列。
        ///     Presenter または画面 Model が生成した完成済みの値をそのまま渡す。
        /// </summary>
        public string ValueText { get; }

        /// <summary>
        ///     View の入力受付可否。
        ///     false の場合は PointerDown を通知せず、無効状態の見た目を描画する。
        /// </summary>
        public bool IsInteractable { get; }

        /// <summary>
        ///     循環順で0始まりの現在位置。負の値の場合はどの位置も選択しない。
        /// </summary>
        public int SelectedIndex { get; }

        /// <summary>
        ///     選択肢の総数。0の場合は位置を表示しない。
        /// </summary>
        public int ChoiceCount { get; }
    }
}
