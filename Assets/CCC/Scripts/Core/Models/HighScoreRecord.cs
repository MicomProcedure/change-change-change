using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// 保存対象のハイスコアと、達成時のアプリケーションバージョンを表す不変データです。
    /// </summary>
    public sealed class HighScoreRecord
    {
        /// <summary>
        /// ハイスコアを取得します。
        /// </summary>
        public int Score { get; }

        /// <summary>
        /// ハイスコアを達成したアプリケーションバージョンを取得します。
        /// </summary>
        public string AppVersion { get; }

        /// <summary>
        /// HighScoreRecordを生成します。
        /// </summary>
        /// <param name="score">ハイスコア。</param>
        /// <param name="appVersion">ハイスコアを達成したアプリケーションバージョン。</param>
        /// <exception cref="ArgumentOutOfRangeException">scoreが負の場合。</exception>
        /// <exception cref="ArgumentException">appVersionがnullまたは空白の場合。</exception>
        public HighScoreRecord(int score, string appVersion)
        {
            if (score < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score), score, null);
            }

            if (string.IsNullOrWhiteSpace(appVersion))
            {
                throw new ArgumentException("App version must not be blank.", nameof(appVersion));
            }

            Score = score;
            AppVersion = appVersion;
        }
    }
}
