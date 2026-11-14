using System;
using Ccc.Core.Model;
using UnityEngine;

namespace Ccc.Infrastructure.Persistence
{
    /// <summary>
    /// ハイスコア記録と保存用JSONを相互に変換します。
    /// </summary>
    public static class HighScoreRecordJsonCodec
    {
        /// <summary>
        /// 現在の保存形式の版数です。保存形式を変更するときに増やします。
        /// </summary>
        public const int CurrentFormatVersion = 1;

        /// <summary>
        /// 記録を保存用JSONへ変換します。
        /// </summary>
        /// <param name="record">変換する記録。</param>
        /// <returns>保存用JSON。</returns>
        /// <exception cref="ArgumentNullException">recordがnullの場合。</exception>
        public static string Encode(HighScoreRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            return JsonUtility.ToJson(new HighScoreRecordDto
            {
                formatVersion = CurrentFormatVersion,
                score = record.Score,
                appVersion = record.AppVersion
            });
        }

        /// <summary>
        /// 保存用JSONから記録を復元します。
        /// </summary>
        /// <param name="json">保存用JSON。</param>
        /// <param name="record">復元した記録。復元できない場合はnull。</param>
        /// <returns>有効な記録を復元できた場合はtrue。</returns>
        public static bool TryDecode(string json, out HighScoreRecord record)
        {
            record = null;
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            HighScoreRecordDto dto;
            try
            {
                dto = JsonUtility.FromJson<HighScoreRecordDto>(json);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (dto == null ||
                dto.formatVersion != CurrentFormatVersion ||
                dto.score < 0 ||
                string.IsNullOrWhiteSpace(dto.appVersion))
            {
                return false;
            }

            record = new HighScoreRecord(dto.score, dto.appVersion);
            return true;
        }

        // JsonUtilityはフィールド名をそのままJSONのキーにするため、保存形式の一部としてこの名前を固定する
        [Serializable]
        private sealed class HighScoreRecordDto
        {
            public int formatVersion;
            public int score;
            public string appVersion;
        }
    }
}
