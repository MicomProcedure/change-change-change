using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// ハイスコアの参照と更新を、バージョン互換方針を適用して行います。
    /// </summary>
    public sealed class HighScoreService
    {
        private const int NoRecordHighScore = 0;

        private readonly IHighScoreStore _store;
        private readonly IHighScoreCompatibilityPolicy _compatibilityPolicy;
        private readonly IApplicationVersionProvider _versionProvider;

        /// <summary>
        /// HighScoreServiceを生成します。
        /// </summary>
        /// <param name="store">ハイスコアの保存先。</param>
        /// <param name="compatibilityPolicy">保存済み記録のバージョン互換方針。</param>
        /// <param name="versionProvider">実行中のアプリケーションバージョンの提供元。</param>
        /// <exception cref="ArgumentNullException">依存のいずれかがnullの場合。</exception>
        public HighScoreService(
            IHighScoreStore store,
            IHighScoreCompatibilityPolicy compatibilityPolicy,
            IApplicationVersionProvider versionProvider)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _compatibilityPolicy =
                compatibilityPolicy ?? throw new ArgumentNullException(nameof(compatibilityPolicy));
            _versionProvider = versionProvider ?? throw new ArgumentNullException(nameof(versionProvider));
        }

        /// <summary>
        /// 指定した組み合わせのハイスコアを取得します。記録がない場合は0を返します。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <returns>ハイスコア。</returns>
        /// <exception cref="ArgumentOutOfRangeException">enumが未定義の場合。</exception>
        public int GetHighScore(GameMode gameMode, GameDifficulty difficulty)
        {
            ValidateTarget(gameMode, difficulty);
            return LoadCompatibleHighScore(gameMode, difficulty);
        }

        /// <summary>
        /// スコアを記録します。既存のハイスコアを超えた場合のみ、実行中のバージョンとともに保存します。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="score">記録するスコア。</param>
        /// <returns>記録した結果。</returns>
        /// <exception cref="ArgumentOutOfRangeException">enumが未定義、またはscoreが負の場合。</exception>
        public HighScoreUpdateResult Record(GameMode gameMode, GameDifficulty difficulty, int score)
        {
            ValidateTarget(gameMode, difficulty);
            if (score < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score), score, null);
            }

            int currentHighScore = LoadCompatibleHighScore(gameMode, difficulty);
            if (score <= currentHighScore)
            {
                return new HighScoreUpdateResult(score, currentHighScore, false);
            }

            _store.Save(gameMode, difficulty, new HighScoreRecord(score, _versionProvider.CurrentVersion));
            return new HighScoreUpdateResult(score, score, true);
        }

        private int LoadCompatibleHighScore(GameMode gameMode, GameDifficulty difficulty)
        {
            if (!_store.TryLoad(gameMode, difficulty, out HighScoreRecord record))
            {
                return NoRecordHighScore;
            }

            HighScoreCompatibilityDecision decision = _compatibilityPolicy.Decide(
                gameMode,
                difficulty,
                record,
                _versionProvider.CurrentVersion);

            switch (decision.Action)
            {
                case HighScoreCompatibilityAction.Keep:
                    return record.Score;
                case HighScoreCompatibilityAction.Discard:
                    _store.Delete(gameMode, difficulty);
                    return NoRecordHighScore;
                case HighScoreCompatibilityAction.Replace:
                    _store.Save(gameMode, difficulty, decision.ReplacementRecord);
                    return decision.ReplacementRecord.Score;
                case HighScoreCompatibilityAction.Ignore:
                    return NoRecordHighScore;
                default:
                    throw new ArgumentOutOfRangeException(nameof(decision), decision.Action, null);
            }
        }

        private static void ValidateTarget(GameMode gameMode, GameDifficulty difficulty)
        {
            if (!Enum.IsDefined(typeof(GameMode), gameMode))
            {
                throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, null);
            }

            if (!Enum.IsDefined(typeof(GameDifficulty), difficulty))
            {
                throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null);
            }
        }
    }
}
