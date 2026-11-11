using Ccc.Core.UseCases;
using UnityEngine;

namespace Ccc.Infrastructure.Persistence
{
    /// <summary>
    /// UnityのPlayer設定からアプリケーションバージョンを提供します。
    /// </summary>
    public sealed class UnityApplicationVersionProvider : IApplicationVersionProvider
    {
        /// <summary>
        /// 実行中のアプリケーションバージョンを取得します。
        /// </summary>
        public string CurrentVersion => Application.version;
    }
}
