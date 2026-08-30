using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay開始前のCountdown表示を担当するViewです。
    /// </summary>
    public sealed class CountdownView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _countdownText;

        /// <summary>
        /// Countdownの残り秒数を表示します。
        /// </summary>
        /// <param name="remainingSeconds">表示する残り秒数。</param>
        public void ShowCountdown(int remainingSeconds)
        {
            if (remainingSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(remainingSeconds),
                    remainingSeconds,
                    "Countdown seconds must be positive.");
            }

            if (_countdownText == null)
            {
                return;
            }

            _countdownText.text = remainingSeconds.ToString(CultureInfo.InvariantCulture);
            _countdownText.gameObject.SetActive(true);
        }

        /// <summary>
        /// Countdown表示を非表示にします。
        /// </summary>
        public void HideCountdown()
        {
            if (_countdownText == null)
            {
                return;
            }

            _countdownText.text = string.Empty;
            _countdownText.gameObject.SetActive(false);
        }
    }
}
