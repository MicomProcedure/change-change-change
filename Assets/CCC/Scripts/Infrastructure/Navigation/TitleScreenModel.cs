using System;
using System.Globalization;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Ccc.Presentation.Navigation;
using Ccc.Presentation.ScoreRank;
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

        private const string GameResetLoadFailedErrorMessage =
            "リセット後の画面を読み込めませんでした。";

        private const GameMode SelectedGameMode = GameMode.LifeEndless;

        private const GameDifficulty InitialDifficulty = GameDifficulty.Easy;

        private readonly ISceneNavigator _sceneNavigator;
        private readonly OptionMenuModel _optionMenuModel;
        private readonly ILocalDataEraser _localDataEraser;
        private readonly ReactiveProperty<TitleScreenState> _currentScreen;
        private readonly ReactiveProperty<bool> _isBusy;
        private readonly ReactiveProperty<string> _errorMessage;
        private readonly ReactiveProperty<GameDifficulty> _selectedDifficulty;
        private readonly ReactiveProperty<bool> _isResetConfirmationOpen;
        private readonly Subject<TitleScreenTransitionDirection> _screenTransitioned;

        private bool _isDisposed;

        /// <summary>
        /// 現在のTitleScene画面を取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<TitleScreenState> CurrentScreen { get; }

        /// <summary>
        /// 受け付けた画面遷移の方向を、CurrentScreenの更新後に通知します。
        /// 受け付けなかった遷移要求は通知しません。
        /// </summary>
        public Observable<TitleScreenTransitionDirection> ScreenTransitioned { get; }

        /// <summary>
        /// Gameplay開始処理中かどうかを取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsBusy { get; }

        /// <summary>
        /// 画面に表示するエラーメッセージを取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<string> ErrorMessage { get; }

        /// <summary>
        /// Stageで選択中のゲーム難易度を取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<GameDifficulty> SelectedDifficulty { get; }

        /// <summary>
        /// 選択中のゲームモードと難易度のハイスコアを表示用の文字列で取得します。記録がない場合は0です。
        /// </summary>
        public ReadOnlyReactiveProperty<string> SelectedHighScoreText { get; }

        /// <summary>
        /// 選択中のゲームモードと難易度のハイスコアで到達した評価段階の数を取得します。記録がない場合は0です。
        /// </summary>
        public ReadOnlyReactiveProperty<int> SelectedHighScoreAchievedRankCount { get; }

        /// <summary>
        /// OptionMenu上のゲームリセット確認ポップアップが開いているかどうかを取得します。
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsResetConfirmationOpen { get; }

        /// <summary>
        /// TitleScreenModelを生成します。
        /// </summary>
        /// <param name="sceneNavigator">Scene遷移処理。</param>
        /// <param name="optionMenuModel">Option設定を保持するModel。</param>
        /// <param name="startRequest">TitleScene開始Request。</param>
        /// <param name="highScoreService">ハイスコアの参照処理。</param>
        /// <param name="scoreRankThresholdsProvider">ハイスコアの評価段階の基準を提供するポート。</param>
        /// <param name="localDataEraser">ローカルの永続化情報の消去処理。</param>
        public TitleScreenModel(
            ISceneNavigator sceneNavigator,
            OptionMenuModel optionMenuModel,
            TitleStartRequest startRequest,
            HighScoreService highScoreService,
            IScoreRankThresholdsProvider scoreRankThresholdsProvider,
            ILocalDataEraser localDataEraser)
        {
            _sceneNavigator =
                sceneNavigator ?? throw new ArgumentNullException(nameof(sceneNavigator));

            _optionMenuModel =
                optionMenuModel ?? throw new ArgumentNullException(nameof(optionMenuModel));

            _localDataEraser =
                localDataEraser ?? throw new ArgumentNullException(nameof(localDataEraser));

            if (startRequest == null)
            {
                throw new ArgumentNullException(nameof(startRequest));
            }

            if (highScoreService == null)
            {
                throw new ArgumentNullException(nameof(highScoreService));
            }

            if (scoreRankThresholdsProvider == null)
            {
                throw new ArgumentNullException(nameof(scoreRankThresholdsProvider));
            }

            _currentScreen =
                new ReactiveProperty<TitleScreenState>(startRequest.InitialScreen);

            _isBusy = new ReactiveProperty<bool>(false);
            _errorMessage = new ReactiveProperty<string>(string.Empty);
            _selectedDifficulty = new ReactiveProperty<GameDifficulty>(InitialDifficulty);
            _isResetConfirmationOpen = new ReactiveProperty<bool>(false);
            _screenTransitioned = new Subject<TitleScreenTransitionDirection>();

            CurrentScreen = _currentScreen.ToReadOnlyReactiveProperty();
            ScreenTransitioned = _screenTransitioned;
            IsBusy = _isBusy.ToReadOnlyReactiveProperty();
            ErrorMessage = _errorMessage.ToReadOnlyReactiveProperty();
            SelectedDifficulty = _selectedDifficulty.ToReadOnlyReactiveProperty();
            SelectedHighScoreText = _selectedDifficulty
                .Select(difficulty => highScoreService
                    .GetHighScore(SelectedGameMode, difficulty)
                    .ToString(CultureInfo.InvariantCulture))
                .ToReadOnlyReactiveProperty();
            SelectedHighScoreAchievedRankCount = _selectedDifficulty
                .Select(difficulty => scoreRankThresholdsProvider
                    .Get(SelectedGameMode, difficulty)
                    .CountAchievedRanks(highScoreService.GetHighScore(SelectedGameMode, difficulty)))
                .ToReadOnlyReactiveProperty();
            IsResetConfirmationOpen = _isResetConfirmationOpen.ToReadOnlyReactiveProperty();
        }

        /// <summary>
        /// TitleScreenからMainMenuへ遷移します。
        /// </summary>
        public void EnterMainMenu()
        {
            ChangeScreen(
                TitleScreenState.TitleScreen,
                TitleScreenState.MainMenu,
                TitleScreenTransitionDirection.Forward);
        }

        /// <summary>
        /// MainMenuからOptionMenuへ遷移します。
        /// </summary>
        public void OpenOptionMenu()
        {
            ChangeScreen(
                TitleScreenState.MainMenu,
                TitleScreenState.OptionMenu,
                TitleScreenTransitionDirection.Forward);
        }

        /// <summary>
        /// MainMenuからStageへ遷移します。
        /// </summary>
        public void OpenStage()
        {
            ChangeScreen(
                TitleScreenState.MainMenu,
                TitleScreenState.Stage,
                TitleScreenTransitionDirection.Forward);
        }

        /// <summary>
        /// OptionMenuからMainMenuへ戻ります。
        /// </summary>
        public void CloseOptionMenu()
        {
            ChangeScreen(
                TitleScreenState.OptionMenu,
                TitleScreenState.MainMenu,
                TitleScreenTransitionDirection.Backward);
        }

        /// <summary>
        /// MainMenuからCreditへ遷移します。
        /// </summary>
        public void OpenCredit()
        {
            ChangeScreen(
                TitleScreenState.MainMenu,
                TitleScreenState.Credit,
                TitleScreenTransitionDirection.Forward);
        }

        /// <summary>
        /// CreditからMainMenuへ戻ります。
        /// </summary>
        public void CloseCredit()
        {
            ChangeScreen(
                TitleScreenState.Credit,
                TitleScreenState.MainMenu,
                TitleScreenTransitionDirection.Backward);
        }

        /// <summary>
        /// StageからTutorialへ遷移します。
        /// </summary>
        public void OpenTutorial()
        {
            ChangeScreen(
                TitleScreenState.Stage,
                TitleScreenState.Tutorial,
                TitleScreenTransitionDirection.Forward);
        }

        /// <summary>
        /// TutorialからStageへ戻ります。
        /// </summary>
        public void CloseTutorial()
        {
            ChangeScreen(
                TitleScreenState.Tutorial,
                TitleScreenState.Stage,
                TitleScreenTransitionDirection.Backward);
        }

        /// <summary>
        /// StageからMainMenuへ戻ります。
        /// </summary>
        public void CloseStage()
        {
            ChangeScreen(
                TitleScreenState.Stage,
                TitleScreenState.MainMenu,
                TitleScreenTransitionDirection.Backward);
        }

        /// <summary>
        /// 選択中の難易度を宣言順（Easy → Normal → Hard → Easy）に切り替えます。
        /// </summary>
        public void CycleDifficulty()
        {
            if (_isDisposed ||
                _isBusy.Value ||
                _currentScreen.Value != TitleScreenState.Stage)
            {
                return;
            }

            GameDifficulty[] difficulties =
                (GameDifficulty[])Enum.GetValues(typeof(GameDifficulty));
            int nextIndex =
                (Array.IndexOf(difficulties, _selectedDifficulty.Value) + 1) % difficulties.Length;
            _selectedDifficulty.Value = difficulties[nextIndex];
        }

        /// <summary>
        /// OptionMenu上にゲームリセットの確認ポップアップを開きます。
        /// </summary>
        public void OpenResetConfirmation()
        {
            if (_isDisposed ||
                _isBusy.Value ||
                _currentScreen.Value != TitleScreenState.OptionMenu)
            {
                return;
            }

            _errorMessage.Value = string.Empty;
            _isResetConfirmationOpen.Value = true;
        }

        /// <summary>
        /// ゲームリセットの確認ポップアップを閉じます。
        /// </summary>
        public void CloseResetConfirmation()
        {
            if (_isDisposed || _isBusy.Value)
            {
                return;
            }

            _errorMessage.Value = string.Empty;
            _isResetConfirmationOpen.Value = false;
        }

        /// <summary>
        /// ローカルの永続化情報を全て消去し、TitleSceneをタイトル画面から読み込み直します。
        /// 全ての画面状態と音量を消去後の保存内容から再構築します。
        /// </summary>
        public async UniTask ConfirmGameResetAsync()
        {
            ThrowIfDisposed();

            if (!_isResetConfirmationOpen.Value ||
                _isBusy.Value)
            {
                return;
            }

            await LoadSceneWhileBusyAsync(
                () =>
                {
                    _localDataEraser.EraseAll();
                    return _sceneNavigator.LoadTitleAsync(
                        new TitleStartRequest(TitleScreenState.TitleScreen));
                },
                GameResetLoadFailedErrorMessage);
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

            await LoadSceneWhileBusyAsync(
                () => _sceneNavigator.LoadGameplayAsync(
                    new GameplayNavigationRequest(
                        SelectedGameMode,
                        _selectedDifficulty.Value,
                        _optionMenuModel.Settings.CurrentValue)),
                GameplayLoadFailedErrorMessage);
        }

        private async UniTask LoadSceneWhileBusyAsync(
            Func<UniTask<SceneNavigationResult>> loadSceneAsync,
            string loadFailedErrorMessage)
        {
            _errorMessage.Value = string.Empty;
            _isBusy.Value = true;

            try
            {
                SceneNavigationResult result = await loadSceneAsync();

                if (_isDisposed || result.IsSuccess)
                {
                    return;
                }

                _errorMessage.Value =
                    result.FailureKind ==
                    SceneNavigationFailureKind.AlreadyLoading
                        ? AlreadyLoadingErrorMessage
                        : loadFailedErrorMessage;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                if (!_isDisposed)
                {
                    _errorMessage.Value = loadFailedErrorMessage;
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
            SelectedDifficulty.Dispose();
            SelectedHighScoreText.Dispose();
            SelectedHighScoreAchievedRankCount.Dispose();
            IsResetConfirmationOpen.Dispose();

            _currentScreen.Dispose();
            _isBusy.Dispose();
            _errorMessage.Dispose();
            _selectedDifficulty.Dispose();
            _isResetConfirmationOpen.Dispose();
            _screenTransitioned.Dispose();
        }

        private void ChangeScreen(
            TitleScreenState expectedCurrentScreen,
            TitleScreenState nextScreen,
            TitleScreenTransitionDirection direction)
        {
            if (_isDisposed ||
                _isBusy.Value ||
                _isResetConfirmationOpen.Value ||
                _currentScreen.Value != expectedCurrentScreen)
            {
                return;
            }

            _errorMessage.Value = string.Empty;
            _currentScreen.Value = nextScreen;
            _screenTransitioned.OnNext(direction);
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
