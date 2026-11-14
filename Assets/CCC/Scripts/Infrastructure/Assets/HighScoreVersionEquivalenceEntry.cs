using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// ゲームモードと難易度の組に対する、ハイスコアを同一視するバージョングループの設定です。
    /// </summary>
    [Serializable]
    public sealed class HighScoreVersionEquivalenceEntry
    {
        [SerializeField] private GameMode _gameMode;
        [SerializeField] private GameDifficulty _difficulty;
        [Tooltip("同じグループのバージョンで達成したハイスコアを同一視する。")]
        [SerializeField] private List<VersionGroup> _versionGroups = new List<VersionGroup>();

        /// <summary>
        /// 対象のゲームモードを取得します。
        /// </summary>
        public GameMode GameMode
        {
            get { return _gameMode; }
        }

        /// <summary>
        /// 対象のゲーム難易度を取得します。
        /// </summary>
        public GameDifficulty Difficulty
        {
            get { return _difficulty; }
        }

        /// <summary>
        /// 設定内容から純粋なバージョングループを生成します。
        /// </summary>
        /// <returns>生成したバージョングループ。</returns>
        /// <exception cref="ArgumentException">グループの内容が不正な場合。</exception>
        public VersionEquivalenceGroups CreateVersionEquivalenceGroups()
        {
            var groups = new List<IReadOnlyCollection<string>>();
            if (_versionGroups != null)
            {
                for (int index = 0; index < _versionGroups.Count; index++)
                {
                    groups.Add(_versionGroups[index]?.Versions);
                }
            }

            return new VersionEquivalenceGroups(groups);
        }
    }
}
