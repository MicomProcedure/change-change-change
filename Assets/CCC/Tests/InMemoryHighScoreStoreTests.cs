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
    }
}
