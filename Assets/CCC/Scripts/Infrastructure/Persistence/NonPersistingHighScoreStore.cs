using System;
using Ccc.Core.Model;
using Ccc.Core.UseCases;

namespace Ccc.Infrastructure.Persistence
{
    /// <summary>
    /// 読み込みだけを内側の保存先へ委譲し、書き込みを永続化しないDebugMode用のハイスコア保存先です。
    /// </summary>
    public sealed class NonPersistingHighScoreStore : IHighScoreStore
    {
        private readonly IHighScoreStore _innerStore;

        /// <summary>
        /// NonPersistingHighScoreStoreを生成します。
        /// </summary>
        /// <param name="innerStore">読み込みの委譲先。</param>
        /// <exception cref="ArgumentNullException">innerStoreがnullの場合。</exception>
        public NonPersistingHighScoreStore(IHighScoreStore innerStore)
        {
            _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
        }

        /// <summary>
        /// 内側の保存先から記録を読み込みます。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="record">読み込んだ記録。有効な記録がない場合はnull。</param>
        /// <returns>有効な記録がある場合はtrue。</returns>
        public bool TryLoad(GameMode gameMode, GameDifficulty difficulty, out HighScoreRecord record)
        {
            return _innerStore.TryLoad(gameMode, difficulty, out record);
        }

        /// <summary>
        /// 何も保存しません。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="record">保存しない記録。</param>
        public void Save(GameMode gameMode, GameDifficulty difficulty, HighScoreRecord record)
        {
        }

        /// <summary>
        /// 何も削除しません。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        public void Delete(GameMode gameMode, GameDifficulty difficulty)
        {
        }
    }
}
