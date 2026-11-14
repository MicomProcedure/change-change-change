using DG.Tweening;
using UnityEngine;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    /// アタッチしたRectTransformを上方向へ跳ねさせ、元の位置へ戻すアニメーションを再生します。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class TextBounceAnimator : MonoBehaviour
    {
        private const float DefaultJumpHeight = 12f;
        private const float DefaultRiseDurationSeconds = 0.08f;
        private const float DefaultFallDurationSeconds = 0.25f;

        [Header("跳ね上がり")]
        [SerializeField] private float _jumpHeight = DefaultJumpHeight;
        [SerializeField] private float _riseDurationSeconds = DefaultRiseDurationSeconds;
        [SerializeField] private Ease _riseEase = Ease.OutQuad;

        [Header("着地")]
        [SerializeField] private float _fallDurationSeconds = DefaultFallDurationSeconds;
        [SerializeField] private Ease _fallEase = Ease.OutBounce;

        private RectTransform _rectTransform;
        private Vector2 _defaultAnchoredPosition;
        private Sequence _bounceSequence;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _defaultAnchoredPosition = _rectTransform.anchoredPosition;
        }

        /// <summary>
        /// 跳ねるアニメーションを最初から再生します。
        /// 再生中に呼ばれた場合は元の位置へ戻してから再生し直します。
        /// </summary>
        [ContextMenu("Play Bounce Animation")]
        public void Play()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            StopAndResetPosition();

            float defaultPositionY = _defaultAnchoredPosition.y;
            _bounceSequence = DOTween.Sequence()
                .Append(_rectTransform
                    .DOAnchorPosY(defaultPositionY + _jumpHeight, _riseDurationSeconds)
                    .SetEase(_riseEase))
                .Append(_rectTransform
                    .DOAnchorPosY(defaultPositionY, _fallDurationSeconds)
                    .SetEase(_fallEase));
        }

        private void StopAndResetPosition()
        {
            _bounceSequence?.Kill();
            _bounceSequence = null;
            _rectTransform.anchoredPosition = _defaultAnchoredPosition;
        }

        private void OnDisable()
        {
            // 跳ねている途中で非表示になっても、元の位置から再開できるようにする
            StopAndResetPosition();
        }
    }
}
