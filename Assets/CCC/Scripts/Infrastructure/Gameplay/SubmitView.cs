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
        private UnityEngine.Events.UnityAction _submitAction;

        /// <summary>
        /// 提出入力のストリームを取得します。
        /// </summary>
        public Observable<GameplayInput> Inputs { get; private set; }

        private void Awake()
        {
            _inputsSubject = new Subject<GameplayInput>();
            Inputs = _inputsSubject;
        }

        private void Start()
        {
            if (_submitButton == null || _inputsSubject == null)
            {
                return;
            }

            _submitAction =
                () => _inputsSubject.OnNext(new GameplayInput(GameplayInputKind.Submit));
            _submitButton.onClick.AddListener(_submitAction);
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
        }

        private void OnDestroy()
        {
            if (_submitButton != null && _submitAction != null)
            {
                _submitButton.onClick.RemoveListener(_submitAction);
            }

            if (_inputsSubject != null)
            {
                _inputsSubject.OnCompleted();
                _inputsSubject.Dispose();
            }
        }
    }
}
