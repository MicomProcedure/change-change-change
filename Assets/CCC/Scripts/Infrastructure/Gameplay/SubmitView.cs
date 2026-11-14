using System;
using System.Collections.Generic;
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
        private PressInputView[] _submitInputViews = Array.Empty<PressInputView>();
        private readonly List<IDisposable> _submitInputSubscriptions = new List<IDisposable>();
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
            _submitInputViews = GetComponents<PressInputView>();
        }

        private void Start()
        {
            if (_inputsSubject == null)
            {
                return;
            }

            for (int index = 0; index < _submitInputViews.Length; index++)
            {
                _submitInputSubscriptions.Add(_submitInputViews[index].Pressed.Subscribe(
                    _ => _inputsSubject.OnNext(new GameplayInput(GameplayInputKind.Submit))));
            }
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

            for (int index = 0; index < _submitInputViews.Length; index++)
            {
                _submitInputViews[index].SetInteractable(canInteract);
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
            for (int index = 0; index < _submitInputSubscriptions.Count; index++)
            {
                _submitInputSubscriptions[index].Dispose();
            }

            _submitInputSubscriptions.Clear();

            if (_inputsSubject != null)
            {
                _inputsSubject.OnCompleted();
                _inputsSubject.Dispose();
            }
        }
    }
}
