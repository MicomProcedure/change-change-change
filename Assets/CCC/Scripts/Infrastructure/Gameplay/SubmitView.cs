using System;
using Ccc.Infrastructure.UI;
using Ccc.Presentation;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// 式の提出ボタンを表示し、提出入力を公開するViewです。
    /// </summary>
    public sealed class SubmitView : MonoBehaviour
    {
        [SerializeField] private Button _submitButton;

        private Subject<GameplayInput> _inputsSubject;
        private PointerDownInputView _submitInputView;
        private IDisposable _submitInputSubscription;
        private bool _isInputEnabled = true;
        private bool _isSubmitInteractable;

        /// <summary>
        /// 提出入力のストリームを取得します。
        /// </summary>
        public Observable<GameplayInput> Inputs { get; private set; }

        private void Awake()
        {
            _inputsSubject = new Subject<GameplayInput>();
            Inputs = _inputsSubject;
            _submitInputView = GetComponent<PointerDownInputView>();
        }

        private void Start()
        {
            if (_submitInputView == null || _inputsSubject == null)
            {
                return;
            }

            _submitInputSubscription = _submitInputView.Pressed.Subscribe(
                _ => _inputsSubject.OnNext(new GameplayInput(GameplayInputKind.Submit)));
        }

        /// <summary>
        /// 提出ボタンの操作可能状態を更新します。
        /// </summary>
        /// <param name="isInteractable">提出可能かどうか。</param>
        public void Render(bool isInteractable)
        {
            _isSubmitInteractable = isInteractable;
            bool canInteract = isInteractable && _isInputEnabled;
            if (_submitButton != null)
            {
                _submitButton.interactable = canInteract;
            }

            if (_submitInputView != null)
            {
                _submitInputView.SetInteractable(canInteract);
            }
        }

        /// <summary>
        /// 提出入力の受付状態を切り替えます。
        /// </summary>
        /// <param name="isEnabled">入力を受け付ける場合はtrue。</param>
        public void SetInputEnabled(bool isEnabled)
        {
            _isInputEnabled = isEnabled;
            Render(_isSubmitInteractable);
        }

        private void OnDestroy()
        {
            _submitInputSubscription?.Dispose();

            if (_inputsSubject != null)
            {
                _inputsSubject.OnCompleted();
                _inputsSubject.Dispose();
            }
        }
    }
}
