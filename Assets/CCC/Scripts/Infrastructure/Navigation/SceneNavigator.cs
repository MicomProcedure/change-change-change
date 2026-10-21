using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Ccc.Presentation;
using Ccc.Presentation.Navigation;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// Unity Scene間の遷移とScene間Requestの受け渡しを担当します。
    /// </summary>
    public sealed class SceneNavigator : ISceneNavigator
    {
        private const string AlreadyLoadingErrorMessage =
            "Sceneを読み込み中です。";

        private const string LoadFailedErrorMessage =
            "Sceneの読み込みに失敗しました。";

        private readonly SceneCatalogAsset _sceneCatalogAsset;
        private readonly IPlayPlatformMode _playPlatformMode;
        private bool _isLoading;

        /// <summary>
        /// SceneNavigatorを生成します。
        /// </summary>
        /// <param name="sceneCatalogAsset">画面の種類とプラットフォームからScene名を解決するCatalog。</param>
        /// <param name="playPlatformMode">プレイ環境のプラットフォーム。</param>
        /// <exception cref="ArgumentNullException">引数がnullの場合。</exception>
        public SceneNavigator(
            SceneCatalogAsset sceneCatalogAsset,
            IPlayPlatformMode playPlatformMode)
        {
            _sceneCatalogAsset = sceneCatalogAsset != null
                ? sceneCatalogAsset
                : throw new ArgumentNullException(nameof(sceneCatalogAsset));
            _playPlatformMode = playPlatformMode ?? throw new ArgumentNullException(nameof(playPlatformMode));
        }

        /// <summary>
        /// Sceneをロード中かどうかを取得します。
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
        }

        /// <summary>
        /// プラットフォームに対応するGameplay用Sceneをロードします。
        /// </summary>
        /// <param name="request">Gameplay開始Requestです。</param>
        /// <returns>Scene遷移結果です。</returns>
        public UniTask<SceneNavigationResult> LoadGameplayAsync(
            GameplayNavigationRequest request)
        {
            return LoadSceneAsync(ResolveSceneName(SceneKind.Gameplay), request);
        }

        /// <summary>
        /// プラットフォームに対応するTitle用Sceneをロードします。
        /// </summary>
        /// <param name="request">TitleScene開始Requestです。</param>
        /// <returns>Scene遷移結果です。</returns>
        public UniTask<SceneNavigationResult> LoadTitleAsync(
            TitleStartRequest request)
        {
            return LoadSceneAsync(ResolveSceneName(SceneKind.Title), request);
        }

        private string ResolveSceneName(SceneKind sceneKind)
        {
            return _sceneCatalogAsset.GetSceneName(sceneKind, _playPlatformMode.Platform);
        }

        private async UniTask<SceneNavigationResult> LoadSceneAsync<TRequest>(
            string sceneName,
            TRequest request)
            where TRequest : class
        {
            Debug.Log($"[SceneNavigator] LoadSceneAsync: {sceneName} with request: {request}");

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (_isLoading)
            {
                return CreateAlreadyLoadingResult();
            }

            _isLoading = true;

            try
            {
                using (LifetimeScope.Enqueue(
                    builder => builder.RegisterInstance(request)))
                {
                    AsyncOperation operation =
                        SceneManager.LoadSceneAsync(sceneName);

                    if (operation == null)
                    {
                        throw new InvalidOperationException(
                            "Sceneロードを開始できませんでした。");
                    }

                    await operation;
                }

                return CreateSuccessResult();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return CreateLoadFailedResult(exception);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private static SceneNavigationResult CreateSuccessResult()
        {
            return new SceneNavigationResult(
                true,
                SceneNavigationFailureKind.None,
                string.Empty,
                null);
        }

        private static SceneNavigationResult CreateAlreadyLoadingResult()
        {
            return new SceneNavigationResult(
                false,
                SceneNavigationFailureKind.AlreadyLoading,
                AlreadyLoadingErrorMessage,
                null);
        }

        private static SceneNavigationResult CreateLoadFailedResult(
            Exception cause)
        {
            if (cause == null)
            {
                throw new ArgumentNullException(nameof(cause));
            }

            return new SceneNavigationResult(
                false,
                SceneNavigationFailureKind.LoadFailed,
                LoadFailedErrorMessage,
                cause);
        }
    }
}