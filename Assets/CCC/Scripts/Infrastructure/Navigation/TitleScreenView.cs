using System;
using Ccc.Presentation.Navigation;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

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
        [SerializeField] private GameObject _optionMenuRoot;
        [SerializeField] private GameObject _creditRoot;

        [Header("ボタン")]
        [SerializeField] private Button _enterMainMenuButton;

        [FormerlySerializedAs("_startGameplayButton")]
        [SerializeField] private Button _difficultySelectionButton;

        [SerializeField] private Button _openOptionButton;
        [SerializeField] private Button _closeOptionButton;
        [SerializeField] private Button _openCreditButton;
        [SerializeField] private Button _closeCreditButton;

        [Header("エラー表示")]
        [SerializeField] private TMP_Text _errorText;

        private TitleScreenModel _model;
        private IDisposable _screenSubscription;
        private IDisposable _busySubscription;
        private IDisposable _errorSubscription;
        private bool _isInitialized;

        [Inject]
        private void Construct(TitleScreenModel model)
        {
            _model = model;
        }

        private void Start()
        {
            if (!ValidateReferences())
            {
                return;
            }

            _enterMainMenuButton.onClick.AddListener(_model.EnterMainMenu);
            _difficultySelectionButton.onClick.AddListener(HandleStartGameplay);
            _openOptionButton.onClick.AddListener(_model.OpenOptionMenu);
            _closeOptionButton.onClick.AddListener(_model.CloseOptionMenu);
            _openCreditButton.onClick.AddListener(_model.OpenCredit);
            _closeCreditButton.onClick.AddListener(_model.CloseCredit);

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

            if (_difficultySelectionButton != null)
            {
                _difficultySelectionButton.onClick.RemoveListener(
                    HandleStartGameplay);
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
            _titleScreenRoot.SetActive(
                screenState == TitleScreenState.TitleScreen);

            _mainMenuRoot.SetActive(
                screenState == TitleScreenState.MainMenu);

            _optionMenuRoot.SetActive(
                screenState == TitleScreenState.OptionMenu);

            _creditRoot.SetActive(
                screenState == TitleScreenState.Credit);
        }

        private void RenderBusyState(bool isBusy)
        {
            bool isInteractable = !isBusy;

            _enterMainMenuButton.interactable = isInteractable;
            _difficultySelectionButton.interactable = isInteractable;
            _openOptionButton.interactable = isInteractable;
            _closeOptionButton.interactable = isInteractable;
            _openCreditButton.interactable = isInteractable;
            _closeCreditButton.interactable = isInteractable;
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
            bool isValid = true;

            if (_model == null)
            {
                LogMissingReference(nameof(_model));
                isValid = false;
            }

            if (_titleScreenRoot == null)
            {
                LogMissingReference(nameof(_titleScreenRoot));
                isValid = false;
            }

            if (_mainMenuRoot == null)
            {
                LogMissingReference(nameof(_mainMenuRoot));
                isValid = false;
            }

            if (_optionMenuRoot == null)
            {
                LogMissingReference(nameof(_optionMenuRoot));
                isValid = false;
            }

            if (_creditRoot == null)
            {
                LogMissingReference(nameof(_creditRoot));
                isValid = false;
            }

            if (_enterMainMenuButton == null)
            {
                LogMissingReference(nameof(_enterMainMenuButton));
                isValid = false;
            }

            if (_difficultySelectionButton == null)
            {
                LogMissingReference(nameof(_difficultySelectionButton));
                isValid = false;
            }

            if (_openOptionButton == null)
            {
                LogMissingReference(nameof(_openOptionButton));
                isValid = false;
            }

            if (_closeOptionButton == null)
            {
                LogMissingReference(nameof(_closeOptionButton));
                isValid = false;
            }

            if (_openCreditButton == null)
            {
                LogMissingReference(nameof(_openCreditButton));
                isValid = false;
            }

            if (_closeCreditButton == null)
            {
                LogMissingReference(nameof(_closeCreditButton));
                isValid = false;
            }

            if (_errorText == null)
            {
                Debug.LogWarning(
                    $"{nameof(TitleScreenView)}: " +
                    $"{nameof(_errorText)}が未設定です。 " +
                    "エラー表示は無効になります。",
                    this);
            }

            return isValid;
        }

        private void LogMissingReference(string fieldName)
        {
            Debug.LogWarning(
                $"{nameof(TitleScreenView)}: " +
                $"{fieldName}が未設定です。Title画面の初期化を中止します。",
                this);
        }
    }
}