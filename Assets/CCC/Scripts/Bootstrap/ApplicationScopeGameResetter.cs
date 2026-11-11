using System;
using System.Collections.Generic;
using Ccc.Core.UseCases;
using Ccc.Presentation.Navigation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// ApplicationLifetimeScopeを破棄して作り直すことで、アプリケーションを起動直後と同じ状態に戻すゲームリセットです。
    /// 表示中のScene、ApplicationLifetimeScopeの順に破棄し、全て破棄してからローカルの永続化情報を消去します。
    /// このクラス自身もApplicationLifetimeScopeに登録されており、破棄後もリセット処理を続けます。
    /// そのため破棄後に使う依存は、破棄で使えなくならないもの（IDisposableでないもの）に限ります。
    /// 作り直した後のSceneの読み込みには、作り直したScopeから取り出したISceneNavigatorを使います。
    /// この取り出しは、Scopeを作り直す本クラスに限って許可するService Locatorの例外です。
    /// </summary>
    public sealed class ApplicationScopeGameResetter : IGameResetter
    {
        private const string TransitionSceneName = "GameResetTransition";

        private readonly LifetimeScope _applicationScope;
        private readonly ILocalDataEraser _localDataEraser;
        private bool _isResetting;

        /// <summary>
        /// ApplicationScopeGameResetterを生成します。
        /// </summary>
        /// <param name="applicationScope">破棄して作り直すApplicationLifetimeScope。</param>
        /// <param name="localDataEraser">ローカルの永続化情報の消去処理。</param>
        /// <exception cref="ArgumentNullException">引数がnullの場合。</exception>
        public ApplicationScopeGameResetter(
            LifetimeScope applicationScope,
            ILocalDataEraser localDataEraser)
        {
            _applicationScope = applicationScope != null
                ? applicationScope
                : throw new ArgumentNullException(nameof(applicationScope));
            _localDataEraser =
                localDataEraser ?? throw new ArgumentNullException(nameof(localDataEraser));
        }

        /// <summary>
        /// 表示中のSceneとApplicationLifetimeScopeを破棄し、ローカルの永続化情報を消去してから、
        /// 作り直したApplicationLifetimeScopeでTitleSceneを読み込みます。
        /// </summary>
        /// <param name="titleStartRequest">読み込み直したTitleSceneの開始Request。</param>
        /// <returns>作り直したApplicationLifetimeScopeによるTitleSceneの読み込み結果。</returns>
        /// <exception cref="ArgumentNullException">titleStartRequestがnullの場合。</exception>
        /// <exception cref="InvalidOperationException">
        /// 既にリセット中の場合、またはApplicationLifetimeScopeを作り直せない構成の場合は、何も破棄せずに送出します。
        /// 表示中のSceneのアンロードを始めた後に、アンロードを開始できない場合やScopeの破棄が完了していない場合は、
        /// 画面を破棄した状態のまま送出します。
        /// </exception>
        public async UniTask<SceneNavigationResult> ResetAsync(TitleStartRequest titleStartRequest)
        {
            if (titleStartRequest == null)
            {
                throw new ArgumentNullException(nameof(titleStartRequest));
            }

            if (_isResetting)
            {
                throw new InvalidOperationException(
                    $"{nameof(ApplicationScopeGameResetter)}は既にリセット中です。");
            }

            VContainerSettings settings = ResolveRebuildableSettings();
            _isResetting = true;

            // ここから先は表示中の画面を破棄するため、失敗しても元の画面には戻れない
            await UnloadDisplayedScenesAsync();

            _applicationScope.Dispose();

            // LifetimeScopeのGameObjectはフレームの終わりに破棄されるため、破棄の完了を待ってから作り直す
            await UniTask.NextFrame();
            if (_applicationScope != null)
            {
                throw new InvalidOperationException(
                    "ApplicationLifetimeScopeの破棄が完了していないため、作り直せません。");
            }

            _localDataEraser.EraseAll();

            // Title用ScopeへのRequestの登録がApplicationLifetimeScopeに適用されないよう、読み込みより先に作り直す
            LifetimeScope rebuiltApplicationScope = settings.GetOrCreateRootLifetimeScopeInstance();
            ISceneNavigator sceneNavigator =
                rebuiltApplicationScope.Container.Resolve<ISceneNavigator>();
            return await sceneNavigator.LoadTitleAsync(titleStartRequest);
        }

        private VContainerSettings ResolveRebuildableSettings()
        {
            VContainerSettings settings = VContainerSettings.Instance;
            if (settings == null || settings.RootLifetimeScope == null)
            {
                throw new InvalidOperationException(
                    "VContainerSettingsにRootLifetimeScopeが設定されていないため、ゲームリセットを実行できません。");
            }

            if (!settings.IsRootLifetimeScopeInstance(_applicationScope))
            {
                throw new InvalidOperationException(
                    "ApplicationLifetimeScopeがVContainerSettingsのRootLifetimeScopeではないため、ゲームリセットを実行できません。");
            }

            return settings;
        }

        private static async UniTask UnloadDisplayedScenesAsync()
        {
            // 読み込み済みのSceneが残っていないと全てのSceneをアンロードできないため、空のSceneを経由する
            Scene transitionScene = SceneManager.CreateScene(TransitionSceneName);
            SceneManager.SetActiveScene(transitionScene);

            var displayedScenes = new List<Scene>();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene != transitionScene && scene.isLoaded)
                {
                    displayedScenes.Add(scene);
                }
            }

            for (int index = 0; index < displayedScenes.Count; index++)
            {
                AsyncOperation operation = SceneManager.UnloadSceneAsync(displayedScenes[index]);
                if (operation == null)
                {
                    throw new InvalidOperationException(
                        $"Scene「{displayedScenes[index].name}」のアンロードを開始できませんでした。");
                }

                await operation;
            }
        }
    }
}
