using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// バージョンの同一視グループと、それに基づくハイスコア互換方針を検証します。
    /// </summary>
    public sealed class VersionEquivalenceTests
    {
        private const GameMode TargetGameMode = GameMode.LifeEndless;
        private const GameDifficulty TargetDifficulty = GameDifficulty.Hard;
        private const GameDifficulty OtherDifficulty = GameDifficulty.Easy;
        private const string FirstVersion = "1.0.1";
        private const string SecondVersion = "1.0.2";
        private const string ThirdVersion = "1.1.0";
        private const string UnregisteredVersion = "9.9.9";

        /// <summary>
        /// 同じグループのバージョンは順序によらず同一視することを検証します。
        /// </summary>
        [Test]
        public void AreEquivalent_WhenVersionsAreInSameGroup_ShouldReturnTrue()
        {
            VersionEquivalenceGroups groups = CreateGroups(new[] { FirstVersion, SecondVersion });

            Assert.That(groups.AreEquivalent(FirstVersion, SecondVersion), Is.True);
            Assert.That(groups.AreEquivalent(SecondVersion, FirstVersion), Is.True);
        }

        /// <summary>
        /// 別のグループのバージョンは同一視しないことを検証します。
        /// </summary>
        [Test]
        public void AreEquivalent_WhenVersionsAreInDifferentGroups_ShouldReturnFalse()
        {
            VersionEquivalenceGroups groups = CreateGroups(new[] { FirstVersion }, new[] { ThirdVersion });

            Assert.That(groups.AreEquivalent(FirstVersion, ThirdVersion), Is.False);
        }

        /// <summary>
        /// 未登録のバージョンは、同じバージョンとだけ同一視することを検証します。
        /// </summary>
        [Test]
        public void AreEquivalent_WhenVersionIsUnregistered_ShouldMatchOnlySameVersion()
        {
            VersionEquivalenceGroups groups = CreateGroups(new[] { FirstVersion, SecondVersion });

            Assert.That(groups.AreEquivalent(UnregisteredVersion, UnregisteredVersion), Is.True);
            Assert.That(groups.AreEquivalent(UnregisteredVersion, FirstVersion), Is.False);
        }

        /// <summary>
        /// 登録済みのバージョンだけをContainsで判定できることを検証します。
        /// </summary>
        [Test]
        public void Contains_ShouldReturnWhetherVersionIsRegistered()
        {
            VersionEquivalenceGroups groups = CreateGroups(new[] { FirstVersion });

            Assert.That(groups.Contains(FirstVersion), Is.True);
            Assert.That(groups.Contains(UnregisteredVersion), Is.False);
        }

        /// <summary>
        /// 空白のバージョン、複数箇所に登録されたバージョン、nullのグループを拒否することを検証します。
        /// </summary>
        [Test]
        public void Constructor_WhenGroupsAreInvalid_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => CreateGroups(new[] { " " }));
            Assert.Throws<ArgumentException>(() => CreateGroups(new string[] { null }));
            Assert.Throws<ArgumentException>(() => CreateGroups(new[] { FirstVersion }, new[] { FirstVersion }));
            Assert.Throws<ArgumentException>(() => CreateGroups(new[] { FirstVersion, FirstVersion }));
            Assert.Throws<ArgumentException>(() => CreateGroups((string[])null));
            Assert.Throws<ArgumentNullException>(() => new VersionEquivalenceGroups(null));
        }

        /// <summary>
        /// 記録のバージョンが実行中のバージョンと同一視される場合はKeepになることを検証します。
        /// </summary>
        [Test]
        public void Decide_WhenRecordVersionIsEquivalent_ShouldKeep()
        {
            VersionEquivalenceHighScoreCompatibilityPolicy policy = CreatePolicy(
                CreateGroups(new[] { FirstVersion, SecondVersion }));

            HighScoreCompatibilityDecision decision = policy.Decide(
                TargetGameMode,
                TargetDifficulty,
                new HighScoreRecord(5, FirstVersion),
                SecondVersion);

            Assert.That(decision.Action, Is.EqualTo(HighScoreCompatibilityAction.Keep));
        }

        /// <summary>
        /// 同一視されない場合は記録を消さずIgnoreになることを検証します。
        /// </summary>
        [Test]
        public void Decide_WhenRecordVersionIsNotEquivalent_ShouldIgnore()
        {
            VersionEquivalenceHighScoreCompatibilityPolicy policy = CreatePolicy(
                CreateGroups(new[] { FirstVersion }, new[] { ThirdVersion }));

            HighScoreCompatibilityDecision decision = policy.Decide(
                TargetGameMode,
                TargetDifficulty,
                new HighScoreRecord(5, FirstVersion),
                ThirdVersion);

            Assert.That(decision.Action, Is.EqualTo(HighScoreCompatibilityAction.Ignore));
        }

        /// <summary>
        /// 同一視の設定は難易度ごとに独立していることを検証します。
        /// </summary>
        [Test]
        public void Decide_ShouldUseGroupsOfRecordDifficulty()
        {
            var policy = new VersionEquivalenceHighScoreCompatibilityPolicy(
                new Dictionary<(GameMode, GameDifficulty), VersionEquivalenceGroups>
                {
                    [(TargetGameMode, TargetDifficulty)] = CreateGroups(new[] { FirstVersion }, new[] { SecondVersion }),
                    [(TargetGameMode, OtherDifficulty)] = CreateGroups(new[] { FirstVersion, SecondVersion })
                });
            var record = new HighScoreRecord(5, FirstVersion);

            Assert.That(
                policy.Decide(TargetGameMode, TargetDifficulty, record, SecondVersion).Action,
                Is.EqualTo(HighScoreCompatibilityAction.Ignore));
            Assert.That(
                policy.Decide(TargetGameMode, OtherDifficulty, record, SecondVersion).Action,
                Is.EqualTo(HighScoreCompatibilityAction.Keep));
        }

        /// <summary>
        /// 設定がない組み合わせは、同じバージョンの記録だけをKeepすることを検証します。
        /// </summary>
        [Test]
        public void Decide_WhenTargetHasNoGroups_ShouldMatchOnlySameVersion()
        {
            var policy = new VersionEquivalenceHighScoreCompatibilityPolicy(
                new Dictionary<(GameMode, GameDifficulty), VersionEquivalenceGroups>());
            var record = new HighScoreRecord(5, FirstVersion);

            Assert.That(
                policy.Decide(TargetGameMode, TargetDifficulty, record, FirstVersion).Action,
                Is.EqualTo(HighScoreCompatibilityAction.Keep));
            Assert.That(
                policy.Decide(TargetGameMode, TargetDifficulty, record, SecondVersion).Action,
                Is.EqualTo(HighScoreCompatibilityAction.Ignore));
        }

        /// <summary>
        /// 指定バージョンが未登録の組み合わせを全て列挙することを検証します。
        /// </summary>
        [Test]
        public void FindUnregisteredTargets_ShouldReturnAllTargetsWithoutVersion()
        {
            var policy = new VersionEquivalenceHighScoreCompatibilityPolicy(
                new Dictionary<(GameMode, GameDifficulty), VersionEquivalenceGroups>
                {
                    [(GameMode.LifeEndless, GameDifficulty.Easy)] = CreateGroups(new[] { FirstVersion }),
                    [(GameMode.LifeEndless, GameDifficulty.Normal)] = CreateGroups(new[] { SecondVersion })
                });

            IReadOnlyList<(GameMode GameMode, GameDifficulty Difficulty)> unregisteredTargets =
                policy.FindUnregisteredTargets(FirstVersion);

            Assert.That(unregisteredTargets, Is.EquivalentTo(new[]
            {
                (GameMode.LifeEndless, GameDifficulty.Normal),
                (GameMode.LifeEndless, GameDifficulty.Hard)
            }));
        }

        /// <summary>
        /// 互換方針の設定にnullを指定できないことを検証します。
        /// </summary>
        [Test]
        public void PolicyConstructor_WhenArgumentIsInvalid_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(
                () => new VersionEquivalenceHighScoreCompatibilityPolicy(null));
            Assert.Throws<ArgumentException>(
                () => new VersionEquivalenceHighScoreCompatibilityPolicy(
                    new Dictionary<(GameMode, GameDifficulty), VersionEquivalenceGroups>
                    {
                        [(TargetGameMode, TargetDifficulty)] = null
                    }));
        }

        private static VersionEquivalenceGroups CreateGroups(params string[][] groups)
        {
            return new VersionEquivalenceGroups(groups);
        }

        private static VersionEquivalenceHighScoreCompatibilityPolicy CreatePolicy(VersionEquivalenceGroups groups)
        {
            return new VersionEquivalenceHighScoreCompatibilityPolicy(
                new Dictionary<(GameMode, GameDifficulty), VersionEquivalenceGroups>
                {
                    [(TargetGameMode, TargetDifficulty)] = groups
                });
        }
    }
}
