using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 〇を描画した後、一定時間待機してフェードアウトし、自動で非表示にするスクリプト
/// </summary>
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))] // フェード処理を安全に行うために必須
public class CircleShaderDrawer : MonoBehaviour
{
    [Header("Shader Property Settings")]
    [Tooltip("シェーダー内のプロパティ名")]
    [SerializeField] private string propertyName = "_Progress";

    [Header("1. Draw Settings (〇の描画)")]
    [Tooltip("描画アニメーションにかかる時間（秒）")]
    [SerializeField] private float drawDuration = 1.0f;
    [SerializeField] private float startValue = 0.0f;
    [SerializeField] private float endValue = 1.0f;
    [SerializeField] private Ease drawEase = Ease.OutQuad;

    [Header("2. Delay & Fade Settings (待機とフェード)")]
    [Tooltip("描画完了からフェードアウトを開始するまでの待機時間（秒）")]
    [SerializeField] private float waitDuration = 0.5f;

    [Tooltip("フェードアウトにかかる時間（秒）")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Tooltip("フェード時のイージング")]
    [SerializeField] private Ease fadeEase = Ease.Linear;

    [Header("Auto Play")]
    [SerializeField] private bool playOnEnable = false;

    private Image targetImage;
    private CanvasGroup canvasGroup;
    private Material instantiatedMaterial;
    private Sequence currentSequence;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (targetImage != null && targetImage.material != null)
        {
            instantiatedMaterial = targetImage.material;
        }
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
    /// 「描画」 -> 「待機」 -> 「フェードアウト」 -> 「非表示」
    /// </summary>
    [ContextMenu("Play Animation Sequence")]
    public void Play()
    {
        if (instantiatedMaterial == null || canvasGroup == null) return;

        // 進行中のアニメーション（Sequence）があれば安全に停止・破棄
        currentSequence?.Kill();

        // 1. 初期状態のリセット
        gameObject.SetActive(true);
        canvasGroup.alpha = 1.0f; // 不透明に戻す
        instantiatedMaterial.SetFloat(propertyName, startValue); // 描画を0に戻す

        // 2. DOTween Sequence（一連のアニメーションの流れ）を作成
        currentSequence = DOTween.Sequence();

        // Step A: 〇を描画する
        currentSequence.Append(
            instantiatedMaterial.DOFloat(endValue, propertyName, drawDuration)
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
        // オブジェクト破棄時にSequenceを安全に破棄
        currentSequence?.Kill();
    }
}