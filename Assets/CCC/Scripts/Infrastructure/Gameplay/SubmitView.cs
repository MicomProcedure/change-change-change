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
            if (_submitButton != null)
            {
                _submitButton.interactable = isInteractable;
            }

            if (_submitInputView != null)
            {
                _submitInputView.SetInteractable(isInteractable);
            }
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
