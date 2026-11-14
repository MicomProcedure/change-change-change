using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// GameModeとGameDifficultyの組ごとにハイスコア記録を永続化します。
    /// </summary>
    public interface IHighScoreStore
    {
        /// <summary>
        /// 保存済みの記録を読み込みます。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="record">読み込んだ記録。有効な記録がない場合はnull。</param>
        /// <returns>有効な記録がある場合はtrue。</returns>
        bool TryLoad(GameMode gameMode, GameDifficulty difficulty, out HighScoreRecord record);

        /// <summary>
        /// 記録を保存します。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="record">保存する記録。</param>
        void Save(GameMode gameMode, GameDifficulty difficulty, HighScoreRecord record);

        /// <summary>
        /// 記録を削除します。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        void Delete(GameMode gameMode, GameDifficulty difficulty);
    }
}
