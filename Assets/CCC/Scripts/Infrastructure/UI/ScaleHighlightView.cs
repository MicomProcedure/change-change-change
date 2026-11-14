using DG.Tweening;
using UnityEngine;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    /// アタッチしたUI要素を拡大縮小し続けることで強調表示するViewです。
    /// </summary>
    public sealed class ScaleHighlightView : MonoBehaviour
    {
        private const float DefaultScaleMultiplier = 1.1f;
        private const float DefaultHalfCycleDurationSeconds = 0.5f;
        private const Ease DefaultEase = Ease.InOutSine;

        [Tooltip("元のスケールに対する、拡大時のスケールの倍率。")]
        [SerializeField] private float _scaleMultiplier = DefaultScaleMultiplier;
        [Tooltip("元のスケールから拡大時のスケールへ変化するまでの秒数。")]
        [SerializeField] private float _halfCycleDurationSeconds = DefaultHalfCycleDurationSeconds;
        [SerializeField] private Ease _ease = DefaultEase;

        private Vector3 _defaultScale;
        private Tween _scaleTween;

        private void Awake()
        {
            _defaultScale = transform.localScale;
        }

        /// <summary>
        /// 強調表示の拡大縮小を開始し、<see cref="Stop"/>が呼ばれるまで繰り返します。
        /// 再生中または非アクティブな状態で呼ばれた場合は何もしません。
        /// </summary>
        [ContextMenu("Play Scale Highlight")]
        public void Play()
        {
            if (!isActiveAndEnabled || (_scaleTween != null && _scaleTween.IsActive()))
            {
                return;
            }

            // 同じTransformへのDOKillなどで外部から止められていた場合も、元のスケールから再開する
            Stop();

            _scaleTween = transform
                .DOScale(_defaultScale * _scaleMultiplier, _halfCycleDurationSeconds)
                .SetEase(_ease)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// 強調表示の拡大縮小を停止し、すぐに元のスケールへ戻します。
        /// 再生中でない場合は何もしません。
        /// </summary>
        [ContextMenu("Stop Scale Highlight")]
        public void Stop()
        {
            if (_scaleTween == null)
            {
                return;
            }

            if (_scaleTween.IsActive())
            {
                _scaleTween.Kill();
            }

            _scaleTween = null;
            transform.localScale = _defaultScale;
        }

        private void OnDisable()
        {
            // 拡大途中のスケールのまま次回表示されないよう、非表示になる時点で元に戻す
            Stop();
        }
    }
}
