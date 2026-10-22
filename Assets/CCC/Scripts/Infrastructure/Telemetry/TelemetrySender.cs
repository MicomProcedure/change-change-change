using System;
using System.Net;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace Ccc.Infrastructure.Telemetry
{
    /// <summary>
    /// プレイデータをGAS WebアプリへPOSTします。失敗した場合は警告ログを出して破棄します。
    /// </summary>
    public sealed class TelemetrySender
    {
        private const string JsonContentType = "application/json";

        private readonly TelemetrySettingsAsset _settings;
        private readonly ILogger<TelemetrySender> _logger;

        /// <summary>
        /// TelemetrySenderを生成します。
        /// </summary>
        /// <param name="settings">送信設定。</param>
        /// <param name="logger">送信結果を記録するLogger。</param>
        /// <exception cref="ArgumentNullException">依存のいずれかがnullの場合。</exception>
        public TelemetrySender(TelemetrySettingsAsset settings, ILogger<TelemetrySender> logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 送信完了を待たずにプレイデータを送信します。
        /// </summary>
        /// <param name="payload">送信するプレイデータ。</param>
        public void SendWithoutWaiting(TelemetryPayloadDto payload)
        {
            SendAsync(payload).Forget();
        }

        private async UniTaskVoid SendAsync(TelemetryPayloadDto payload)
        {
            // Scene遷移で送信を止めないよう、CancellationTokenは渡さずタイムアウトだけで打ち切る
            try
            {
                string json = JsonUtility.ToJson(payload);
                using UnityWebRequest request = UnityWebRequest.Post(_settings.EndpointUrl, json, JsonContentType);
                request.timeout = _settings.TimeoutSeconds;
                await request.SendWebRequest();
                string responseText = request.downloadHandler.text;
                if (responseText.Trim() == _settings.SuccessResponseText)
                {
                    _logger.LogDebug("Telemetry sent. ResponseCode: {ResponseCode}", request.responseCode);
                    return;
                }

                _logger.LogWarning(
                    "Telemetry was rejected by the endpoint. ResponseCode: {ResponseCode}, Url: {Url}, Body: {Body}",
                    request.responseCode,
                    request.url,
                    ExtractReadableText(responseText));
            }
            catch (Exception exception)
            {
                _logger.LogWarning("Telemetry send failed and was discarded: {Message}", exception.Message);
            }
        }

        private string ExtractReadableText(string responseText)
        {
            // GASのエラー画面はHTMLのため、script・style・タグを除いてエラーメッセージを読める形にする
            string withoutBlocks = Regex.Replace(
                responseText,
                "<(script|style)[^>]*>.*?</\\1>",
                " ",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            string withoutTags = Regex.Replace(withoutBlocks, "<[^>]+>", " ");
            string readableText = Regex.Replace(WebUtility.HtmlDecode(withoutTags), "\\s+", " ").Trim();
            return readableText.Length <= _settings.LoggedResponseMaxLength
                ? readableText
                : readableText.Substring(0, _settings.LoggedResponseMaxLength);
        }
    }
}
