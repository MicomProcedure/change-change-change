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
