using DG.Tweening;
using UnityEngine;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    /// カードの表裏を回転させるアニメーションを再生します。
    /// </summary>
    public class CardFlipAnimation : MonoBehaviour
    {
        [Header("アニメーション設定")]
        [SerializeField] private float duration = 0.08f;
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private Vector2 pivot = new Vector2(0.5f, 0.5f);

        private Sequence flipSequence;
        private RectTransform rectTransform;
        private Quaternion defaultRotation;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            defaultRotation = rectTransform.localRotation;
            rectTransform.pivot = pivot;
        }

        /// <summary>
        /// 表 → 裏 → 表
        /// </summary>
        [ContextMenu("Play Flip Animation")]
        public void Play()
        {
            flipSequence?.Kill();

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            rectTransform.localRotation = defaultRotation;

            flipSequence = DOTween.Sequence();
            flipSequence.Append(
                rectTransform
                    .DOLocalRotate(new Vector3(0f, 90f, 0f), duration)
                    .SetEase(ease)
            );
            flipSequence.Append(
                rectTransform
                    .DOLocalRotate(defaultRotation.eulerAngles, duration)
                    .SetEase(ease)
            );
        }

        private void OnDestroy()
        {
            flipSequence?.Kill();
        }
    }

    /// <summary>
    /// カードの向きを横向きに変えた後に元の向きへ戻すアニメーションを再生します。
    /// </summary>
    public class CardRotateAnimation : MonoBehaviour
    {
        [Header("アニメーション設定")]
        [SerializeField] private float duration = 0.08f;
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private Vector2 pivot = new Vector2(0.5f, 0.5f);

        private Sequence rotateSequence;
        private RectTransform rectTransform;
        private Quaternion defaultRotation;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            defaultRotation = rectTransform.localRotation;
            rectTransform.pivot = pivot;
        }

        /// <summary>
        /// 正面 → 横向き → 正面
        /// </summary>
        public void Play()
        {
            rotateSequence?.Kill();

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            rectTransform.localRotation = defaultRotation;

            rotateSequence = DOTween.Sequence();
            rotateSequence.Append(
                rectTransform
                    .DOLocalRotate(new Vector3(0f, 90f, 0f), duration)
                    .SetEase(ease)
            );
            rotateSequence.Append(
                rectTransform
                    .DOLocalRotate(defaultRotation.eulerAngles, duration)
                    .SetEase(ease)
            );
        }

        private void OnDestroy()
        {
            rotateSequence?.Kill();
        }
    }
}
