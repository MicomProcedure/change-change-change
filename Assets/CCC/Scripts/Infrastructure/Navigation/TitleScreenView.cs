using System;
using Ccc.Presentation.Navigation;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;
using Ccc.Presentation;

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

        [Header("ボタン")]
        [SerializeField] private Button _enterMainMenuButton;
        [SerializeField] private Button _openStageButton;

        [FormerlySerializedAs("_difficultySelectionButton")]
        [SerializeField] private Button _startGameplayButton;

        [SerializeField] private Button _openOptionButton;
        [SerializeField] private Button _closeOptionButton;
        [SerializeField] private Button _openCreditButton;
        [SerializeField] private Button _closeCreditButton;

        [Header("エラー表示")]
        [SerializeField] private TMP_Text _errorText;

        private TitleScreenModel _model;
        private IRuntimeMode _runtimeMode;
        private IDisposable _screenSubscription;
        private IDisposable _busySubscription;
        private IDisposable _errorSubscription;
        private bool _isInitialized;

        [Inject]
        private void Construct(TitleScreenModel model, IRuntimeMode runtimeMode)
        {
            _model = model;
            _runtimeMode = runtimeMode;
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

            _screenSubscription =
                _model.CurrentScreen.Subscribe(RenderScreen);

            _busySubscription =
                _model.IsBusy.Subscribe(RenderBusyState);

            _errorSubscription =
                _model.ErrorMessage.Subscribe(RenderErrorMessage);

            RenderScreen(_model.CurrentScreen.CurrentValue);
            RenderBusyState(_model.IsBusy.CurrentValue);
            RenderErrorMessage(_model.ErrorMessage.CurrentValue);

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            _screenSubscription?.Dispose();
            _busySubscription?.Dispose();
            _errorSubscription?.Dispose();

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

        private void HandleStartGameplay()
        {
            _model.StartGameplayAsync().Forget(HandleAsyncFailure);
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
