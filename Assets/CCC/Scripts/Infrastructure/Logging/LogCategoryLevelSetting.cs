using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// カテゴリ別の最低LogLevel上書きをInspectorで編集するSerializable設定です。
    /// </summary>
    [Serializable]
    public sealed class LogCategoryLevelSetting
    {
        [SerializeField]
        private string categoryPrefix;

        [SerializeField]
        private LogLevel minimumLevel = LogLevel.Information;

        /// <summary>
        /// Inspector設定を検証し、検出したエラーを追加します。
        /// </summary>
        /// <param name="errors">エラーの追加先。</param>
        /// <param name="index">一覧内の位置。</param>
        public void Validate(List<string> errors, int index)
        {
            if (string.IsNullOrWhiteSpace(categoryPrefix))
            {
                errors.Add($"Category level override #{index} must have a category prefix.");
            }
        }

        /// <summary>
        /// Inspector設定から実行時の上書き設定を生成します。
        /// </summary>
        /// <returns>カテゴリ別の最低LogLevel上書き設定。</returns>
        public LogCategoryLevelOverride CreateOverride()
        {
            return new LogCategoryLevelOverride(categoryPrefix, minimumLevel);
        }
    }
}
