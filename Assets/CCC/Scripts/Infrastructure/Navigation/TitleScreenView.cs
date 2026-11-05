using System;
using System.Globalization;
using Ccc.Core.UseCases;
using Ccc.Presentation.Navigation;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;
using Ccc.Core.Model;
using Ccc.Presentation;
using Ccc.Infrastructure.UI;
using Ccc.Presentation.UI;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// TitleScene内の画面表示とボタン入力を担当します。
    /// </summary>
    public sealed class TitleScreenView : MonoBehaviour
    {
        [Header("画面Root")]
        [SerializeField] private GameObject _titleScreenRoot;
        [SerializeField] private GameObject _mainMenuRoot;
        [SerializeField] private GameObject _stageRoot;
        [SerializeField] private GameObject _optionMenuRoot;
        [SerializeField] private GameObject _creditRoot;
        [SerializeField] private GameObject _tutorialRoot;
        [SerializeField] private GameObject _resetConfirmationRoot;


        [Header("ボタン")]
        [SerializeField] private Button _enterMainMenuButton;
        [SerializeField] private Button _openStageButton;
        [SerializeField] private Button _closeStageButton;  

        [FormerlySerializedAs("_difficultySelectionButton")]
        [SerializeField] private Button _startGameplayButton;

        [SerializeField] private Button _openOptionButton;
        [SerializeField] private Button _closeOptionButton;
        [SerializeField] private Button _openCreditButton;
        [SerializeField] private Button _closeCreditButton;
        [SerializeField] private Button _openTutorialButton;
        [SerializeField] private Button _closeTutorialButton;
        [SerializeField] private Button _openResetConfirmationButton;
        [SerializeField] private Button _closeResetConfirmationButton;
        [SerializeField] private Button _confirmGameResetButton;

        [Header("難易度選択")]
        [SerializeField] private CycleSelectorView _difficultySelectorView;
        [SerializeField] private DifficultyDescriptionView _difficultyDescriptionView;
        [SerializeField] private TutorialPageView _tutorialPageView;
        [SerializeField] private TMP_Text _highScoreText;

        [Header("音量設定")]
        [SerializeField] private CycleSelectorView _seVolumeSelectorView;
        [SerializeField] private CycleSelectorView _bgmVolumeSelectorView;

        [Header("バージョン表示")]
        [Tooltip("アプリケーションバージョンを表示するText。タイトル画面とクレジットに配置する。")]
        [SerializeField] private TMP_Text[] _versionTexts;
        [Tooltip("バージョン表示の書式。{0}にアプリケーションバージョンが入る。")]
        [SerializeField] private string _versionTextFormat = "v{0}";

        [Header("エラー表示")]
        [SerializeField] private TMP_Text _errorText;

        // TitleScreenModel.CycleDifficulty() は enum の定義順で循環するため、この並びがそのまま丸の並びになる
        private static readonly GameDifficulty[] DifficultyChoices =
            (GameDifficulty[])Enum.GetValues(typeof(GameDifficulty));

        private TitleScreenModel _model;
        private IRuntimeMode _runtimeMode;
        private IApplicationVersionProvider _applicationVersionProvider;
        private IDisposable _screenSubscription;
        private IDisposable _busySubscription;
        private IDisposable _errorSubscription;
        private IDisposable _difficultyChangeSubscription;
        private IDisposable _difficultySubscription;
        private IDisposable _highScoreSubscription;
        private OptionMenuModel _optionMenuModel;
        private IDisposable _seVolumeChangeSubscription;
        private IDisposable _bgmVolumeChangeSubscription;
        private IDisposable _optionMenuSubscription;
        private IDisposable _resetConfirmationSubscription;
        private bool _isInitialized;

        [Inject]
        private void Construct(
            TitleScreenModel model,
            OptionMenuModel optionMenuModel,
            IRuntimeMode runtimeMode,
            IApplicationVersionProvider applicationVersionProvider)
        {
            _model = model;
            _optionMenuModel = optionMenuModel;
            _runtimeMode = runtimeMode;
            _applicationVersionProvider = applicationVersionProvider;
        }

        private void Start()
        {
            if (!ValidateReferences())
            {
                return;
            }

            if (_enterMainMenuButton != null)
            {
                _enterMainMenuButton.onClick.AddListener(_model.EnterMainMenu);
            }

            if (_startGameplayButton != null)
            {
                _startGameplayButton.onClick.AddListener(HandleStartGameplay);
            }

            if (_openStageButton != null)
            {
                _openStageButton.onClick.AddListener(_model.OpenStage);
            }

            if (_openOptionButton != null)
            {
                _openOptionButton.onClick.AddListener(_model.OpenOptionMenu);
            }

            if (_closeOptionButton != null)
            {
                _closeOptionButton.onClick.AddListener(_model.CloseOptionMenu);
            }

            if (_openCreditButton != null)
            {
                _openCreditButton.onClick.AddListener(_model.OpenCredit);
            }

            if (_closeCreditButton != null)
            {
                _closeCreditButton.onClick.AddListener(_model.CloseCredit);
            }

            if (_closeStageButton != null)
            {
                _closeStageButton.onClick.AddListener(_model.CloseStage);
            }

            if (_openTutorialButton != null)
            {
                _openTutorialButton.onClick.AddListener(_model.OpenTutorial);
            }

            if (_closeTutorialButton != null)
            {
                _closeTutorialButton.onClick.AddListener(_model.CloseTutorial);
            }

            if (_openResetConfirmationButton != null)
            {
                _openResetConfirmationButton.onClick.AddListener(_model.OpenResetConfirmation);
            }

            if (_closeResetConfirmationButton != null)
            {
                _closeResetConfirmationButton.onClick.AddListener(_model.CloseResetConfirmation);
            }

            if (_confirmGameResetButton != null)
            {
                _confirmGameResetButton.onClick.AddListener(HandleConfirmGameReset);
            }

            if (_difficultySelectorView != null)
            {
                _difficultyChangeSubscription =
                    _difficultySelectorView.ChangeRequested.Subscribe(_ => _model.CycleDifficulty());
            }

            if (_seVolumeSelectorView != null)
            {
                _seVolumeChangeSubscription =
                    _seVolumeSelectorView.ChangeRequested.Subscribe(_ => _optionMenuModel.CycleSEVolumeLevel());
            }

            if (_bgmVolumeSelectorView != null)
            {
                _bgmVolumeChangeSubscription =
                    _bgmVolumeSelectorView.ChangeRequested.Subscribe(_ => _optionMenuModel.CycleBgmVolumeLevel());
            }

            _screenSubscription =
                _model.CurrentScreen.Subscribe(RenderScreen);

            _busySubscription =
                _model.IsBusy.Subscribe(RenderBusyState);

            _errorSubscription =
                _model.ErrorMessage.Subscribe(RenderErrorMessage);

            _difficultySubscription =
                _model.SelectedDifficulty.Subscribe(RenderDifficulty);

            _highScoreSubscription =
                _model.SelectedHighScoreText.Subscribe(RenderHighScore);


            _optionMenuSubscription =
                _optionMenuModel.Settings
                    .CombineLatest(
                        _model.IsBusy,
                        _model.IsResetConfirmationOpen,
                        (settings, isBusy, isResetConfirmationOpen) =>
                            OptionMenuViewStateDto.Create(settings, !isBusy && !isResetConfirmationOpen))
                    .Subscribe(RenderOptionMenu);

            _resetConfirmationSubscription =
                _model.IsResetConfirmationOpen.Subscribe(RenderResetConfirmation);

            RenderScreen(_model.CurrentScreen.CurrentValue);
            RenderBusyState(_model.IsBusy.CurrentValue);
            RenderErrorMessage(_model.ErrorMessage.CurrentValue);
            RenderApplicationVersion();

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            _screenSubscription?.Dispose();
            _busySubscription?.Dispose();
            _errorSubscription?.Dispose();
            _difficultySubscription?.Dispose();
            _difficultyChangeSubscription?.Dispose();
            _highScoreSubscription?.Dispose();
            _seVolumeChangeSubscription?.Dispose();
            _bgmVolumeChangeSubscription?.Dispose();
            _optionMenuSubscription?.Dispose();
            _resetConfirmationSubscription?.Dispose();

            if (!_isInitialized || _model == null)
            {
                return;
            }

            if (_enterMainMenuButton != null)
            {
                _enterMainMenuButton.onClick.RemoveListener(
                    _model.EnterMainMenu);
            }

            if (_startGameplayButton != null)
            {
                _startGameplayButton.onClick.RemoveListener(
                    HandleStartGameplay);
            }

            if (_openStageButton != null)
            {
                _openStageButton.onClick.RemoveListener(_model.OpenStage);
            }

            if (_openOptionButton != null)
            {
                _openOptionButton.onClick.RemoveListener(
                    _model.OpenOptionMenu);
            }

            if (_closeOptionButton != null)
            {
                _closeOptionButton.onClick.RemoveListener(
                    _model.CloseOptionMenu);
            }

            if (_openCreditButton != null)
            {
                _openCreditButton.onClick.RemoveListener(
                    _model.OpenCredit);
            }

            if (_closeStageButton != null)
            {
                _closeStageButton.onClick.RemoveListener(
                    _model.CloseStage);
            }

            if (_openTutorialButton != null)
            {
                _openTutorialButton.onClick.RemoveListener(
                    _model.OpenTutorial);
            }

            if (_closeTutorialButton != null)
            {
                _closeTutorialButton.onClick.RemoveListener(
                    _model.CloseTutorial);
            }

            if (_openResetConfirmationButton != null)
            {
                _openResetConfirmationButton.onClick.RemoveListener(
                    _model.OpenResetConfirmation);
            }

            if (_closeResetConfirmationButton != null)
            {
                _closeResetConfirmationButton.onClick.RemoveListener(
                    _model.CloseResetConfirmation);
            }

            if (_confirmGameResetButton != null)
            {
                _confirmGameResetButton.onClick.RemoveListener(
                    HandleConfirmGameReset);
            }

            if (_closeCreditButton != null)
            {
                _closeCreditButton.onClick.RemoveListener(
                    _model.CloseCredit);
            }
        }

        private void RenderScreen(TitleScreenState screenState)
        {
            RenderScreenRoot(
                _titleScreenRoot,
                screenState == TitleScreenState.TitleScreen);

            RenderScreenRoot(
                _mainMenuRoot,
                screenState == TitleScreenState.MainMenu);

            RenderScreenRoot(
                _stageRoot,
                screenState == TitleScreenState.Stage);

            RenderScreenRoot(
                _optionMenuRoot,
                screenState == TitleScreenState.OptionMenu);

            RenderScreenRoot(
                _creditRoot,
                screenState == TitleScreenState.Credit);

            RenderScreenRoot(
                _tutorialRoot,
                screenState == TitleScreenState.Tutorial);
        }

        private void RenderResetConfirmation(bool isOpen)
        {
            RenderScreenRoot(_resetConfirmationRoot, isOpen);
        }

        private void RenderBusyState(bool isBusy)
        {
            bool isInteractable = !isBusy;

            SetInteractableIfAvailable(_enterMainMenuButton, isInteractable);
            SetInteractableIfAvailable(_openStageButton, isInteractable);
            SetInteractableIfAvailable(_startGameplayButton, isInteractable);
            SetInteractableIfAvailable(_openOptionButton, isInteractable);
            SetInteractableIfAvailable(_closeOptionButton, isInteractable);
            SetInteractableIfAvailable(_openCreditButton, isInteractable);
            SetInteractableIfAvailable(_closeCreditButton, isInteractable);
            SetInteractableIfAvailable(_closeStageButton, isInteractable);
            SetInteractableIfAvailable(_openTutorialButton, isInteractable);
            SetInteractableIfAvailable(_closeTutorialButton, isInteractable);
            SetInteractableIfAvailable(_openResetConfirmationButton, isInteractable);
            SetInteractableIfAvailable(_closeResetConfirmationButton, isInteractable);
            SetInteractableIfAvailable(_confirmGameResetButton, isInteractable);
            RenderDifficulty(_model.SelectedDifficulty.CurrentValue);
        }

        private void RenderDifficulty(GameDifficulty difficulty)
        {
            if (_difficultyDescriptionView != null)
            {
                _difficultyDescriptionView.Render(difficulty);
            }

            if (_difficultySelectorView == null)
            {
                return;
            }

            _difficultySelectorView.Render(
                new CycleSelectorStateDto(
                    difficulty.ToString(),
                    !_model.IsBusy.CurrentValue,
                    Array.IndexOf(DifficultyChoices, difficulty),
                    DifficultyChoices.Length));
        }

        private void RenderHighScore(string highScoreText)
        {
            if (_highScoreText != null)
            {
                _highScoreText.text = highScoreText;
            }
        }

        private void RenderOptionMenu(OptionMenuViewStateDto viewState)
        {
            RenderSelector(_seVolumeSelectorView, viewState.SEVolumeSelector);
            RenderSelector(_bgmVolumeSelectorView, viewState.BgmVolumeSelector);
        }

        private static void RenderSelector(CycleSelectorView selectorView, CycleSelectorStateDto selectorState)
        {
            if (selectorView != null)
            {
                selectorView.Render(selectorState);
            }
        }

        private void RenderErrorMessage(string errorMessage)
        {
            if (_errorText == null)
            {
                return;
            }

            bool hasError = !string.IsNullOrEmpty(errorMessage);

            _errorText.gameObject.SetActive(hasError);
            _errorText.text = errorMessage;
        }

        private void RenderApplicationVersion()
        {
            if (_versionTexts == null)
            {
                return;
            }

            string versionText = string.Format(
                CultureInfo.InvariantCulture,
                _versionTextFormat,
                _applicationVersionProvider.CurrentVersion);

            for (int index = 0; index < _versionTexts.Length; index++)
            {
                if (_versionTexts[index] != null)
                {
                    _versionTexts[index].text = versionText;
                }
            }
        }

        private void HandleStartGameplay()
        {
            _model.StartGameplayAsync().Forget(HandleAsyncFailure);
        }

        private void HandleConfirmGameReset()
        {
            _model.ConfirmGameResetAsync().Forget(HandleAsyncFailure);
        }

        private void HandleAsyncFailure(Exception exception)
        {
            Debug.LogException(exception);
        }

        private bool ValidateReferences()
        {
            if (_model == null)
            {
                LogMissingReference(nameof(_model));
                return false;
            }

            if (_optionMenuModel == null)
            {
                LogMissingReference(nameof(_optionMenuModel));
                return false;
            }

            if (_applicationVersionProvider == null)
            {
                LogMissingReference(nameof(_applicationVersionProvider));
                return false;
            }

            if (_titleScreenRoot == null)
            {
                LogMissingReference(nameof(_titleScreenRoot));
            }

            if (_mainMenuRoot == null)
            {
                LogMissingReference(nameof(_mainMenuRoot));
            }

            if (_stageRoot == null)
            {
                LogMissingReference(nameof(_stageRoot));
            }

            if (_optionMenuRoot == null)
            {
                LogMissingReference(nameof(_optionMenuRoot));
            }

            if (_creditRoot == null)
            {
                LogMissingReference(nameof(_creditRoot));
            }

             if (_tutorialRoot == null)
            {
                LogMissingReference(nameof(_tutorialRoot));
            }

            if (_resetConfirmationRoot == null)
            {
                LogMissingReference(nameof(_resetConfirmationRoot));
            }


            if (_enterMainMenuButton == null)
            {
                LogMissingReference(nameof(_enterMainMenuButton));
            }

            if (_openStageButton == null)
            {
                LogMissingReference(nameof(_openStageButton));
            }

            if (_startGameplayButton == null)
            {
                LogMissingReference(nameof(_startGameplayButton));
            }

            if (_openOptionButton == null)
            {
                LogMissingReference(nameof(_openOptionButton));
            }

            if (_closeOptionButton == null)
            {
                LogMissingReference(nameof(_closeOptionButton));
            }

            if (_openCreditButton == null)
            {
                LogMissingReference(nameof(_openCreditButton));
            }

            if (_closeCreditButton == null)
            {
                LogMissingReference(nameof(_closeCreditButton));
            }

            if (_closeStageButton == null)
            {
                LogMissingReference(nameof(_closeStageButton));
            }

            if (_openTutorialButton == null)
            {
                LogMissingReference(nameof(_openTutorialButton));
            }

            if (_closeTutorialButton == null)
            {
                LogMissingReference(nameof(_closeTutorialButton));
            }

            if (_openResetConfirmationButton == null)
            {
                LogMissingReference(nameof(_openResetConfirmationButton));
            }

            if (_closeResetConfirmationButton == null)
            {
                LogMissingReference(nameof(_closeResetConfirmationButton));
            }

            if (_confirmGameResetButton == null)
            {
                LogMissingReference(nameof(_confirmGameResetButton));
            }


            if (_difficultySelectorView == null)
            {
                LogMissingReference(nameof(_difficultySelectorView));
            }

            if (_difficultyDescriptionView == null)
            {
                LogMissingReference(nameof(_difficultyDescriptionView));
            }
            else
            {
                for (int index = 0; index < DifficultyChoices.Length; index++)
                {
                    GameDifficulty difficulty = DifficultyChoices[index];
                    if (!_difficultyDescriptionView.HasDescription(difficulty))
                    {
                        LogMissingReference($"{nameof(_difficultyDescriptionView)}の{difficulty}の説明");
                    }
                }
            }

            if (_tutorialPageView == null)
            {
                LogMissingReference(nameof(_tutorialPageView));
            }

            if (_highScoreText == null)
            {
                LogMissingReference(nameof(_highScoreText));
            }

            if (_seVolumeSelectorView == null)
            {
                LogMissingReference(nameof(_seVolumeSelectorView));
            }

            if (_bgmVolumeSelectorView == null)
            {
                LogMissingReference(nameof(_bgmVolumeSelectorView));
            }

            if (_versionTexts == null || _versionTexts.Length == 0)
            {
                LogMissingReference(nameof(_versionTexts));
            }

            if (_errorText == null)
            {
                LogMissingReference(nameof(_errorText));
            }

            return true;
        }

        private void LogMissingReference(string fieldName)
        {
            string message =
                $"{nameof(TitleScreenView)}: " +
                $"{fieldName}が未設定です。" +
                (IsDebugMode
                    ? "DebugModeのため初期化を継続します。"
                    : "該当するUI表示または入力を無効にして処理を継続します。");
            if (IsDebugMode)
            {
                Debug.LogError(message, this);
            }
            else
            {
                Debug.LogWarning(message, this);
            }
        }

        private bool IsDebugMode
        {
            get { return _runtimeMode != null && _runtimeMode.IsDebugMode; }
        }

        private static void RenderScreenRoot(GameObject target, bool shouldShow)
        {
            if (target == null)
            {
                return;
            }

            if (target.GetComponent<PanelScaleAnimation>() is PanelScaleAnimation panelScaleAnimation)
            {
                if (shouldShow)
                {
                    panelScaleAnimation.Show();
                }
                else
                {
                    panelScaleAnimation.Hide();
                }
                return;
            }

            target.SetActive(shouldShow);
        }

        private static void SetInteractableIfAvailable(
            Button button,
            bool isInteractable)
        {
            if (button != null)
            {
                button.interactable = isInteractable;
            }
        }
    }
}
