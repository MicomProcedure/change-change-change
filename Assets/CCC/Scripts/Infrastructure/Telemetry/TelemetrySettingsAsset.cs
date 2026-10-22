using System.Collections.Generic;
using UnityEngine;

namespace Ccc.Infrastructure.Telemetry
{
    /// <summary>
    /// プレイデータ送信の有効状態、送信先、タイムアウトをInspectorで編集するScriptableObjectです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Telemetry Settings")]
    public sealed class TelemetrySettingsAsset : ScriptableObject
    {
        private const int DefaultTimeoutSeconds = 5;
        private const string DefaultSuccessResponseText = "ok";
        private const int DefaultLoggedResponseMaxLength = 500;

        [SerializeField]
        private bool isEnabled;

        [SerializeField]
        private string endpointUrl;

        [SerializeField]
        private int timeoutSeconds = DefaultTimeoutSeconds;

        [SerializeField]
        private string successResponseText = DefaultSuccessResponseText;

        [SerializeField]
        private int loggedResponseMaxLength = DefaultLoggedResponseMaxLength;

        /// <summary>
        /// GASが正常に記録したときに返す応答本文を取得します。GASはエラー時もHTTP 200を返すため、本文で成否を判定します。
        /// </summary>
        public string SuccessResponseText => successResponseText;

        /// <summary>
        /// 失敗時にログへ出す応答本文の最大文字数を取得します。
        /// </summary>
        public int LoggedResponseMaxLength => loggedResponseMaxLength;

        /// <summary>
        /// プレイデータ送信を有効にするかどうかを取得します。
        /// </summary>
        public bool IsEnabled => isEnabled;

        /// <summary>
        /// 送信先のGAS WebアプリURLを取得します。
        /// </summary>
        public string EndpointUrl => endpointUrl;

        /// <summary>
        /// 送信のタイムアウト秒数を取得します。
        /// </summary>
        public int TimeoutSeconds => timeoutSeconds;

        /// <summary>
        /// Inspector設定を検証し、検出されたエラー一覧を返します。
        /// </summary>
        /// <returns>設定エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(endpointUrl))
            {
                errors.Add("Telemetry endpoint url must not be blank.");
            }

            if (timeoutSeconds <= 0)
            {
                errors.Add("Telemetry timeout seconds must be positive.");
            }

            if (string.IsNullOrEmpty(successResponseText))
            {
                errors.Add("Telemetry success response text must not be empty.");
            }

            if (loggedResponseMaxLength <= 0)
            {
                errors.Add("Telemetry logged response max length must be positive.");
            }

            return errors;
        }
    }
}
