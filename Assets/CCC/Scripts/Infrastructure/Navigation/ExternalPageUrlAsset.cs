using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// アプリ外で開くWebページのURLを保持するScriptableObjectです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/External Page Url")]
    public sealed class ExternalPageUrlAsset : ScriptableObject
    {
        [Tooltip("プライバシーポリシーのURL。httpsから始まる絶対URLを設定する。")]
        [SerializeField] private string _privacyPolicyUrl;

        /// <summary>
        /// プライバシーポリシーのURLを取得します。
        /// </summary>
        public string PrivacyPolicyUrl => _privacyPolicyUrl;

        /// <summary>
        /// 設定されたURLを検証します。
        /// </summary>
        /// <returns>検証エラーの一覧。問題がない場合は空の一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            ValidateHttpsUrl(errors, _privacyPolicyUrl, nameof(PrivacyPolicyUrl));
            return errors;
        }

        private static void ValidateHttpsUrl(List<string> errors, string url, string fieldName)
        {
            bool isHttpsUrl =
                Uri.TryCreate(url, UriKind.Absolute, out Uri uri) &&
                uri.Scheme == Uri.UriSchemeHttps;
            if (!isHttpsUrl)
            {
                errors.Add($"{fieldName} must be an absolute https URL.");
            }
        }
    }
}
