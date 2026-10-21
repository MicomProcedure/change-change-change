namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// TitleScene内の画面状態を表します。
    /// </summary>
    public enum TitleScreenState
    {
        /// <summary>タイトル画面です。</summary>
        TitleScreen = 0,

        /// <summary>メインメニューです。</summary>
        MainMenu,

        /// <summary>ゲーム開始条件を選択する画面です。</summary>
        Stage,

        /// <summary>オプション画面です。</summary>
        OptionMenu,

        /// <summary>クレジット画面です。</summary>
        Credit,

        /// <summary>チュートリアル画像を表示する画面です。</summary>
        Tutorial

    }
}
