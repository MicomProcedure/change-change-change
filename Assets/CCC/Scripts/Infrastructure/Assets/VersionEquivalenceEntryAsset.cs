using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// 保存データの種類ごとの、バージョン同一視設定をまとめるアセットです。
    /// 同一視が必要な情報が増えた場合は、ここへ設定を追加します。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Version Equivalence Entry")]
    public sealed class VersionEquivalenceEntryAsset : ScriptableObject
    {
        [SerializeField] private HighScoreVersionEquivalenceAsset _highScore;

        /// <summary>
        /// 設定内容を検証します。
        /// </summary>
        /// <returns>検出したエラーの一覧。問題がない場合は空です。</returns>
        public IReadOnlyList<string> Validate()
        {
            if (_highScore == null)
            {
                return new[] { $"{nameof(VersionEquivalenceEntryAsset)} requires a high score setting." };
            }

            return _highScore.Validate();
        }

        /// <summary>
        /// 指定したバージョンの登録漏れを検出します。
        /// </summary>
        /// <param name="version">確認するバージョン。</param>
        /// <returns>登録漏れを示すメッセージの一覧。問題がない場合は空です。</returns>
        /// <exception cref="InvalidOperationException">設定内容が不正な場合。</exception>
        public IReadOnlyList<string> FindUnregisteredVersionErrors(string version)
        {
            var errors = new List<string>();
            IReadOnlyList<(GameMode GameMode, GameDifficulty Difficulty)> unregisteredTargets =
                CreateHighScoreCompatibilityPolicy().FindUnregisteredTargets(version);
            for (int index = 0; index < unregisteredTargets.Count; index++)
            {
                errors.Add(
                    $"Version {version} is not registered for high score of " +
                    $"GameMode.{unregisteredTargets[index].GameMode} and " +
                    $"GameDifficulty.{unregisteredTargets[index].Difficulty}.");
            }

            return errors;
        }

        /// <summary>
        /// ハイスコアの互換方針を生成します。
        /// </summary>
        /// <returns>生成した互換方針。</returns>
        /// <exception cref="InvalidOperationException">設定内容が不正な場合。</exception>
        public VersionEquivalenceHighScoreCompatibilityPolicy CreateHighScoreCompatibilityPolicy()
        {
            if (_highScore == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(VersionEquivalenceEntryAsset)} requires a high score setting.");
            }

            return _highScore.CreateCompatibilityPolicy();
        }
    }
}
