using UnityEngine;
using UnityEngine.UI;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// タイマーインジケーターの表示モードです。
    /// </summary>
    public enum TimerIndicatorMode
    {
        /// <summary>
        /// 残り時間が減るほどゲージを減らします。
        /// </summary>
        Decrease = 0,

        /// <summary>
        /// 残り時間が減るほどゲージを増やします。
        /// </summary>
        Increase
    }

    /// <summary>
    /// 残り時間の割合を円形ゲージとして表示します。
    /// </summary>
    public sealed class TimerIndicator : MonoBehaviour
    {
        private const float MinimumFillRatio = 0f;
        private const float MaximumFillRatio = 1f;

        [SerializeField] private Image _fillImage;
        [SerializeField] private TimerIndicatorMode _mode;
        [SerializeField] private Gradient _colorGradient;
        [SerializeField] private AnimationCurve _colorEasing;

        /// <summary>
        /// 残り時間割合をゲージと色へ反映します。
        /// </summary>
        /// <param name="remainingTimeRatio">残り時間割合。0から1の範囲。</param>
        public void Render(float remainingTimeRatio)
        {
            if (_fillImage == null)
            {
                return;
            }

            if (float.IsNaN(remainingTimeRatio) ||
                float.IsInfinity(remainingTimeRatio))
            {
                _fillImage.fillAmount = MinimumFillRatio;
                return;
            }

            float normalizedRemainingTimeRatio = Mathf.Clamp(
                remainingTimeRatio,
                MinimumFillRatio,
                MaximumFillRatio);

            float fillRatio = _mode == TimerIndicatorMode.Decrease
                ? normalizedRemainingTimeRatio
                : MaximumFillRatio - normalizedRemainingTimeRatio;

            _fillImage.fillAmount = fillRatio;

            if (_colorGradient == null)
            {
                return;
            }

            float colorRatio = normalizedRemainingTimeRatio;
            if (_colorEasing != null)
            {
                colorRatio = _colorEasing.Evaluate(colorRatio);
            }

            colorRatio = Mathf.Clamp(
                colorRatio,
                MinimumFillRatio,
                MaximumFillRatio);

            _fillImage.color = _colorGradient.Evaluate(colorRatio);
        }
    }
}