using System;
using Ccc.Core.Model;
using Ccc.Presentation.Navigation;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// TitleScene内の画面状態とGameplay開始を管理します。
    /// </summary>
    public sealed class TitleScreenModel : IDisposable
    {
        private const string AlreadyLoadingErrorMessage =
            "画面を読み込み中です。";

        private const string GameplayLoadFailedErrorMessage =
            "ゲームを開始できませんでした。";

        private readonly ISceneNavigator _sceneNavigator;
        private readonly OptionMenuModel _optionMenuModel;
        private readonly ReactiveProperty<TitleScreenState> _currentScreen;
        private readonly ReactiveProperty<bool> _isBusy;
        private readonly ReactiveProperty<string> _errorMessage;

        private bool _isDisposed;

        /// <summary>
        /// 現在のTitleScene画面を取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<TitleScreenState> CurrentScreen { get; }

        /// <summary>
        /// Gameplay開始処理中かどうかを取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsBusy { get; }

        /// <summary>
        /// 画面に表示するエラーメッセージを取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<string> ErrorMessage { get; }

        /// <summary>
        /// TitleScreenModelを生成します。
        /// </summary>
        /// <param name="sceneNavigator">Scene遷移処理。</param>
        /// <param name="optionMenuModel">Option設定を保持するModel。</param>
        /// <param name="startRequest">TitleScene開始Request。</param>
        public TitleScreenModel(
            ISceneNavigator sceneNavigator,
            OptionMenuModel optionMenuModel,
            TitleStartRequest startRequest)
        {
            _sceneNavigator =
                sceneNavigator ?? throw new ArgumentNullException(nameof(sceneNavigator));

            _optionMenuModel =
                optionMenuModel ?? throw new ArgumentNullException(nameof(optionMenuModel));

            if (startRequest == null)
            {
                throw new ArgumentNullException(nameof(startRequest));
            }

            _currentScreen =
                new ReactiveProperty<TitleScreenState>(startRequest.InitialScreen);

            _isBusy = new ReactiveProperty<bool>(false);
            _errorMessage = new ReactiveProperty<string>(string.Empty);

            CurrentScreen = _currentScreen.ToReadOnlyReactiveProperty();
            IsBusy = _isBusy.ToReadOnlyReactiveProperty();
            ErrorMessage = _errorMessage.ToReadOnlyReactiveProperty();
        }

        /// <summary>
        /// TitleScreenからMainMenuへ遷移します。
        /// </summary>
        public void EnterMainMenu()
        {
            ChangeScreen(
                TitleScreenState.TitleScreen,
                TitleScreenState.MainMenu);
        }

        /// <summary>
        /// MainMenuからOptionMenuへ遷移します。
        /// </summary>
        public void OpenOptionMenu()
        {
            ChangeScreen(
                TitleScreenState.MainMenu,
                TitleScreenState.OptionMenu);
        }

        /// <summary>
        /// MainMenuからStageへ遷移します。
        /// </summary>
        public void OpenStage()
        {
            ChangeScreen(
                TitleScreenState.MainMenu,
                TitleScreenState.Stage);
        }

        /// <summary>
        /// OptionMenuからMainMenuへ戻ります。
        /// </summary>
        public void CloseOptionMenu()
        {
            ChangeScreen(
                TitleScreenState.OptionMenu,
                TitleScreenState.MainMenu);
        }

        /// <summary>
        /// MainMenuからCreditへ遷移します。
        /// </summary>
        public void OpenCredit()
        {
            ChangeScreen(
                TitleScreenState.MainMenu,
                TitleScreenState.Credit);
        }

        /// <summary>
        /// CreditからMainMenuへ戻ります。
        /// </summary>
        public void CloseCredit()
        {
            ChangeScreen(
                TitleScreenState.Credit,
                TitleScreenState.MainMenu);
        }

        /// <summary>
        /// StageからGameSceneを開始します。
        /// </summary>
        public async UniTask StartGameplayAsync()
        {
            ThrowIfDisposed();

            if (_currentScreen.Value != TitleScreenState.Stage ||
                _isBusy.Value)
            {
                return;
            }

            _errorMessage.Value = string.Empty;
            _isBusy.Value = true;

            try
            {
                OptionSettingsSnapshot optionSettings =
                    _optionMenuModel.CreateSnapshot();

                GameplayNavigationRequest request =
                    new GameplayNavigationRequest(
                        GameMode.LifeEndless,
                        GameDifficulty.Normal,
                        optionSettings);

                SceneNavigationResult result =
                    await _sceneNavigator.LoadGameplayAsync(request);

                if (_isDisposed || result.IsSuccess)
                {
                    return;
                }

                _errorMessage.Value =
                    result.FailureKind ==
                    SceneNavigationFailureKind.AlreadyLoading
                        ? AlreadyLoadingErrorMessage
                        : GameplayLoadFailedErrorMessage;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                if (!_isDisposed)
                {
                    _errorMessage.Value = GameplayLoadFailedErrorMessage;
                }
            }
            finally
            {
                if (!_isDisposed)
                {
                    _isBusy.Value = false;
                }
            }
        }

        /// <summary>
        /// Modelを破棄します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            CurrentScreen.Dispose();
            IsBusy.Dispose();
            ErrorMessage.Dispose();

            _currentScreen.Dispose();
            _isBusy.Dispose();
            _errorMessage.Dispose();
        }

        private void ChangeScreen(
            TitleScreenState expectedCurrentScreen,
            TitleScreenState nextScreen)
        {
            if (_isDisposed ||
                _isBusy.Value ||
                _currentScreen.Value != expectedCurrentScreen)
            {
                return;
            }

            _errorMessage.Value = string.Empty;
            _currentScreen.Value = nextScreen;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(TitleScreenModel));
            }
        }
    }
}
