using System;
using Ccc.Core.Model;
using UnityEngine;

namespace Ccc.Infrastructure.Persistence
{
    /// <summary>
    /// DebugModeの起動時に保存先へ入れる、ゲームモードと難易度の組に対するハイスコアの設定です。
    /// </summary>
    [Serializable]
    public sealed class DebugHighScoreEntry
    {
        private const float MinScore = 0f;

        [SerializeField] private GameMode _gameMode;
        [SerializeField] private GameDifficulty _difficulty;
        [Min(MinScore)]
        [SerializeField] private int _score;

        /// <summary>
        /// 対象のゲームモードを取得します。
        /// </summary>
        public GameMode GameMode
        {
            get { return _gameMode; }
        }

        /// <summary>
        /// 対象のゲーム難易度を取得します。
        /// </summary>
        public GameDifficulty Difficulty
        {
            get { return _difficulty; }
        }

        /// <summary>
        /// 保存先へ入れるハイスコアを取得します。
        /// </summary>
        public int Score
        {
            get { return _score; }
        }

        /// <summary>
        /// Unityのシリアライズ用に、既定値でDebugHighScoreEntryを生成します。
        /// </summary>
        public DebugHighScoreEntry()
        {
        }

        /// <summary>
        /// 指定した内容でDebugHighScoreEntryを生成します。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="score">保存先へ入れるハイスコア。</param>
        public DebugHighScoreEntry(GameMode gameMode, GameDifficulty difficulty, int score)
        {
            _gameMode = gameMode;
            _difficulty = difficulty;
            _score = score;
        }
    }
}
