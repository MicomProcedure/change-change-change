using DG.Tweening;
using UnityEngine;

public class CardAppearAnimation : MonoBehaviour
{
    [Header("出現")]
    [SerializeField] private float showDuration = 0.2f;
    [SerializeField] private float startScale = 0.7f;
    [SerializeField] private Ease showEase = Ease.OutBack;

    [Header("消滅")]
    [SerializeField] private float hideDuration = 0.2f;
    [SerializeField] private float moveDistance = 50f;
    [SerializeField] private Ease hideEase = Ease.InQuad;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private Vector3 defaultScale;
    private Vector2 defaultPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        // CanvasGroupがなければ自動追加
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 本来の状態を保存
        defaultScale = transform.localScale;
        defaultPosition = rectTransform.anchoredPosition;
    }

    /// <summary>
    /// フェードしながら拡大して出現
    /// </summary>
    public void Show()
    {
        rectTransform.DOKill();
        canvasGroup.DOKill();

        // 初期状態
        canvasGroup.alpha = 0f;
        transform.localScale = defaultScale * startScale;
        rectTransform.anchoredPosition = defaultPosition;

        // フェード
        canvasGroup
            .DOFade(1f, showDuration)
            .SetEase(showEase);

        // 拡大
        transform
            .DOScale(defaultScale, showDuration)
            .SetEase(showEase);
    }

    /// <summary>
    /// フェードしながら下に移動して消滅
    /// </summary>
    public void Hide()
    {
        rectTransform.DOKill();
        canvasGroup.DOKill();

        Vector2 targetPosition =
            defaultPosition + Vector2.down * moveDistance;

        // フェード
        canvasGroup
            .DOFade(0f, hideDuration)
            .SetEase(hideEase);

        // 下へ移動
        rectTransform
            .DOAnchorPos(targetPosition, hideDuration)
            .SetEase(hideEase);
    }

    private void OnDestroy()
    {
        rectTransform?.DOKill();
        canvasGroup?.DOKill();
    }
}