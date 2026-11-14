using UnityEngine.UI;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    ///     ポインター以外の入力からも押下中の見た目へ遷移できる Button。
    /// </summary>
    /// <remarks>
    ///     押下中の見た目は Button に設定された Transition（SpriteSwap / ColorTint など）をそのまま用いる。
    ///     Disabled 状態は外部からの押下より優先する。
    /// </remarks>
    public sealed class ExternalPressButton : Button
    {
        private bool _isExternallyPressed;

        /// <summary>
        ///     外部入力による押下状態を設定し、見た目へ反映する。
        /// </summary>
        /// <param name="isPressed">外部入力で押下中の場合は true。</param>
        public void SetExternallyPressed(bool isPressed)
        {
            if (_isExternallyPressed == isPressed)
            {
                return;
            }

            _isExternallyPressed = isPressed;
            DoStateTransition(currentSelectionState, false);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            _isExternallyPressed = false;
            base.OnDisable();
        }

        /// <inheritdoc />
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(ResolveDisplayState(state), instant);
        }

        private SelectionState ResolveDisplayState(SelectionState state)
        {
            if (_isExternallyPressed && state != SelectionState.Disabled)
            {
                return SelectionState.Pressed;
            }

            return state;
        }
    }
}
