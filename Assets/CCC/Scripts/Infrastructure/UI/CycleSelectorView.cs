using System;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Ccc.Presentation.UI;
using TMPro;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    ///     現在値表示と change 要求入力をまとめて扱うビュー。
    /// </summary>
    public sealed class CycleSelectorView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private PointerDownInputView changeInputView;
        [Tooltip("入力受付可否の視覚的なフィードバックをCanvasGroupで設定する場合はこちらを利用。")]
        [SerializeField] private CanvasGroup interactionCanvasGroup;
        [SerializeField] private Graphic disabledGraphic;

        private Subject<Unit> _changeRequestedSubject = new();
        private IDisposable _pressedSubscription;

        /// <summary>
        ///     変更要求のイベント。
        /// </summary>
        public Observable<Unit> ChangeRequested { get; private set; }

        private void Awake()
        {
            ValidateSerializedFields();

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
                UnityEngine.Debug.LogWarning(
                    "CycleSelectorView has neither a CanvasGroup nor a disabled Graphic for visual feedback. " +
                    "Consider adding one for better user experience.");
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
