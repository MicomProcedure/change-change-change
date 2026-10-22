using System;
using System.Collections.Generic;
using Ccc.Presentation;
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
        [SerializeField] private List<PressInputView> changeInputViews = new();
        [Tooltip("入力受付可否の視覚的なフィードバックをCanvasGroupで設定する場合はこちらを利用。")]
        [SerializeField] private CanvasGroup interactionCanvasGroup;
        [SerializeField] private Graphic disabledGraphic;
        [SerializeField] private CardFlipAnimation flipAnimation;
        [Tooltip("位置を示す丸を並べる親。未設定の場合は位置を表示しない。")]
        [SerializeField] private RectTransform _indicatorRoot;
        [SerializeField] private Image _indicatorSlotPrefab;
        [SerializeField] private Sprite _selectedSlotSprite;
        [SerializeField] private Sprite _unselectedSlotSprite;
        [Tooltip("位置を示す丸の色。カードの枠の色に合わせる。")]
        [SerializeField] private Color _indicatorColor = Color.white;

        private Subject<Unit> _changeRequestedSubject = new();
        private readonly List<IDisposable> _pressedSubscriptions = new();
        private IRuntimeMode _runtimeMode;
        private bool _referencesValidated;
        private bool _canFlipAnimate = false;
        private string _lastValueText;
        private readonly List<Image> _indicatorSlotImages = new();
        private int _renderedSelectedIndex = -1;
        private int _renderedChoiceCount = -1;

        /// <summary>
        ///     変更要求のイベント。
        /// </summary>
        public Observable<Unit> ChangeRequested => _changeRequestedSubject;

        private void Awake()
        {
            if (changeInputViews == null)
            {
                return;
            }

            for (int index = 0; index < changeInputViews.Count; index++)
            {
                PressInputView changeInputView = changeInputViews[index];
                if (changeInputView == null)
                {
                    continue;
                }

                _pressedSubscriptions.Add(
                    changeInputView.Pressed.Subscribe(_ => _changeRequestedSubject.OnNext(Unit.Default)));
            }

            if (flipAnimation == null)
            {
                Debug.LogWarning(
                    $"{nameof(CycleSelectorView)}: {nameof(flipAnimation)}が未設定です。Flipアニメーションは再生されません。",
                    this);
                _canFlipAnimate = false;
            }
            else
            {
                _canFlipAnimate = true;
            }
        }

        [VContainer.Inject]
        private void InjectRuntimeMode(IRuntimeMode runtimeMode)
        {
            _runtimeMode = runtimeMode;
            ValidateSerializedFields();
        }

        private void Start()
        {
            ValidateSerializedFields();
        }

        private void OnDestroy()
        {
            for (int index = 0; index < _pressedSubscriptions.Count; index++)
            {
                _pressedSubscriptions[index].Dispose();
            }

            _pressedSubscriptions.Clear();

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

            if (changeInputViews != null)
            {
                for (int index = 0; index < changeInputViews.Count; index++)
                {
                    PressInputView changeInputView = changeInputViews[index];
                    if (changeInputView != null)
                    {
                        changeInputView.SetInteractable(state.IsInteractable);
                    }
                }
            }

            // ValueTextの変更があった場合のみFlipアニメーションを再生する
            if (_lastValueText != state.ValueText)
            {
                _lastValueText = state.ValueText;
                RenderIndicator(state.SelectedIndex, state.ChoiceCount);
                TryPlayFlipAnimation(state);
            }


            // ApplyDisabledVisualState(state.IsInteractable);
        }

        /// <summary>
        /// Selector入力の受付状態を切り替えます。
        /// </summary>
        /// <param name="isEnabled">入力を受け付ける場合はtrue。</param>
        public void SetInputEnabled(bool isEnabled)
        {
            if (changeInputViews != null)
            {
                for (int index = 0; index < changeInputViews.Count; index++)
                {
                    PressInputView changeInputView = changeInputViews[index];
                    if (changeInputView != null)
                    {
                        changeInputView.SetInteractable(isEnabled);
                    }
                }
            }

            ApplyDisabledVisualState(isEnabled);
        }

        private void ValidateSerializedFields()
        {
            if (_referencesValidated)
            {
                return;
            }

            _referencesValidated = true;
            if (valueText == null)
            {
                LogMissingReference("valueText");
            }

            if (changeInputViews == null || changeInputViews.Count == 0)
            {
                LogMissingReference("changeInputViews");
            }
            else
            {
                for (int index = 0; index < changeInputViews.Count; index++)
                {
                    if (changeInputViews[index] == null)
                    {
                        LogMissingReference($"changeInputViews[{index}]");
                    }
                }
            }

            if (interactionCanvasGroup == null && disabledGraphic == null)
            {
                UnityEngine.Debug.LogWarning(
                    "CycleSelectorView has neither a CanvasGroup nor a disabled Graphic for visual feedback. " +
                    "Consider adding one for better user experience.");
            }
        }

        private void LogMissingReference(string fieldName)
        {
            string message =
                $"{nameof(CycleSelectorView)}: {fieldName}が未設定です。" +
                (_runtimeMode != null && _runtimeMode.IsDebugMode
                    ? "DebugModeのため処理を継続します。"
                    : "該当するUI入力または表示を無効にして処理を継続します。");
            if (_runtimeMode != null && _runtimeMode.IsDebugMode)
            {
                Debug.LogError(message, this);
            }
            else
            {
                Debug.LogWarning(message, this);
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

        private void TryPlayFlipAnimation(CycleSelectorStateDto state)
        {
            if (_canFlipAnimate)
            {
                flipAnimation.Play();
            }
        }

        private void RenderIndicator(int selectedIndex, int choiceCount)
        {
            // Gameplay ではタイマー更新ごとに Render されるため、位置が変わった時だけ描画し直す
            if (_indicatorRoot == null ||
                _indicatorSlotPrefab == null ||
                (_renderedSelectedIndex == selectedIndex && _renderedChoiceCount == choiceCount))
            {
                return;
            }

            _renderedSelectedIndex = selectedIndex;
            _renderedChoiceCount = choiceCount;
            _indicatorRoot.gameObject.SetActive(choiceCount > 0);

            while (_indicatorSlotImages.Count < choiceCount)
            {
                Image slotImage = Instantiate(_indicatorSlotPrefab, _indicatorRoot);
                slotImage.color = _indicatorColor;
                _indicatorSlotImages.Add(slotImage);
            }

            for (int index = 0; index < _indicatorSlotImages.Count; index++)
            {
                bool isUsed = index < choiceCount;
                _indicatorSlotImages[index].gameObject.SetActive(isUsed);
                if (isUsed)
                {
                    _indicatorSlotImages[index].sprite =
                        index == selectedIndex ? _selectedSlotSprite : _unselectedSlotSprite;
                }
            }
        }
    }
}
