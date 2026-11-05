using System;
using System.Collections.Generic;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// ゲームモードと難易度の組ごとのバージョングループに基づき、保存済みのハイスコア記録を同一視する互換方針です。
    /// 同一視しない記録は消さずに無視します。
    /// </summary>
    public sealed class VersionEquivalenceHighScoreCompatibilityPolicy : IHighScoreCompatibilityPolicy
    {
        private static readonly VersionEquivalenceGroups NoGroups =
            new VersionEquivalenceGroups(Array.Empty<IReadOnlyCollection<string>>());

        private readonly Dictionary<(GameMode, GameDifficulty), VersionEquivalenceGroups> _groupsByTarget;

        /// <summary>
        /// VersionEquivalenceHighScoreCompatibilityPolicyを生成します。
        /// </summary>
        /// <param name="groupsByTarget">ゲームモードと難易度の組ごとのバージョングループ。</param>
        /// <exception cref="ArgumentNullException">groupsByTargetがnullの場合。</exception>
        /// <exception cref="ArgumentException">バージョングループがnullの場合。</exception>
        public VersionEquivalenceHighScoreCompatibilityPolicy(
            IReadOnlyDictionary<(GameMode, GameDifficulty), VersionEquivalenceGroups> groupsByTarget)
        {
            if (groupsByTarget == null)
            {
                throw new ArgumentNullException(nameof(groupsByTarget));
            }

            _groupsByTarget = new Dictionary<(GameMode, GameDifficulty), VersionEquivalenceGroups>();
            foreach (KeyValuePair<(GameMode, GameDifficulty), VersionEquivalenceGroups> entry in groupsByTarget)
            {
                _groupsByTarget.Add(
                    entry.Key,
                    entry.Value ?? throw new ArgumentException(
                        $"Version groups of {entry.Key} must not be null.",
                        nameof(groupsByTarget)));
            }
        }

        /// <summary>
        /// 保存済みの記録に対する扱いを決定します。同一視する場合はKeep、しない場合はIgnoreを返します。
        /// 設定がない組み合わせは、同じバージョンの記録だけを同一視します。
        /// </summary>
        /// <param name="gameMode">記録のゲームモード。</param>
        /// <param name="difficulty">記録のゲーム難易度。</param>
        /// <param name="record">保存済みの記録。</param>
        /// <param name="currentAppVersion">実行中のアプリケーションバージョン。</param>
        /// <returns>記録の扱い。</returns>
        /// <exception cref="ArgumentNullException">recordがnullの場合。</exception>
        public HighScoreCompatibilityDecision Decide(
            GameMode gameMode,
            GameDifficulty difficulty,
            HighScoreRecord record,
            string currentAppVersion)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return GetGroups(gameMode, difficulty).AreEquivalent(record.AppVersion, currentAppVersion)
                ? HighScoreCompatibilityDecision.Keep()
                : HighScoreCompatibilityDecision.Ignore();
        }

        /// <summary>
        /// 指定したバージョンが未登録のゲームモードと難易度の組を全て取得します。
        /// </summary>
        /// <param name="version">確認するバージョン。</param>
        /// <returns>未登録の組の一覧。</returns>
        public IReadOnlyList<(GameMode GameMode, GameDifficulty Difficulty)> FindUnregisteredTargets(string version)
        {
            var unregisteredTargets = new List<(GameMode, GameDifficulty)>();
            foreach (GameMode gameMode in Enum.GetValues(typeof(GameMode)))
            {
                foreach (GameDifficulty difficulty in Enum.GetValues(typeof(GameDifficulty)))
                {
                    if (!GetGroups(gameMode, difficulty).Contains(version))
                    {
                        unregisteredTargets.Add((gameMode, difficulty));
                    }
                }
            }

            return unregisteredTargets;
        }

        private VersionEquivalenceGroups GetGroups(GameMode gameMode, GameDifficulty difficulty)
        {
            return _groupsByTarget.TryGetValue((gameMode, difficulty), out VersionEquivalenceGroups groups)
                ? groups
                : NoGroups;
        }
    }
}
