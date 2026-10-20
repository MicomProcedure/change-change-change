using System.Runtime.InteropServices;
using UnityEngine;

namespace Ccc.Infrastructure.Platform
{
    /// <summary>
    /// Web 版の画面表示を調整する。
    /// 起動時に描画倍率を端末の画面密度へ合わせる。
    /// </summary>
    public sealed class WebScreenDisplayController : MonoBehaviour
    {
        [SerializeField, Tooltip("描画倍率の上限。大きいほど高精細だが負荷と発熱が増える")]
        private float _maxDevicePixelRatio = 2f;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void WebScreenDisplay_SetDevicePixelRatioLimit(float maxRatio);
#endif

        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebScreenDisplay_SetDevicePixelRatioLimit(_maxDevicePixelRatio);
#endif
        }
    }
}
