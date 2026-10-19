using System.Runtime.InteropServices;
using UnityEngine;

namespace Ccc.Infrastructure.Platform
{
    /// <summary>
    /// Web 版の画面表示を調整する。
    /// 起動時に描画倍率を端末の画面密度へ合わせ、全画面表示の切り替えを提供する。
    /// </summary>
    public sealed class WebScreenDisplayController : MonoBehaviour
    {
        [SerializeField, Tooltip("描画倍率の上限。大きいほど高精細だが負荷と発熱が増える")]
        private float _maxDevicePixelRatio = 2f;

        [SerializeField, Tooltip("全画面ボタンのルート。全画面に対応していない環境では非表示にする")]
        private GameObject _fullscreenButtonRoot;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void WebScreenDisplay_SetDevicePixelRatioLimit(float maxRatio);

        [DllImport("__Internal")]
        private static extern int WebScreenDisplay_IsFullscreenSupported();
#endif

        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebScreenDisplay_SetDevicePixelRatioLimit(_maxDevicePixelRatio);
            _fullscreenButtonRoot.SetActive(WebScreenDisplay_IsFullscreenSupported() == 1);
#else
            _fullscreenButtonRoot.SetActive(false);
#endif
        }

        /// <summary>
        /// 全画面表示を切り替える。UI Button の OnClick から呼ぶこと。
        /// </summary>
        public void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}