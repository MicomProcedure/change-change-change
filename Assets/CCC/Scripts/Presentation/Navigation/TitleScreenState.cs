using System;


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

        /// <summary>オプション画面です。</summary>
        OptionMenu,

        /// <summary>クレジット画面です。</summary>
        Credit
    }
}