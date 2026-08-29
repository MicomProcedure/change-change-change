using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI ImageのFilled機能（Horizontal）を使い、左から右へチェックマークを描画し、
/// 一定時間待機してからフェードアウトして非表示にするスクリプト
/// </summary>
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class PekeView : MonoBehaviour
{
    [Header("1. Draw Settings (描画)")]
    [Tooltip("描画（左から右）にかかる時間（秒）")]
    [SerializeField] private float drawDuration = 0.2f;
    [SerializeField] private Ease drawEase = Ease.Linear;

    [Header("2. Delay & Fade Settings (待機とフェード)")]
    [Tooltip("描画完了からフェードアウトを開始するまでの待機時間（秒）")]
    [SerializeField] private float waitDuration = 0.4f;
    [Tooltip("フェードアウトにかかる時間（秒）")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private Ease fadeEase = Ease.Linear;

    [Header("Auto Play")]
    [SerializeField] private bool playOnEnable = false;

    private Image targetImage;
    private CanvasGroup canvasGroup;
    private Sequence currentSequence;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    /// <summary>
    /// アニメーションを一連の流れで再生します
    /// </summary>
    [ContextMenu("Play Draw Sequence")]
    public void Play()
    {
        if (targetImage == null || canvasGroup == null) return;

        // 進行中のアニメーションがあれば安全に停止・破棄
        currentSequence?.Kill();

        // --- 初期状態のリセット ---
        gameObject.SetActive(true);
        canvasGroup.alpha = 1.0f;          // 不透明に戻す
        targetImage.fillAmount = 0.0f;     // チェックマークを消す

        // --- DOTween Sequence（一連のアニメーションの流れ）を作成 ---
        currentSequence = DOTween.Sequence();

        // Step A: ImageのFilled機能で左から右へ描画する
        currentSequence.Append(
            targetImage.DOFillAmount(1.0f, drawDuration)
                        .SetEase(drawEase)
        );

        // Step B: 指定した秒数だけ待機する
        currentSequence.AppendInterval(waitDuration);

        // Step C: CanvasGroupを使ってフェードアウトする
        currentSequence.Append(
            canvasGroup.DOFade(0.0f, fadeDuration)
                       .SetEase(fadeEase)
        );

        // Step D: 全て終わったらオブジェクトを非表示にする
        currentSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void OnDestroy()
    {
        currentSequence?.Kill();
    }
}