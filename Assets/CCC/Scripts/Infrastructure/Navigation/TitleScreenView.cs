using System;
using Ccc.Presentation.Navigation;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
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
        [SerializeField] private Button _startGameplayButton;
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

        [Inject]
        private void Construct(TitleScreenModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        private void Start()
        {
            ValidateReferences();

            _enterMainMenuButton.onClick.AddListener(_model.EnterMainMenu);
            _startGameplayButton.onClick.AddListener(HandleStartGameplay);
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
        }

        private void OnDestroy()
        {
            _screenSubscription?.Dispose();
            _busySubscription?.Dispose();
            _errorSubscription?.Dispose();

            if (_model == null)
            {
                return;
            }

            _enterMainMenuButton.onClick.RemoveListener(_model.EnterMainMenu);
            _startGameplayButton.onClick.RemoveListener(HandleStartGameplay);
            _openOptionButton.onClick.RemoveListener(_model.OpenOptionMenu);
            _closeOptionButton.onClick.RemoveListener(_model.CloseOptionMenu);
            _openCreditButton.onClick.RemoveListener(_model.OpenCredit);
            _closeCreditButton.onClick.RemoveListener(_model.CloseCredit);
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
            _startGameplayButton.interactable = isInteractable;
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

        private void ValidateReferences()
        {
            if (_model == null ||
                _titleScreenRoot == null ||
                _mainMenuRoot == null ||
                _optionMenuRoot == null ||
                _creditRoot == null ||
                _enterMainMenuButton == null ||
                _startGameplayButton == null ||
                _openOptionButton == null ||
                _closeOptionButton == null ||
                _openCreditButton == null ||
                _closeCreditButton == null)
            {
                throw new InvalidOperationException(
                    "TitleScreenViewのInspector参照が不足しています。");
            }
        }
    }
}