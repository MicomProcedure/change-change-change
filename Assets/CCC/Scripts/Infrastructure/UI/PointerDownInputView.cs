using System;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ccc.Infrastructure.UI
{
    /// <summary>
    ///     1 回のポインターダウンを 1 回の押下イベントへ変換する共通入力コンポーネント。
    /// </summary>
    public sealed class PointerDownInputView : MonoBehaviour, IPointerDownHandler
    {
        private Subject<Unit> _pressedSubject;
        private bool _isDestroyed;
        private bool _isInteractable = true;

        /// <summary>
        ///     押下イベントの発行元。
        /// </summary>
        public Observable<Unit> Pressed { get; private set; }

        private void Awake()
        {
            _pressedSubject = new Subject<Unit>();
            Pressed = _pressedSubject;
        }

        private void OnDestroy()
        {
            _isDestroyed = true;

            if (_pressedSubject != null)
            {
                _pressedSubject.OnCompleted();
                _pressedSubject.Dispose();
            }
        }

        /// <summary>
        ///     入力の受付可否を切り替える。
        /// </summary>
        /// <param name="isInteractable">入力受付可否。</param>
        public void SetInteractable(bool isInteractable)
        {
            _isInteractable = isInteractable;
        }

        /// <summary>
        ///     PointerDown を受け取り、有効時に 1 回の Unit を発行する。
        /// </summary>
        /// <param name="eventData">Unity のポインタイベントデータ。</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isDestroyed)
            {
                return;
            }

            if (eventData == null)
            {
                throw new ArgumentNullException(nameof(eventData));
            }

            if (!_isInteractable)
            {
                return;
            }

            var isMouse = eventData.pointerId == -1;
            if (isMouse && eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _pressedSubject.OnNext(Unit.Default);
        }
    }
}
