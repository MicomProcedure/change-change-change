using Ccc.Core.Model;
using Ccc.Core.UseCases;
using UnityEngine;

namespace Ccc.Infrastructure.Persistence
{
    /// <summary>
    /// PlayerPrefsを使用してハイスコア記録を保存します。
    /// </summary>
    public sealed class PlayerPrefsHighScoreStore : IHighScoreStore
    {
        private const string KeyPrefix = "HighScore";

        /// <summary>
        /// 保存済みの記録を読み込みます。内容が不正な場合は警告を出し、記録なしとして扱います。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="record">読み込んだ記録。有効な記録がない場合はnull。</param>
        /// <returns>有効な記録がある場合はtrue。</returns>
        public bool TryLoad(GameMode gameMode, GameDifficulty difficulty, out HighScoreRecord record)
        {
            record = null;
            string key = CreateKey(gameMode, difficulty);
            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }

            if (HighScoreRecordJsonCodec.TryDecode(PlayerPrefs.GetString(key), out record))
            {
                return true;
            }

            Debug.LogWarning(
                $"[{nameof(PlayerPrefsHighScoreStore)}] {key}の保存値が不正なため、記録なしとして扱います。");
            return false;
        }

        /// <summary>
        /// 記録を保存します。保存に失敗した場合はエラーログを出し、例外を送出しません。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        /// <param name="record">保存する記録。</param>
        /// <exception cref="System.ArgumentNullException">recordがnullの場合。</exception>
        public void Save(GameMode gameMode, GameDifficulty difficulty, HighScoreRecord record)
        {
            string json = HighScoreRecordJsonCodec.Encode(record);
            string key = CreateKey(gameMode, difficulty);
            try
            {
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
            }
            catch (PlayerPrefsException exception)
            {
                Debug.LogError(
                    $"[{nameof(PlayerPrefsHighScoreStore)}] {key}の保存に失敗しました: {exception.Message}");
            }
        }

        /// <summary>
        /// 記録を削除します。
        /// </summary>
        /// <param name="gameMode">対象のゲームモード。</param>
        /// <param name="difficulty">対象のゲーム難易度。</param>
        public void Delete(GameMode gameMode, GameDifficulty difficulty)
        {
            PlayerPrefs.DeleteKey(CreateKey(gameMode, difficulty));
            PlayerPrefs.Save();
        }

        private static string CreateKey(GameMode gameMode, GameDifficulty difficulty)
        {
            return $"{KeyPrefix}.{gameMode}.{difficulty}";
        }
    }
}
