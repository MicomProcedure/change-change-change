using Cysharp.Threading.Tasks;

namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// ローカルの永続化情報を全て消去し、アプリケーションを起動直後と同じ状態から再構築するポートです。
    /// </summary>
    public interface IGameResetter
    {
        /// <summary>
        /// 表示中のSceneとアプリケーション共通の状態を全て破棄し、ローカルの永続化情報を消去してから、
        /// TitleSceneを読み込み直します。
        /// リセットを開始できない場合は、何も破棄せずに例外を送出します。
        /// 表示中のSceneの破棄を始めた後に失敗した場合は、画面を破棄した状態のまま例外を送出するか、失敗の結果を返します。
        /// </summary>
        /// <param name="titleStartRequest">読み込み直したTitleSceneの開始Request。</param>
        /// <returns>TitleSceneの読み込み結果。</returns>
        UniTask<SceneNavigationResult> ResetAsync(TitleStartRequest titleStartRequest);
    }
}
