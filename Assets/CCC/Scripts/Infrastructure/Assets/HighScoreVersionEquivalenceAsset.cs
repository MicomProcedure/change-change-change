using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// ゲームモードと難易度の組ごとに、ハイスコアを同一視するバージョンを設定するアセットです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/High Score Version Equivalence")]
    public sealed class HighScoreVersionEquivalenceAsset : ScriptableObject
    {
        [SerializeField]
        private List<HighScoreVersionEquivalenceEntry> _entries = new List<HighScoreVersionEquivalenceEntry>();

        /// <summary>
        /// 設定内容を検証します。
        /// </summary>
        /// <returns>検出したエラーの一覧。問題がない場合は空です。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (_entries == null)
            {
                return errors;
            }

            var registeredTargets = new HashSet<(GameMode, GameDifficulty)>();
            for (int index = 0; index < _entries.Count; index++)
            {
                HighScoreVersionEquivalenceEntry entry = _entries[index];
                if (entry == null)
                {
                    errors.Add($"High score version equivalence entry at index {index} is null.");
                    continue;
                }

                if (!Enum.IsDefined(typeof(GameMode), entry.GameMode))
                {
                    errors.Add($"Entry at index {index}: GameMode is undefined: {entry.GameMode}.");
                }

                if (!Enum.IsDefined(typeof(GameDifficulty), entry.Difficulty))
                {
                    errors.Add($"Entry at index {index}: GameDifficulty is undefined: {entry.Difficulty}.");
                }

                if (!registeredTargets.Add((entry.GameMode, entry.Difficulty)))
                {
                    errors.Add(
                        $"GameMode.{entry.GameMode} and GameDifficulty.{entry.Difficulty} are duplicated.");
                }

                try
                {
                    entry.CreateVersionEquivalenceGroups();
                }
                catch (ArgumentException exception)
                {
                    errors.Add(
                        $"GameMode.{entry.GameMode} and GameDifficulty.{entry.Difficulty}: {exception.Message}");
                }
            }

            return errors;
        }

        /// <summary>
        /// 設定内容からハイスコアの互換方針を生成します。
        /// </summary>
        /// <returns>生成した互換方針。</returns>
        /// <exception cref="InvalidOperationException">設定内容が不正な場合。</exception>
        public VersionEquivalenceHighScoreCompatibilityPolicy CreateCompatibilityPolicy()
        {
            IReadOnlyList<string> errors = Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "High score version equivalence validation failed: " + string.Join(" | ", errors));
            }

            var groupsByTarget = new Dictionary<(GameMode, GameDifficulty), VersionEquivalenceGroups>();
            if (_entries != null)
            {
                for (int index = 0; index < _entries.Count; index++)
                {
                    HighScoreVersionEquivalenceEntry entry = _entries[index];
                    groupsByTarget.Add(
                        (entry.GameMode, entry.Difficulty),
                        entry.CreateVersionEquivalenceGroups());
                }
            }

            return new VersionEquivalenceHighScoreCompatibilityPolicy(groupsByTarget);
        }
    }
}
