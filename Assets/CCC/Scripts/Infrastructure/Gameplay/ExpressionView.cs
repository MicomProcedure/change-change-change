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
        [Tooltip("ExpressionDisplayStyle.Plainで表示するSelector一式。")]
        [SerializeField] private ExpressionDisplaySet _plainDisplaySet;
        [Tooltip("ExpressionDisplayStyle.ChoiceIndicatorで表示するSelector一式。")]
        [SerializeField] private ExpressionDisplaySet _choiceIndicatorDisplaySet;
        [SerializeField] private TMP_Text _answerText;
        [SerializeField] private TextBounceAnimator _answerBounceAnimator;

        private readonly List<IDisposable> _inputSubscriptions = new List<IDisposable>();
        private Subject<GameplayInput> _inputsSubject;
        private ExpressionDisplaySet _activeDisplaySet;
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
        /// 指定された表示スタイルのSelector一式だけを表示し、以降の表示と入力の対象にします。
        /// 対応する一式が未設定の場合は、すべての一式を非表示にして式の表示と入力を無効にします。
        /// </summary>
        /// <param name="displayStyle">表示するスタイル。</param>
        /// <returns>対応する一式を表示できた場合はtrue。未設定の場合はfalse。</returns>
        public bool TryApplyDisplayStyle(ExpressionDisplayStyle displayStyle)
        {
            ExpressionDisplaySet matchedDisplaySet = GetDisplaySet(displayStyle);
            ExpressionDisplaySet[] displaySets = GetDisplaySets();
            for (int index = 0; index < displaySets.Length; index++)
            {
                ExpressionDisplaySet displaySet = displaySets[index];
                if (displaySet != null)
                {
                    displaySet.SetVisible(displaySet == matchedDisplaySet);
                }
            }

            _activeDisplaySet = matchedDisplaySet;
            return matchedDisplaySet != null;
        }

        /// <summary>
        /// 式と答えの表示を更新します。
        /// 答えの表示文言が変わった場合は答えを跳ねさせます。
        /// </summary>
        /// <param name="leftText">式の左側。</param>
        /// <param name="operatorText">式の演算子。</param>
        /// <param name="rightText">式の右側。</param>
        /// <param name="leftIndex">式の左側の、選択肢の中での位置。</param>
        /// <param name="operatorIndex">式の演算子の、選択肢の中での位置。</param>
        /// <param name="rightIndex">式の右側の、選択肢の中での位置。</param>
        /// <param name="numberChoiceCount">数値の選択肢の総数。</param>
        /// <param name="operatorChoiceCount">演算子の選択肢の総数。</param>
        /// <param name="answerText">答え。</param>
        /// <param name="isInteractable">式を変更できるかどうか。</param>
        public void Render(
            string leftText,
            string operatorText,
            string rightText,
            int leftIndex,
            int operatorIndex,
            int rightIndex,
            int numberChoiceCount,
            int operatorChoiceCount,
            string answerText,
            bool isInteractable)
        {
            if (_activeDisplaySet != null)
            {
                bool isSelectorInteractable = isInteractable && _isInputEnabled;
                RenderSelector(
                    _activeDisplaySet.LeftNumberView,
                    new CycleSelectorStateDto(leftText, isSelectorInteractable, leftIndex, numberChoiceCount));
                RenderSelector(
                    _activeDisplaySet.OperatorView,
                    new CycleSelectorStateDto(operatorText, isSelectorInteractable, operatorIndex, operatorChoiceCount));
                RenderSelector(
                    _activeDisplaySet.RightNumberView,
                    new CycleSelectorStateDto(rightText, isSelectorInteractable, rightIndex, numberChoiceCount));
            }

            RenderAnswer(answerText);
        }

        /// <summary>
        /// 式入力の受付状態を切り替えます。
        /// </summary>
        /// <param name="isEnabled">入力を受け付ける場合はtrue。</param>
        public void SetInputEnabled(bool isEnabled)
        {
            _isInputEnabled = isEnabled;
            // 表示スタイルの適用前に呼ばれても受付状態が揃うよう、非表示の一式にも反映する
            ExpressionDisplaySet[] displaySets = GetDisplaySets();
            for (int index = 0; index < displaySets.Length; index++)
            {
                ExpressionDisplaySet displaySet = displaySets[index];
                if (displaySet == null)
                {
                    continue;
                }

                RenderSelectorInputState(displaySet.LeftNumberView, isEnabled);
                RenderSelectorInputState(displaySet.OperatorView, isEnabled);
                RenderSelectorInputState(displaySet.RightNumberView, isEnabled);
            }
        }

        private ExpressionDisplaySet GetDisplaySet(ExpressionDisplayStyle displayStyle)
        {
            switch (displayStyle)
            {
                case ExpressionDisplayStyle.Plain:
                    return _plainDisplaySet;
                case ExpressionDisplayStyle.ChoiceIndicator:
                    return _choiceIndicatorDisplaySet;
                default:
                    return null;
            }
        }

        private ExpressionDisplaySet[] GetDisplaySets()
        {
            return new[] { _plainDisplaySet, _choiceIndicatorDisplaySet };
        }

        private void ConnectSelectorInputs()
        {
            ExpressionDisplaySet[] displaySets = GetDisplaySets();
            for (int index = 0; index < displaySets.Length; index++)
            {
                ExpressionDisplaySet displaySet = displaySets[index];
                if (displaySet == null)
                {
                    continue;
                }

                SubscribeSelector(displaySet, displaySet.LeftNumberView, GameplayInputKind.ChangeLeftNumber);
                SubscribeSelector(displaySet, displaySet.OperatorView, GameplayInputKind.ChangeOperator);
                SubscribeSelector(displaySet, displaySet.RightNumberView, GameplayInputKind.ChangeRightNumber);
            }
        }

        private void SubscribeSelector(
            ExpressionDisplaySet displaySet,
            CycleSelectorView selector,
            GameplayInputKind inputKind)
        {
            if (selector == null || selector.ChangeRequested == null || _inputsSubject == null)
            {
                return;
            }

            // 各一式のSelectorは同じボタンを購読しているため、表示中の一式からの入力だけを流す
            _inputSubscriptions.Add(selector.ChangeRequested
                .Where(_ => displaySet == _activeDisplaySet)
                .Subscribe(_ => _inputsSubject.OnNext(new GameplayInput(inputKind))));
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
            CycleSelectorStateDto state)
        {
            if (selector != null)
            {
                selector.Render(state);
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
