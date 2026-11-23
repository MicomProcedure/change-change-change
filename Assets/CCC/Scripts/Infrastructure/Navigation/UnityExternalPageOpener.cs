using System;
using Ccc.Presentation.Navigation;
using UnityEngine;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// UnityのApplication.OpenURLでアプリ外のWebページを開きます。
    /// </summary>
    public sealed class UnityExternalPageOpener : IExternalPageOpener
    {
        private readonly ExternalPageUrlAsset _externalPageUrlAsset;

        /// <summary>
        /// UnityExternalPageOpenerを生成します。
        /// </summary>
        /// <param name="externalPageUrlAsset">開くWebページのURLを保持するAsset。検証済みであること。</param>
        /// <exception cref="ArgumentNullException">引数がnullの場合。</exception>
        public UnityExternalPageOpener(ExternalPageUrlAsset externalPageUrlAsset)
        {
            _externalPageUrlAsset = externalPageUrlAsset != null
                ? externalPageUrlAsset
                : throw new ArgumentNullException(nameof(externalPageUrlAsset));
        }

        /// <summary>
        /// プライバシーポリシーのWebページを開きます。
        /// </summary>
        public void OpenPrivacyPolicy()
        {
            Application.OpenURL(_externalPageUrlAsset.PrivacyPolicyUrl);
        }
    }
}
