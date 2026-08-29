using DG.Tweening;
using UnityEngine;

public class EndPanelAnimation : MonoBehaviour
{
    [Header("対象")]
    [SerializeField] private RectTransform endPanel;

    [Header("アニメーション設定")]
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private RectTransform canvasRect;
    private Vector2 showPosition;
    private Vector2 hiddenPosition;

    private void Awake()
    {
        // CanvasのRectTransformを取得
        Canvas canvas = endPanel.GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        // Inspectorで設定した表示位置を保存
        showPosition = endPanel.anchoredPosition;

        // Canvasの上端より完全に上へ移動する位置
        float hiddenY =
            canvasRect.rect.height / 2f
            + endPanel.rect.height / 2f;

        hiddenPosition = new Vector2(
            showPosition.x,
            showPosition.y + hiddenY
        );

        // 実行開始時点では画面外
        endPanel.anchoredPosition = hiddenPosition;
    }

    /// <summary>
    /// EndPanelを上から画面内へ出す
    /// </summary>
    public void Show()
    {
        endPanel.DOKill();

        // 非アクティブだったEndPanelを表示
        endPanel.gameObject.SetActive(true);

        // 念のため画面外から開始
        endPanel.anchoredPosition = hiddenPosition;

        // 画面内へ移動
        endPanel
            .DOAnchorPos(showPosition, duration)
            .SetEase(ease);
    }

    /// <summary>
    /// EndPanelを上へ戻して非表示にする
    /// </summary>
    public void Hide()
    {
        endPanel.DOKill();

        endPanel
            .DOAnchorPos(hiddenPosition, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                endPanel.gameObject.SetActive(false);
            });
    }

    private void OnDestroy()
    {
        endPanel?.DOKill();
    }
}