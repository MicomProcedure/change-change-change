using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
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

        private bool _isLoading;

        /// <summary>
        /// Sceneをロード中かどうかを取得します。
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
        }

        /// <summary>
        /// Gameplay用のGameSceneをロードします。
        /// </summary>
        /// <param name="request">Gameplay開始Requestです。</param>
        /// <returns>Scene遷移結果です。</returns>
        public UniTask<SceneNavigationResult> LoadGameplayAsync(
            GameplayNavigationRequest request)
        {
            return LoadSceneAsync(SceneNames.Game, request);
        }

        /// <summary>
        /// Title用のTitleSceneをロードします。
        /// </summary>
        /// <param name="request">TitleScene開始Requestです。</param>
        /// <returns>Scene遷移結果です。</returns>
        public UniTask<SceneNavigationResult> LoadTitleAsync(
            TitleStartRequest request)
        {
            return LoadSceneAsync(SceneNames.Title, request);
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

    /// <summary>
    /// Scene名を一元管理します。
    /// </summary>
    internal static class SceneNames
    {
        internal const string Title = "TitleScene";
        internal const string Game = "GameScene";
    }
}