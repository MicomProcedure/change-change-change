using DG.Tweening;
using UnityEngine;

public class PanelScaleAnimation : MonoBehaviour
{
    [Header("表示")]
    [SerializeField] private float showDuration = 0.4f;
    [SerializeField] private float startScale = 0.7f;
    [SerializeField] private Ease showEase = Ease.OutBack;

    [Header("消去")]
    [SerializeField] private float hideDuration = 0.3f;
    [SerializeField] private float endScale = 0.7f;
    [SerializeField] private Ease hideEase = Ease.InBack;

    private CanvasGroup canvasGroup;
    private Vector3 defaultScale;

    public bool IsShowing { get; private set; }

    /// <summary>
    /// PanelManagerから呼び出して初期化する
    /// </summary>
    public void Initialize()
    {
        defaultScale = transform.localScale;

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);

        transform.DOKill();
        canvasGroup.DOKill();

        transform.localScale = defaultScale * startScale;
        canvasGroup.alpha = 0f;

        transform
            .DOScale(defaultScale, showDuration)
            .SetEase(showEase);

        canvasGroup
            .DOFade(1f, showDuration)
            .SetEase(Ease.OutQuad);

        IsShowing = true;
    }

    public void Hide()
    {
        transform.DOKill();
        canvasGroup.DOKill();

        transform
            .DOScale(defaultScale * endScale, hideDuration)
            .SetEase(hideEase);

        canvasGroup
            .DOFade(0f, hideDuration)
            .SetEase(hideEase)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                IsShowing = false;
            });
    }

    private void OnDestroy()
    {
        transform.DOKill();
        canvasGroup?.DOKill();
    }
}