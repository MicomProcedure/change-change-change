using DG.Tweening;
using UnityEngine;

public class CardRotateAnimation : MonoBehaviour
{
    [Header("アニメーション設定")]
    [SerializeField] private float duration = 0.08f;
    [SerializeField] private Ease ease = Ease.Linear;

    private Sequence rotateSequence;

    /// <summary>
    /// 正面 → 横向き → 正面
    /// </summary>
    public void Play()
    {
        // 再生中のアニメーションがあれば停止
        rotateSequence?.Kill();

        // 現在の回転をリセット
        transform.localRotation = Quaternion.identity;

        rotateSequence = DOTween.Sequence();

        // 正面 → 横向き
        rotateSequence.Append(
            transform.DOLocalRotate(
                new Vector3(0f, 90f, 0f),
                duration
            )
            .SetEase(ease)
        );

        // 横向き → 正面
        rotateSequence.Append(
            transform.DOLocalRotate(
                Vector3.zero,
                duration
            )
            .SetEase(ease)
        );
    }

    private void OnDestroy()
    {
        rotateSequence?.Kill();
    }
}