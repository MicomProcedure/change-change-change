using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class PanelScaleAnimation : MonoBehaviour
{
    [Header("表示")]
    [FormerlySerializedAs("showDuration")]
    [SerializeField] private float _showDuration = 0.4f;

    [FormerlySerializedAs("startScale")]
    [SerializeField] private float _startScale = 0.7f;

    [FormerlySerializedAs("showEase")]
    [SerializeField] private Ease _showEase = Ease.OutBack;

    [Header("消去")]
    [FormerlySerializedAs("hideDuration")]
    [SerializeField] private float _hideDuration = 0.3f;

    [FormerlySerializedAs("endScale")]
    [SerializeField] private float _endScale = 0.7f;

    [FormerlySerializedAs("hideEase")]
    [SerializeField] private Ease _hideEase = Ease.InBack;

    private CanvasGroup _canvasGroup;
    private Vector3 _defaultScale;
    private bool _isInitialized;
    private bool _isHiding;

    /// <summary>
    /// パネルが表示状態へ遷移中または表示済みかどうかを取得します。
    /// </summary>
    public bool IsShowing { get; private set; }

    private void Awake()
    {
        EnsureInitialized();
    }

    /// <summary>
    /// パネルを表示し、表示アニメーションを再生します。
    /// </summary>
    public void Show()
    {
        EnsureInitialized();

        if (IsShowing && gameObject.activeSelf)
        {
            return;
        }

        gameObject.SetActive(true);

        KillAnimation();
        _isHiding = false;

        transform.localScale = _defaultScale * _startScale;
        _canvasGroup.alpha = 0f;

        transform
            .DOScale(_defaultScale, _showDuration)
            .SetEase(_showEase);

        _canvasGroup
            .DOFade(1f, _showDuration)
            .SetEase(Ease.OutQuad);

        IsShowing = true;
    }

    /// <summary>
    /// パネルを非表示にし、非表示アニメーションを再生します。
    /// </summary>
    public void Hide()
    {
        EnsureInitialized();

        if (!gameObject.activeSelf)
        {
            IsShowing = false;
            _isHiding = false;
            return;
        }

        if (_isHiding)
        {
            return;
        }

        KillAnimation();
        IsShowing = false;
        _isHiding = true;

        transform
            .DOScale(_defaultScale * _endScale, _hideDuration)
            .SetEase(_hideEase);

        _canvasGroup
            .DOFade(0f, _hideDuration)
            .SetEase(_hideEase)
            .OnComplete(() =>
            {
                _isHiding = false;
                gameObject.SetActive(false);
            });
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        _defaultScale = transform.localScale;
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _isInitialized = true;
    }

    private void KillAnimation()
    {
        transform.DOKill();
        _canvasGroup.DOKill();
    }

    private void OnDestroy()
    {
        transform.DOKill();
        _canvasGroup?.DOKill();
    }
}
