using System;
using Ccc.Core.Model;
using Ccc.Infrastructure.Persistence;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// DebugMode用にメモリ上だけで記録を保持するハイスコア保存先を検証します。
    /// </summary>
    public sealed class InMemoryHighScoreStoreTests
    {
        private const GameMode TargetGameMode = GameMode.LifeEndless;
        private const GameDifficulty TargetDifficulty = GameDifficulty.Easy;
        private const GameDifficulty OtherDifficulty = GameDifficulty.Hard;
        private const string AppVersion = "1.0.1";

        /// <summary>
        /// 保存前は記録がないことを検証します。
        /// </summary>
        [Test]
        public void TryLoad_WhenNotSaved_ShouldReturnFalse()
        {
            var store = new InMemoryHighScoreStore();

            bool isLoaded = store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord record);

            Assert.That(isLoaded, Is.False);
            Assert.That(record, Is.Null);
        }

        /// <summary>
        /// 保存した記録を読み込めることを検証します。
        /// </summary>
        [Test]
        public void TryLoad_WhenSaved_ShouldReturnSavedRecord()
        {
            var store = new InMemoryHighScoreStore();
            var savedRecord = new HighScoreRecord(5, AppVersion);
            store.Save(TargetGameMode, TargetDifficulty, savedRecord);

            bool isLoaded = store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord record);

            Assert.That(isLoaded, Is.True);
            Assert.That(record, Is.SameAs(savedRecord));
        }

        /// <summary>
        /// 記録を難易度ごとに区別することを検証します。
        /// </summary>
        [Test]
        public void TryLoad_WhenOtherDifficultyIsSaved_ShouldReturnFalse()
        {
            var store = new InMemoryHighScoreStore();
            store.Save(TargetGameMode, OtherDifficulty, new HighScoreRecord(5, AppVersion));

            bool isLoaded = store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord _);

            Assert.That(isLoaded, Is.False);
        }

        /// <summary>
        /// 同じ組への保存は記録を上書きすることを検証します。
        /// </summary>
        [Test]
        public void Save_WhenAlreadySaved_ShouldOverwriteRecord()
        {
            var store = new InMemoryHighScoreStore();
            var latestRecord = new HighScoreRecord(6, AppVersion);
            store.Save(TargetGameMode, TargetDifficulty, new HighScoreRecord(5, AppVersion));

            store.Save(TargetGameMode, TargetDifficulty, latestRecord);

            store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord record);
            Assert.That(record, Is.SameAs(latestRecord));
        }

        /// <summary>
        /// 保存する記録にnullを指定できないことを検証します。
        /// </summary>
        [Test]
        public void Save_WhenRecordIsNull_ShouldThrow()
        {
            var store = new InMemoryHighScoreStore();

            Assert.Throws<ArgumentNullException>(
                () => store.Save(TargetGameMode, TargetDifficulty, null));
        }

        /// <summary>
        /// 削除すると対象の組の記録だけがなくなることを検証します。
        /// </summary>
        [Test]
        public void Delete_ShouldRemoveOnlyTargetRecord()
        {
            var store = new InMemoryHighScoreStore();
            store.Save(TargetGameMode, TargetDifficulty, new HighScoreRecord(5, AppVersion));
            store.Save(TargetGameMode, OtherDifficulty, new HighScoreRecord(6, AppVersion));

            store.Delete(TargetGameMode, TargetDifficulty);

            Assert.That(store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord _), Is.False);
            Assert.That(store.TryLoad(TargetGameMode, OtherDifficulty, out HighScoreRecord _), Is.True);
        }

        /// <summary>
        /// 初期記録を、指定したスコアとバージョンで読み込めることを検証します。
        /// </summary>
        [Test]
        public void TryLoad_WhenInitialEntryIsSpecified_ShouldReturnRecordWithSpecifiedVersion()
        {
            var store = new InMemoryHighScoreStore(
                new[] { new DebugHighScoreEntry(TargetGameMode, TargetDifficulty, 10) },
                AppVersion);

            bool isLoaded = store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord record);

            Assert.That(isLoaded, Is.True);
            Assert.That(record.Score, Is.EqualTo(10));
            Assert.That(record.AppVersion, Is.EqualTo(AppVersion));
        }

        /// <summary>
        /// 初期記録を難易度ごとに保持することを検証します。
        /// </summary>
        [Test]
        public void TryLoad_WhenInitialEntriesHaveMultipleDifficulties_ShouldReturnEachScore()
        {
            var store = new InMemoryHighScoreStore(
                new[]
                {
                    new DebugHighScoreEntry(TargetGameMode, TargetDifficulty, 10),
                    new DebugHighScoreEntry(TargetGameMode, OtherDifficulty, 20)
                },
                AppVersion);

            store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord targetRecord);
            store.TryLoad(TargetGameMode, OtherDifficulty, out HighScoreRecord otherRecord);

            Assert.That(targetRecord.Score, Is.EqualTo(10));
            Assert.That(otherRecord.Score, Is.EqualTo(20));
        }

        /// <summary>
        /// 初期記録に指定のない組は記録なしで始まることを検証します。
        /// </summary>
        [Test]
        public void TryLoad_WhenInitialEntryIsNotSpecifiedForDifficulty_ShouldReturnFalse()
        {
            var store = new InMemoryHighScoreStore(
                new[] { new DebugHighScoreEntry(TargetGameMode, OtherDifficulty, 10) },
                AppVersion);

            bool isLoaded = store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord _);

            Assert.That(isLoaded, Is.False);
        }

        /// <summary>
        /// 初期記録が空の場合は記録なしで始まることを検証します。
        /// </summary>
        [Test]
        public void TryLoad_WhenInitialEntriesAreEmpty_ShouldReturnFalse()
        {
            var store = new InMemoryHighScoreStore(new DebugHighScoreEntry[0], AppVersion);

            bool isLoaded = store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord _);

            Assert.That(isLoaded, Is.False);
        }

        /// <summary>
        /// 初期記録で同じ組を複数指定した場合は後の指定を使うことを検証します。
        /// </summary>
        [Test]
        public void Constructor_WhenSamePairIsSpecifiedTwice_ShouldUseLastEntry()
        {
            var store = new InMemoryHighScoreStore(
                new[]
                {
                    new DebugHighScoreEntry(TargetGameMode, TargetDifficulty, 10),
                    new DebugHighScoreEntry(TargetGameMode, TargetDifficulty, 20)
                },
                AppVersion);

            store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord record);

            Assert.That(record.Score, Is.EqualTo(20));
        }

        /// <summary>
        /// 初期記録にnullを指定できないことを検証します。
        /// </summary>
        [Test]
        public void Constructor_WhenInitialEntriesAreNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(
                () => new InMemoryHighScoreStore(null, AppVersion));
        }

        /// <summary>
        /// 初期記録に負のスコアを指定できないことを検証します。
        /// </summary>
        [Test]
        public void Constructor_WhenInitialScoreIsNegative_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new InMemoryHighScoreStore(
                    new[] { new DebugHighScoreEntry(TargetGameMode, TargetDifficulty, -1) },
                    AppVersion));
        }

        /// <summary>
        /// 初期記録を保存で上書きできることを検証します。
        /// </summary>
        [Test]
        public void Save_WhenInitialEntryExists_ShouldOverwriteRecord()
        {
            var store = new InMemoryHighScoreStore(
                new[] { new DebugHighScoreEntry(TargetGameMode, TargetDifficulty, 10) },
                AppVersion);
            var latestRecord = new HighScoreRecord(11, AppVersion);

            store.Save(TargetGameMode, TargetDifficulty, latestRecord);

            store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord record);
            Assert.That(record, Is.SameAs(latestRecord));
        }
    }
}
