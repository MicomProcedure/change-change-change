using Cysharp.Threading.Tasks;
using Ccc.Core.Model;

namespace Ccc.Presentation
{
    /// <summary>
    /// Gameplay画面から外部画面へ移動するためのポートです。
    /// </summary>
    public interface IGameplayNavigation
    {
        /// <summary>
        /// SessionResultを結果画面へ渡します。
        /// </summary>
        /// <param name="result">表示対象のセッション結果。</param>
        void ShowResult(SessionResult result);

        /// <summary>
        /// MainMenuへ非同期で移動します。
        /// </summary>
        /// <returns>画面遷移完了を表す非同期処理。</returns>
        UniTask NavigateToMainMenuAsync();
    }
}
