using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ccc.Infrastructure.Gameplay
{
    /// <summary>
    /// Gameplay画面の設定エラーと回復不能エラーを表示するViewです。
    /// </summary>
    public sealed class GameplayErrorView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private GameObject _errorPanel;

        /// <summary>
        /// 設定エラー一覧を表示します。
        /// </summary>
        /// <param name="errors">表示するエラー一覧。</param>
        public void ShowConfigurationError(IReadOnlyList<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            Show(string.Join("\n", errors));
        }

        /// <summary>
        /// 回復不能エラーを表示します。
        /// </summary>
        /// <param name="message">表示するエラーメッセージ。</param>
        public void ShowFatalError(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Show(message);
        }

        private void Show(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
            }

            if (_errorPanel != null)
            {
                _errorPanel.SetActive(true);
            }
        }
    }
}
