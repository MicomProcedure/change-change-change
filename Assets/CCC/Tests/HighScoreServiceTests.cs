using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// ハイスコアの参照、更新判定、バージョン互換の適用を検証します。
    /// </summary>
    public sealed class HighScoreServiceTests
    {
        private const GameMode TargetGameMode = GameMode.LifeEndless;
        private const GameDifficulty TargetDifficulty = GameDifficulty.Normal;
        private const GameDifficulty OtherDifficulty = GameDifficulty.Hard;
        private const string CurrentAppVersion = "2.0.0";
        private const string OldAppVersion = "1.0.0";

        /// <summary>
        /// 記録がない場合のハイスコアは0になることを検証します。
        /// </summary>
        [Test]
        public void GetHighScore_WhenNoRecord_ShouldReturnZero()
        {
            HighScoreService service = CreateService(new InMemoryHighScoreStore());

            int highScore = service.GetHighScore(TargetGameMode, TargetDifficulty);

            Assert.That(highScore, Is.EqualTo(0));
        }

        /// <summary>
        /// 保存済みのハイスコアを難易度ごとに区別して返すことを検証します。
        /// </summary>
        [Test]
        public void GetHighScore_WhenRecordsExist_ShouldReturnScoreOfRequestedDifficulty()
        {
            var store = new InMemoryHighScoreStore();
            store.Save(TargetGameMode, TargetDifficulty, new HighScoreRecord(5, OldAppVersion));
            store.Save(TargetGameMode, OtherDifficulty, new HighScoreRecord(9, OldAppVersion));
            HighScoreService service = CreateService(store);

            int highScore = service.GetHighScore(TargetGameMode, TargetDifficulty);

            Assert.That(highScore, Is.EqualTo(5));
        }

        /// <summary>
        /// 未定義のGameModeとGameDifficultyを拒否することを検証します。
        /// </summary>
        [Test]
        public void GetHighScore_WhenEnumIsUndefined_ShouldThrow()
        {
            HighScoreService service = CreateService(new InMemoryHighScoreStore());

            Assert.Throws<ArgumentOutOfRangeException>(
                () => service.GetHighScore((GameMode)(-1), TargetDifficulty));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => service.GetHighScore(TargetGameMode, (GameDifficulty)(-1)));
        }

        /// <summary>
        /// 記録がない状態で1以上のスコアを記録すると、現在のバージョンで保存して新記録になることを検証します。
        /// </summary>
        [Test]
        public void Record_WhenFirstPositiveScore_ShouldSaveWithCurrentVersionAsNewHighScore()
        {
            var store = new InMemoryHighScoreStore();
            HighScoreService service = CreateService(store);

            HighScoreUpdateResult result = service.Record(TargetGameMode, TargetDifficulty, 3);

            Assert.That(result.IsNewHighScore, Is.True);
            Assert.That(result.Score, Is.EqualTo(3));
            Assert.That(result.HighScore, Is.EqualTo(3));
            HighScoreRecord savedRecord = store.Get(TargetGameMode, TargetDifficulty);
            Assert.That(savedRecord.Score, Is.EqualTo(3));
            Assert.That(savedRecord.AppVersion, Is.EqualTo(CurrentAppVersion));
        }

        /// <summary>
        /// 記録がない状態でスコア0を記録しても、新記録にならず保存もしないことを検証します。
        /// </summary>
        [Test]
        public void Record_WhenFirstScoreIsZero_ShouldNotSave()
        {
            var store = new InMemoryHighScoreStore();
            HighScoreService service = CreateService(store);

            HighScoreUpdateResult result = service.Record(TargetGameMode, TargetDifficulty, 0);

            Assert.That(result.IsNewHighScore, Is.False);
            Assert.That(result.HighScore, Is.EqualTo(0));
            Assert.That(store.Get(TargetGameMode, TargetDifficulty), Is.Null);
        }

        /// <summary>
        /// 既存記録を超えると、スコアとバージョンを更新して新記録になることを検証します。
        /// </summary>
        [Test]
        public void Record_WhenScoreExceedsRecord_ShouldUpdateScoreAndVersion()
        {
            var store = new InMemoryHighScoreStore();
            store.Save(TargetGameMode, TargetDifficulty, new HighScoreRecord(5, OldAppVersion));
            HighScoreService service = CreateService(store);

            HighScoreUpdateResult result = service.Record(TargetGameMode, TargetDifficulty, 6);

            Assert.That(result.IsNewHighScore, Is.True);
            Assert.That(result.HighScore, Is.EqualTo(6));
            HighScoreRecord savedRecord = store.Get(TargetGameMode, TargetDifficulty);
            Assert.That(savedRecord.Score, Is.EqualTo(6));
            Assert.That(savedRecord.AppVersion, Is.EqualTo(CurrentAppVersion));
        }

        /// <summary>
        /// 既存記録以下のスコアでは、新記録にならず記録を変更しないことを検証します。
        /// </summary>
        /// <param name="score">記録するスコア。</param>
        [TestCase(5)]
        [TestCase(4)]
        public void Record_WhenScoreDoesNotExceedRecord_ShouldKeepRecord(int score)
        {
            var store = new InMemoryHighScoreStore();
            var existingRecord = new HighScoreRecord(5, OldAppVersion);
            store.Save(TargetGameMode, TargetDifficulty, existingRecord);
            HighScoreService service = CreateService(store);

            HighScoreUpdateResult result = service.Record(TargetGameMode, TargetDifficulty, score);

            Assert.That(result.IsNewHighScore, Is.False);
            Assert.That(result.Score, Is.EqualTo(score));
            Assert.That(result.HighScore, Is.EqualTo(5));
            Assert.That(store.Get(TargetGameMode, TargetDifficulty), Is.SameAs(existingRecord));
        }

        /// <summary>
        /// 負のスコアと未定義のenumを拒否することを検証します。
        /// </summary>
        [Test]
        public void Record_WhenArgumentIsInvalid_ShouldThrow()
        {
            HighScoreService service = CreateService(new InMemoryHighScoreStore());

            Assert.Throws<ArgumentOutOfRangeException>(
                () => service.Record(TargetGameMode, TargetDifficulty, -1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => service.Record((GameMode)(-1), TargetDifficulty, 1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => service.Record(TargetGameMode, (GameDifficulty)(-1), 1));
        }

        /// <summary>
        /// 互換方針へ対象の組み合わせ、保存済み記録、現在のバージョンを渡すことを検証します。
        /// </summary>
        [Test]
        public void GetHighScore_WhenRecordExists_ShouldPassCurrentVersionToPolicy()
        {
            var store = new InMemoryHighScoreStore();
            var existingRecord = new HighScoreRecord(5, OldAppVersion);
            store.Save(TargetGameMode, TargetDifficulty, existingRecord);
            var policy = new StubCompatibilityPolicy(HighScoreCompatibilityDecision.Keep());
            HighScoreService service = CreateService(store, policy);

            service.GetHighScore(TargetGameMode, TargetDifficulty);

            Assert.That(policy.LastGameMode, Is.EqualTo(TargetGameMode));
            Assert.That(policy.LastDifficulty, Is.EqualTo(TargetDifficulty));
            Assert.That(policy.LastRecord, Is.SameAs(existingRecord));
            Assert.That(policy.LastCurrentAppVersion, Is.EqualTo(CurrentAppVersion));
        }

        /// <summary>
        /// 互換方針がDiscardの場合、記録を削除して0を返すことを検証します。
        /// </summary>
        [Test]
        public void GetHighScore_WhenPolicyDiscards_ShouldDeleteRecordAndReturnZero()
        {
            var store = new InMemoryHighScoreStore();
            store.Save(TargetGameMode, TargetDifficulty, new HighScoreRecord(5, OldAppVersion));
            HighScoreService service = CreateService(
                store,
                new StubCompatibilityPolicy(HighScoreCompatibilityDecision.Discard()));

            int highScore = service.GetHighScore(TargetGameMode, TargetDifficulty);

            Assert.That(highScore, Is.EqualTo(0));
            Assert.That(store.Get(TargetGameMode, TargetDifficulty), Is.Null);
        }

        /// <summary>
        /// 互換方針がIgnoreの場合、記録を残したまま0を返すことを検証します。
        /// </summary>
        [Test]
        public void GetHighScore_WhenPolicyIgnores_ShouldReturnZeroAndKeepRecord()
        {
            var store = new InMemoryHighScoreStore();
            var existingRecord = new HighScoreRecord(5, OldAppVersion);
            store.Save(TargetGameMode, TargetDifficulty, existingRecord);
            HighScoreService service = CreateService(
                store,
                new StubCompatibilityPolicy(HighScoreCompatibilityDecision.Ignore()));

            int highScore = service.GetHighScore(TargetGameMode, TargetDifficulty);

            Assert.That(highScore, Is.EqualTo(0));
            Assert.That(store.Get(TargetGameMode, TargetDifficulty), Is.SameAs(existingRecord));
        }

        /// <summary>
        /// 無視対象の記録より低いスコアでも、1以上なら新記録として上書きすることを検証します。
        /// </summary>
        [Test]
        public void Record_WhenPolicyIgnoresExistingRecord_ShouldOverwriteWithPositiveScore()
        {
            var store = new InMemoryHighScoreStore();
            store.Save(TargetGameMode, TargetDifficulty, new HighScoreRecord(5, OldAppVersion));
            HighScoreService service = CreateService(
                store,
                new StubCompatibilityPolicy(HighScoreCompatibilityDecision.Ignore()));

            HighScoreUpdateResult result = service.Record(TargetGameMode, TargetDifficulty, 1);

            Assert.That(result.IsNewHighScore, Is.True);
            Assert.That(result.HighScore, Is.EqualTo(1));
            HighScoreRecord savedRecord = store.Get(TargetGameMode, TargetDifficulty);
            Assert.That(savedRecord.Score, Is.EqualTo(1));
            Assert.That(savedRecord.AppVersion, Is.EqualTo(CurrentAppVersion));
        }

        /// <summary>
        /// 無視対象の記録はスコア0では上書きせず、残したままにすることを検証します。
        /// </summary>
        [Test]
        public void Record_WhenPolicyIgnoresExistingRecordAndScoreIsZero_ShouldKeepRecord()
        {
            var store = new InMemoryHighScoreStore();
            var existingRecord = new HighScoreRecord(5, OldAppVersion);
            store.Save(TargetGameMode, TargetDifficulty, existingRecord);
            HighScoreService service = CreateService(
                store,
                new StubCompatibilityPolicy(HighScoreCompatibilityDecision.Ignore()));

            HighScoreUpdateResult result = service.Record(TargetGameMode, TargetDifficulty, 0);

            Assert.That(result.IsNewHighScore, Is.False);
            Assert.That(result.HighScore, Is.EqualTo(0));
            Assert.That(store.Get(TargetGameMode, TargetDifficulty), Is.SameAs(existingRecord));
        }

        /// <summary>
        /// 互換方針がReplaceの場合、変換後の記録を保存して返すことを検証します。
        /// </summary>
        [Test]
        public void GetHighScore_WhenPolicyReplaces_ShouldSaveAndReturnReplacement()
        {
            var store = new InMemoryHighScoreStore();
            store.Save(TargetGameMode, TargetDifficulty, new HighScoreRecord(5, OldAppVersion));
            var replacement = new HighScoreRecord(7, CurrentAppVersion);
            HighScoreService service = CreateService(
                store,
                new StubCompatibilityPolicy(HighScoreCompatibilityDecision.Replace(replacement)));

            int highScore = service.GetHighScore(TargetGameMode, TargetDifficulty);

            Assert.That(highScore, Is.EqualTo(7));
            Assert.That(store.Get(TargetGameMode, TargetDifficulty), Is.SameAs(replacement));
        }

        /// <summary>
        /// 破棄対象の記録はスコア更新の比較に使わないことを検証します。
        /// </summary>
        [Test]
        public void Record_WhenPolicyDiscardsExistingRecord_ShouldCompareWithZero()
        {
            var store = new InMemoryHighScoreStore();
            store.Save(TargetGameMode, TargetDifficulty, new HighScoreRecord(5, OldAppVersion));
            HighScoreService service = CreateService(
                store,
                new StubCompatibilityPolicy(HighScoreCompatibilityDecision.Discard()));

            HighScoreUpdateResult result = service.Record(TargetGameMode, TargetDifficulty, 1);

            Assert.That(result.IsNewHighScore, Is.True);
            Assert.That(result.HighScore, Is.EqualTo(1));
            Assert.That(store.Get(TargetGameMode, TargetDifficulty).Score, Is.EqualTo(1));
        }

        /// <summary>
        /// 初期の互換方針は全ての記録を維持することを検証します。
        /// </summary>
        [Test]
        public void KeepAllPolicy_ShouldKeepRecord()
        {
            var policy = new KeepAllHighScoreCompatibilityPolicy();

            HighScoreCompatibilityDecision decision = policy.Decide(
                TargetGameMode,
                TargetDifficulty,
                new HighScoreRecord(5, OldAppVersion),
                CurrentAppVersion);

            Assert.That(decision.Action, Is.EqualTo(HighScoreCompatibilityAction.Keep));
        }

        /// <summary>
        /// HighScoreRecordは不正なスコアとバージョンを拒否することを検証します。
        /// </summary>
        [Test]
        public void HighScoreRecord_WhenArgumentIsInvalid_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HighScoreRecord(-1, OldAppVersion));
            Assert.Throws<ArgumentException>(() => new HighScoreRecord(1, null));
            Assert.Throws<ArgumentException>(() => new HighScoreRecord(1, " "));
        }

        /// <summary>
        /// HighScoreUpdateResultはハイスコアを下回る不整合と負のスコアを拒否することを検証します。
        /// </summary>
        [Test]
        public void HighScoreUpdateResult_WhenArgumentIsInvalid_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HighScoreUpdateResult(-1, 1, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HighScoreUpdateResult(2, 1, false));
        }

        /// <summary>
        /// Replaceの変換後記録にnullを指定できないことを検証します。
        /// </summary>
        [Test]
        public void HighScoreCompatibilityDecision_Replace_WhenRecordIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => HighScoreCompatibilityDecision.Replace(null));
        }

        private static HighScoreService CreateService(IHighScoreStore store)
        {
            return CreateService(store, new KeepAllHighScoreCompatibilityPolicy());
        }

        private static HighScoreService CreateService(
            IHighScoreStore store,
            IHighScoreCompatibilityPolicy policy)
        {
            return new HighScoreService(store, policy, new StubApplicationVersionProvider());
        }

        private sealed class InMemoryHighScoreStore : IHighScoreStore
        {
            private readonly Dictionary<(GameMode, GameDifficulty), HighScoreRecord> _records =
                new Dictionary<(GameMode, GameDifficulty), HighScoreRecord>();

            public HighScoreRecord Get(GameMode gameMode, GameDifficulty difficulty)
            {
                return _records.TryGetValue((gameMode, difficulty), out HighScoreRecord record) ? record : null;
            }

            public bool TryLoad(GameMode gameMode, GameDifficulty difficulty, out HighScoreRecord record)
            {
                return _records.TryGetValue((gameMode, difficulty), out record);
            }

            public void Save(GameMode gameMode, GameDifficulty difficulty, HighScoreRecord record)
            {
                _records[(gameMode, difficulty)] = record;
            }

            public void Delete(GameMode gameMode, GameDifficulty difficulty)
            {
                _records.Remove((gameMode, difficulty));
            }
        }

        private sealed class StubCompatibilityPolicy : IHighScoreCompatibilityPolicy
        {
            private readonly HighScoreCompatibilityDecision _decision;

            public GameMode LastGameMode { get; private set; }

            public GameDifficulty LastDifficulty { get; private set; }

            public HighScoreRecord LastRecord { get; private set; }

            public string LastCurrentAppVersion { get; private set; }

            public StubCompatibilityPolicy(HighScoreCompatibilityDecision decision)
            {
                _decision = decision;
            }

            public HighScoreCompatibilityDecision Decide(
                GameMode gameMode,
                GameDifficulty difficulty,
                HighScoreRecord record,
                string currentAppVersion)
            {
                LastGameMode = gameMode;
                LastDifficulty = difficulty;
                LastRecord = record;
                LastCurrentAppVersion = currentAppVersion;
                return _decision;
            }
        }

        private sealed class StubApplicationVersionProvider : IApplicationVersionProvider
        {
            public string CurrentVersion => CurrentAppVersion;
        }
    }
}
