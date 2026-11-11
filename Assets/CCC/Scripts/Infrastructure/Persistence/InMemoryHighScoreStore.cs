using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;

namespace Ccc.Infrastructure.Persistence
{
    /// <summary>
    /// メモリ上だけでハイスコア記録を保持するDebugMode用の保存先です。アプリケーションを終了すると記録は消えます。
    /// </summary>
    public sealed class InMemoryHighScoreStore : IHighScoreStore
    {
        private readonly Dictionary<(GameMode, GameDifficulty), HighScoreRecord> _records =
            new Dictionary<(GameMode, GameDifficulty), HighScoreRecord>();

        /// <summary>
        /// 記録なしの状態で保存先を生成します。
        /// </summary>
        public InMemoryHighScoreStore()
        {
        }

        /// <summary>
        /// 指定した記録を初期値として持つ保存先を生成します。同じ組を複数指定した場合は後の指定を使います。
        /// </summary>
        /// <param name="initialEntries">初期値として持つ記録の設定。</param>
        /// <param name="appVersion">初期値の記録に設定するアプリケーションバージョン。</param>
        /// <exception cref="ArgumentNullException">initialEntriesがnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">スコアが負の場合。</exception>
        /// <exception cref="ArgumentException">記録があるのにappVersionがnullまたは空白の場合。</exception>
        public InMemoryHighScoreStore(IEnumerable<DebugHighScoreEntry> initialEntries, string appVersion)
        {
            if (initialEntries == null)
            {
                throw new ArgumentNullException(nameof(initialEntries));
            }

            foreach (DebugHighScoreEntry entry in initialEntries)
            {
                Save(entry.GameMode, entry.Difficulty, new HighScoreRecord(entry.Score, appVersion));
            }
        }

        /// <summary>
        /// 保持している記録を読み込みます。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="record">読み込んだ記録。記録がない場合はnull。</param>
        /// <returns>記録がある場合はtrue。</returns>
        public bool TryLoad(GameMode gameMode, GameDifficulty difficulty, out HighScoreRecord record)
        {
            return _records.TryGetValue((gameMode, difficulty), out record);
        }

        /// <summary>
        /// 記録を保持します。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="record">保持する記録。</param>
        /// <exception cref="ArgumentNullException">recordがnullの場合。</exception>
        public void Save(GameMode gameMode, GameDifficulty difficulty, HighScoreRecord record)
        {
            _records[(gameMode, difficulty)] =
                record ?? throw new ArgumentNullException(nameof(record));
        }

        /// <summary>
        /// 記録を削除します。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        public void Delete(GameMode gameMode, GameDifficulty difficulty)
        {
            _records.Remove((gameMode, difficulty));
        }
    }
}
