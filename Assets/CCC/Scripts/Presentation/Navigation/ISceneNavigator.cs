using Cysharp.Threading.Tasks;

namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// Scene遷移を抽象化するインターフェースです。
    /// </summary>
    public interface ISceneNavigator
    {
        /// <summary>
        /// Sceneロード中かどうかを取得します。
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// GameSceneをロードします。
        /// </summary>
        /// <param name="request">Gameplay開始Requestです。</param>
        UniTask<SceneNavigationResult> LoadGameplayAsync(
            GameplayStartRequest request);

        /// <summary>
        /// TitleSceneをロードします。
        /// </summary>
        /// <param name="request">TitleScene開始Requestです。</param>
        UniTask<SceneNavigationResult> LoadTitleAsync(
            TitleStartRequest request);
    }
}