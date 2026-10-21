using System;
using System.Collections.Generic;
using Ccc.Infrastructure.UI;
using Ccc.Presentation;
using Ccc.Presentation.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面の式入力と答え表示を扱うViewです。
    /// </summary>
    public sealed class ExpressionView : MonoBehaviour
    {
        [SerializeField] private CycleSelectorView _leftNumberView;
        [SerializeField] private CycleSelectorView _operatorView;
        [SerializeField] private CycleSelectorView _rightNumberView;
        [SerializeField] private TMP_Text _answerText;
        [SerializeField] private TextBounceAnimator _answerBounceAnimator;

        private readonly List<IDisposable> _inputSubscriptions = new List<IDisposable>();
        private Subject<GameplayInput> _inputsSubject;
        private bool _isInputEnabled = true;

        /// <summary>
        /// 式入力で発生したGameplay入力のストリームを取得します。
        /// </summary>
        public Observable<GameplayInput> Inputs { get; private set; }

        private void Awake()
        {
            _inputsSubject = new Subject<GameplayInput>();
            Inputs = _inputsSubject;
        }

        private void Start()
        {
            ConnectSelectorInputs();
        }

        /// <summary>
        /// 式と答えの表示を更新します。
        /// 答えの表示文言が変わった場合は答えを跳ねさせます。
        /// </summary>
        /// <param name="leftText">式の左側。</param>
        /// <param name="operatorText">式の演算子。</param>
        /// <param name="rightText">式の右側。</param>
        /// <param name="answerText">答え。</param>
        /// <param name="isInteractable">式を変更できるかどうか。</param>
        public void Render(
            string leftText,
            string operatorText,
            string rightText,
            string answerText,
            bool isInteractable)
        {
            RenderSelector(_leftNumberView, leftText, isInteractable && _isInputEnabled);
            RenderSelector(_operatorView, operatorText, isInteractable && _isInputEnabled);
            RenderSelector(_rightNumberView, rightText, isInteractable && _isInputEnabled);
            RenderAnswer(answerText);
        }

        /// <summary>
        /// 式入力の受付状態を切り替えます。
        /// </summary>
        /// <param name="isEnabled">入力を受け付ける場合はtrue。</param>
        public void SetInputEnabled(bool isEnabled)
        {
            _isInputEnabled = isEnabled;
            RenderSelectorInputState(_leftNumberView, isEnabled);
            RenderSelectorInputState(_operatorView, isEnabled);
            RenderSelectorInputState(_rightNumberView, isEnabled);
        }

        private void ConnectSelectorInputs()
        {
            SubscribeSelector(_leftNumberView, GameplayInputKind.ChangeLeftNumber);
            SubscribeSelector(_operatorView, GameplayInputKind.ChangeOperator);
            SubscribeSelector(_rightNumberView, GameplayInputKind.ChangeRightNumber);
        }

        private void SubscribeSelector(CycleSelectorView selector, GameplayInputKind inputKind)
        {
            if (selector == null || selector.ChangeRequested == null || _inputsSubject == null)
            {
                return;
            }

            _inputSubscriptions.Add(selector.ChangeRequested.Subscribe(
                _ => _inputsSubject.OnNext(new GameplayInput(inputKind))));
        }

        private void RenderAnswer(string answerText)
        {
            // Renderはタイマー更新ごとにも呼ばれるため、表示文言が変わった時だけ更新して跳ねさせる
            if (_answerText == null || _answerText.text == answerText)
            {
                return;
            }

            _answerText.text = answerText;
            if (_answerBounceAnimator != null)
            {
                _answerBounceAnimator.Play();
            }
        }

        private static void RenderSelector(
            CycleSelectorView selector,
            string valueText,
            bool isInteractable)
        {
            if (selector != null)
            {
                selector.Render(new CycleSelectorStateDto(valueText, isInteractable));
            }
        }

        private static void RenderSelectorInputState(
            CycleSelectorView selector,
            bool isEnabled)
        {
            if (selector != null)
            {
                selector.SetInputEnabled(isEnabled);
            }
        }

        private void OnDestroy()
        {
            for (int index = 0; index < _inputSubscriptions.Count; index++)
            {
                _inputSubscriptions[index].Dispose();
            }

            _inputSubscriptions.Clear();
            if (_inputsSubject != null)
            {
                _inputsSubject.OnCompleted();
                _inputsSubject.Dispose();
            }
        }
    }
}
