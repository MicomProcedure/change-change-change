using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// 保存データを同一視するアプリケーションバージョンの1グループです。
    /// </summary>
    [Serializable]
    public sealed class VersionGroup
    {
        [SerializeField] private List<string> _versions = new List<string>();

        /// <summary>
        /// グループに属するバージョンを取得します。
        /// </summary>
        public IReadOnlyCollection<string> Versions
        {
            get { return _versions; }
        }
    }
}
