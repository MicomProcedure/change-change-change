using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Proto.UI
{
    /// <summary>
    ///     現在値表示と change 要求入力をまとめて扱うビュー。
    /// </summary>
    public sealed class CycleSelectorView : MonoBehaviour
    {
        [SerializeField] private Text valueText;
        [SerializeField] private PointerDownInputView changeInputView;
        [SerializeField] private CanvasGroup interactionCanvasGroup;
        [SerializeField] private Graphic disabledGraphic;

        private Subject<Unit> _changeRequestedSubject;
        private IDisposable _pressedSubscription;

        /// <summary>
        ///     変更要求のイベント。
        /// </summary>
        public Observable<Unit> ChangeRequested { get; private set; }

        private void Awake()
        {
            ValidateSerializedFields();

            _changeRequestedSubject = new Subject<Unit>();
            ChangeRequested = _changeRequestedSubject;

            _pressedSubscription = changeInputView.Pressed.Subscribe(_ => _changeRequestedSubject.OnNext(Unit.Default));
        }

        private void OnDestroy()
        {
            if (_pressedSubscription != null)
            {
                _pressedSubscription.Dispose();
            }

            if (_changeRequestedSubject != null)
            {
                _changeRequestedSubject.OnCompleted();
                _changeRequestedSubject.Dispose();
            }
        }

        /// <summary>
        ///     表示状態を反映する。
        /// </summary>
        /// <param name="state">表示用の状態。</param>
        public void Render(CycleSelectorStateDto state)
        {
            if (valueText != null)
            {
                valueText.text = state.ValueText;
            }

            if (changeInputView != null)
            {
                changeInputView.SetInteractable(state.IsInteractable);
            }

            ApplyDisabledVisualState(state.IsInteractable);
        }

        private void ValidateSerializedFields()
        {
            if (valueText == null)
            {
                throw new InvalidOperationException("CycleSelectorView requires a Text reference.");
            }

            if (changeInputView == null)
            {
                throw new InvalidOperationException("CycleSelectorView requires a PointerDownInputView reference.");
            }

            if (interactionCanvasGroup == null && disabledGraphic == null)
            {
                throw new InvalidOperationException(
                    "CycleSelectorView requires a CanvasGroup or Graphic reference for disabled visuals.");
            }
        }

        private void ApplyDisabledVisualState(bool isInteractable)
        {
            if (interactionCanvasGroup != null)
            {
                interactionCanvasGroup.alpha = isInteractable ? 1f : 0.4f;
                interactionCanvasGroup.interactable = isInteractable;
                interactionCanvasGroup.blocksRaycasts = isInteractable;
            }

            if (disabledGraphic != null)
            {
                var color = disabledGraphic.color;
                color.a = isInteractable ? 1f : 0.4f;
                disabledGraphic.color = color;
            }
        }
    }
}
