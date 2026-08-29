namespace Proto.UI
{
    /// <summary>
    ///     CycleSelectorView に渡す完成済みの表示状態。
    ///     ValueText は Presenter または画面 Model で生成され、View 側では数値・enum・ひらがなを文字列へ変換しない。
    /// </summary>
    public readonly struct CycleSelectorStateDto
    {
        public CycleSelectorStateDto(string valueText, bool isInteractable)
        {
            ValueText = valueText ?? throw new System.ArgumentNullException(nameof(valueText));
            IsInteractable = isInteractable;
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
    }
}
