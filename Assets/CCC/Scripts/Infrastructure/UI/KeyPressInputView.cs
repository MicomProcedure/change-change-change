using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    ///     Input Action の 1 回の押下を 1 回の押下イベントへ変換し、押下中は同じ GameObject の Button を押下表示にする入力コンポーネント。
    /// </summary>
    /// <remarks>
    ///     押した瞬間に 1 回だけ発行し、押し続けても連続発行しない。
    ///     押下表示は同じ GameObject 上の <see cref="ExternalPressButton"/> に委ねる。
    /// </remarks>
    public sealed class KeyPressInputView : PressInputView
    {
        [SerializeField] private InputActionReference _pressAction;

        private readonly Subject<Unit> _pressedSubject = new();
        private ExternalPressButton _pressVisualButton;
        private bool _isInteractable = true;
        private bool _isKeyHeld;

        /// <summary>
        ///     押下イベントの発行元。
        /// </summary>
        public override Observable<Unit> Pressed => _pressedSubject;

        private void Awake()
        {
            _pressVisualButton = GetComponent<ExternalPressButton>();
            if (_pressVisualButton == null)
            {
                Debug.LogWarning(
                    $"{nameof(KeyPressInputView)}: {nameof(ExternalPressButton)}が同じGameObjectにありません。押下表示は行われません。",
                    this);
            }

            if (_pressAction == null)
            {
                Debug.LogWarning(
                    $"{nameof(KeyPressInputView)}: {nameof(_pressAction)}が未設定です。キー入力は無効になります。",
                    this);
            }
        }

        private void OnEnable()
        {
            if (_pressAction == null)
            {
                return;
            }

            InputAction action = _pressAction.action;
            action.performed += OnPressPerformed;
            action.canceled += OnPressCanceled;
            action.Enable();
        }

        private void OnDisable()
        {
            if (_pressAction != null)
            {
                InputAction action = _pressAction.action;
                action.performed -= OnPressPerformed;
                action.canceled -= OnPressCanceled;
                action.Disable();
            }

            ReleaseKey();
        }

        private void OnDestroy()
        {
            _pressedSubject.OnCompleted();
            _pressedSubject.Dispose();
        }

        /// <summary>
        ///     入力の受付可否を切り替える。受付不可になった時点で押下表示を解除する。
        /// </summary>
        /// <param name="isInteractable">入力受付可否。</param>
        public override void SetInteractable(bool isInteractable)
        {
            _isInteractable = isInteractable;
            if (!isInteractable)
            {
                ReleaseKey();
            }
        }

        private void OnPressPerformed(InputAction.CallbackContext context)
        {
            if (!_isInteractable)
            {
                return;
            }

            _isKeyHeld = true;
            ApplyPressVisual();
            _pressedSubject.OnNext(Unit.Default);
        }

        private void OnPressCanceled(InputAction.CallbackContext context)
        {
            ReleaseKey();
        }

        private void ReleaseKey()
        {
            if (!_isKeyHeld)
            {
                return;
            }

            _isKeyHeld = false;
            ApplyPressVisual();
        }

        private void ApplyPressVisual()
        {
            if (_pressVisualButton != null)
            {
                _pressVisualButton.SetExternallyPressed(_isKeyHeld);
            }
        }
    }
}
