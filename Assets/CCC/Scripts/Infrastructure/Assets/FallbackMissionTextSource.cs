using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Presentation;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// MissionTextAssetが未設定の場合にデバッグ用文言を提供するSourceです。
    /// </summary>
    public sealed class FallbackMissionTextSource : IMissionTextSource
    {
        private readonly HashSet<MissionKind> _reportedKinds = new HashSet<MissionKind>();

        /// <summary>
        /// MissionTextAsset未設定を通知し、Fallback文言を返します。
        /// </summary>
        /// <param name="kind">ミッションの種類。</param>
        /// <param name="isNegated">否定条件かどうか。</param>
        /// <returns>デバッグ用のMissionKind表示。</returns>
        public string GetTemplate(MissionKind kind, bool isNegated)
        {
            if (_reportedKinds.Add(kind))
            {
                Debug.LogError(
                    $"[FallbackMissionTextSource] MissionTextAsset is missing. " +
                    $"Using fallback text for MissionKind.{kind}.");
            }

            return $"MissionKind.{kind}";
        }
    }
}
