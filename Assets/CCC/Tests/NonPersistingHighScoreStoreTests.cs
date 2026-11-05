using System;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Persistence;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// DebugMode用に永続化を行わないハイスコア保存先の振る舞いを検証します。
    /// </summary>
    public sealed class NonPersistingHighScoreStoreTests
    {
        private const GameMode TargetGameMode = GameMode.LifeEndless;
        private const GameDifficulty TargetDifficulty = GameDifficulty.Easy;
        private const string AppVersion = "1.0.1";

        /// <summary>
        /// 読み込みは内側の保存先へ委譲することを検証します。
        /// </summary>
        [Test]
        public void TryLoad_ShouldReadFromInnerStore()
        {
            var innerRecord = new HighScoreRecord(5, AppVersion);
            var innerStore = new RecordingStore { LoadedRecord = innerRecord };
            var store = new NonPersistingHighScoreStore(innerStore);

            bool isLoaded = store.TryLoad(TargetGameMode, TargetDifficulty, out HighScoreRecord record);

            Assert.That(isLoaded, Is.True);
            Assert.That(record, Is.SameAs(innerRecord));
        }

        /// <summary>
        /// 保存は内側の保存先へ書き込まないことを検証します。
        /// </summary>
        [Test]
        public void Save_ShouldNotWriteToInnerStore()
        {
            var innerStore = new RecordingStore();
            var store = new NonPersistingHighScoreStore(innerStore);

            store.Save(TargetGameMode, TargetDifficulty, new HighScoreRecord(5, AppVersion));

            Assert.That(innerStore.WriteCount, Is.EqualTo(0));
        }

        /// <summary>
        /// 削除は内側の保存先へ書き込まないことを検証します。
        /// </summary>
        [Test]
        public void Delete_ShouldNotWriteToInnerStore()
        {
            var innerStore = new RecordingStore();
            var store = new NonPersistingHighScoreStore(innerStore);

            store.Delete(TargetGameMode, TargetDifficulty);

            Assert.That(innerStore.WriteCount, Is.EqualTo(0));
        }

        /// <summary>
        /// 内側の保存先にnullを指定できないことを検証します。
        /// </summary>
        [Test]
        public void Constructor_WhenInnerStoreIsNull_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new NonPersistingHighScoreStore(null));
        }

        private sealed class RecordingStore : IHighScoreStore
        {
            public HighScoreRecord LoadedRecord { get; set; }

            public int WriteCount { get; private set; }

            public bool TryLoad(GameMode gameMode, GameDifficulty difficulty, out HighScoreRecord record)
            {
                record = LoadedRecord;
                return record != null;
            }

            public void Save(GameMode gameMode, GameDifficulty difficulty, HighScoreRecord record)
            {
                WriteCount++;
            }

            public void Delete(GameMode gameMode, GameDifficulty difficulty)
            {
                WriteCount++;
            }
        }
    }
}
