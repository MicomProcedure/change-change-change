using System;

namespace Ccc.Presentation.Navigation
{

    /// <summary>
    /// TitleScene開始時の初期画面を保持するRequestです。
    /// </summary>
    public sealed class TitleStartRequest
    {
        /// <summary>
        /// TitleScene開始時の画面状態を取得します。
        /// </summary>
        public TitleScreenState InitialScreen { get; }

        /// <summary>
        /// TitleStartRequestを生成します。
        /// </summary>
        /// <param name="initialScreen">開始時の画面状態です。</param>
        public TitleStartRequest(TitleScreenState initialScreen)
        {
            // 開始時のScreenStateがMainMenuかTitleScreen以外は許容しない
            if (initialScreen is not
                (TitleScreenState.TitleScreen or TitleScreenState.MainMenu))
            {
                throw new ArgumentOutOfRangeException(nameof(initialScreen));
            }

            InitialScreen = initialScreen;
        }
    }
}   