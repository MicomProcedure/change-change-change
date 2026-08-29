using System;
using Ccc.Presentation;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面から他画面へ戻る入力を扱うViewです。
    /// </summary>
    public sealed class GameplayNavigationView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _returnToMainMenuButton;

        private readonly Subject<GameplayInput> _inputsSubject = new Subject<GameplayInput>();
        private UnityEngine.Events.UnityAction _retryAction;
        private UnityEngine.Events.UnityAction _returnToMainMenuAction;

        /// <summary>
        /// Navigation入力のストリームを取得します。
        /// </summary>
        public Observable<GameplayInput> Inputs
        {
            get { return _inputsSubject; }
        }

        /// <summary>
        /// Result画面を表示します。
        /// </summary>
        /// <param name="result">Result画面の表示状態。</param>
        public void RenderResult(GameplayResultViewStateDto result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (_resultText != null)
            {
                _resultText.text = result.ResultText;
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Result画面を非表示にします。
        /// </summary>
        public void HideResult()
        {
            gameObject.SetActive(false);
        }

        private void Start()
        {
            if (_retryButton != null)
            {
                _retryAction =
                    () => _inputsSubject.OnNext(new GameplayInput(GameplayInputKind.Retry));
                _retryButton.onClick.AddListener(_retryAction);
            }

            if (_returnToMainMenuButton != null)
            {
                _returnToMainMenuAction =
                    () => _inputsSubject.OnNext(new GameplayInput(GameplayInputKind.ReturnToMainMenu));
                _returnToMainMenuButton.onClick.AddListener(_returnToMainMenuAction);
            }
        }

        private void OnDestroy()
        {
            if (_retryButton != null && _retryAction != null)
            {
                _retryButton.onClick.RemoveListener(_retryAction);
            }

            if (_returnToMainMenuButton != null && _returnToMainMenuAction != null)
            {
                _returnToMainMenuButton.onClick.RemoveListener(_returnToMainMenuAction);
            }

            _inputsSubject.OnCompleted();
            _inputsSubject.Dispose();
        }
    }
}
