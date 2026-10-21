namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// TitleScene内で受け付けた画面遷移の方向を表します。
    /// </summary>
    public enum TitleScreenTransitionDirection
    {
        /// <summary>現在の画面から次の画面へ進む遷移です。</summary>
        Forward = 0,

        /// <summary>現在の画面を閉じて前の画面へ戻る遷移です。</summary>
        Backward
    }
}
