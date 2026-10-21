using UnityEngine;
using UnityEngine.UI;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    ///     CanvasScaler の基準解像度の大きさで配置した UI を、親の矩形に収まるよう縦横同じ倍率で拡大縮小するコンポーネント。
    ///     KeepAspect では親の縦横比が基準解像度と異なる場合、余った方向は余白になる。
    ///     FillParent では倍率は同じまま、大きさを広げて親の矩形全体を覆う。
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ReferenceResolutionFitter : MonoBehaviour
    {
        private const DrivenTransformProperties DrivenProperties =
            DrivenTransformProperties.Anchors |
            DrivenTransformProperties.Pivot |
            DrivenTransformProperties.AnchoredPosition |
            DrivenTransformProperties.SizeDelta |
            DrivenTransformProperties.Scale;

        private static readonly Vector2 CenterPoint = new(0.5f, 0.5f);

        /// <summary>
        ///     親の矩形への合わせ方。
        /// </summary>
        public enum FitMode
        {
            /// <summary>基準解像度の縦横比を保ち、親に収まる大きさにする。余った方向は余白になる。</summary>
            KeepAspect,

            /// <summary>倍率は KeepAspect と同じまま、親の矩形全体を覆う大きさにする。</summary>
            FillParent,
        }

        [Tooltip("配置の基準解像度を持つ CanvasScaler。")]
        [SerializeField] private CanvasScaler _canvasScaler;

        [Tooltip("親の矩形への合わせ方。")]
        [SerializeField] private FitMode _fitMode = FitMode.KeepAspect;

        private RectTransform _rectTransform;
        private RectTransform _parentRectTransform;
        private DrivenRectTransformTracker _drivenTracker;
        private Vector2 _lastParentSize;
        private Vector2 _lastReferenceResolution;
        private FitMode _lastFitMode;

        private void OnEnable()
        {
            _rectTransform = (RectTransform)transform;
            _parentRectTransform = transform.parent as RectTransform;

            if (!HasValidReferences())
            {
                return;
            }

            _drivenTracker.Add(this, _rectTransform, DrivenProperties);
            ApplyFit();
        }

        private void OnDisable()
        {
            _drivenTracker.Clear();
        }

        private void OnTransformParentChanged()
        {
            _parentRectTransform = transform.parent as RectTransform;

            if (!isActiveAndEnabled || !HasValidReferences())
            {
                return;
            }

            ApplyFit();
        }

        private void LateUpdate()
        {
            if (_canvasScaler == null || _parentRectTransform == null)
            {
                return;
            }

            bool hasParentSizeChanged = _parentRectTransform.rect.size != _lastParentSize;
            bool hasReferenceResolutionChanged = _canvasScaler.referenceResolution != _lastReferenceResolution;
            bool hasFitModeChanged = _fitMode != _lastFitMode;

            if (!hasParentSizeChanged && !hasReferenceResolutionChanged && !hasFitModeChanged)
            {
                return;
            }

            ApplyFit();
        }

        private bool HasValidReferences()
        {
            if (_canvasScaler == null)
            {
                Debug.LogError($"{nameof(ReferenceResolutionFitter)}: {nameof(_canvasScaler)}が未設定です。", this);
                return false;
            }

            if (_parentRectTransform == null)
            {
                Debug.LogError($"{nameof(ReferenceResolutionFitter)}: 親に RectTransform がありません。", this);
                return false;
            }

            return true;
        }

        private void ApplyFit()
        {
            Vector2 referenceResolution = _canvasScaler.referenceResolution;
            Vector2 parentSize = _parentRectTransform.rect.size;

            _lastParentSize = parentSize;
            _lastReferenceResolution = referenceResolution;
            _lastFitMode = _fitMode;

            // 起動直後など、大きさが確定していない間は倍率を計算できないため適用しない
            if (referenceResolution.x <= 0f || referenceResolution.y <= 0f || parentSize.x <= 0f || parentSize.y <= 0f)
            {
                return;
            }

            float fitScale = Mathf.Min(parentSize.x / referenceResolution.x, parentSize.y / referenceResolution.y);

            _rectTransform.anchorMin = CenterPoint;
            _rectTransform.anchorMax = CenterPoint;
            _rectTransform.pivot = CenterPoint;
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.sizeDelta = _fitMode == FitMode.FillParent
                ? parentSize / fitScale
                : referenceResolution;
            _rectTransform.localScale = new Vector3(fitScale, fitScale, 1f);
        }
    }
}
