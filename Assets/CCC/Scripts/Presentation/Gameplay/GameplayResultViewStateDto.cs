using System;

namespace Ccc.Presentation
{
    /// <summary>
    /// Result画面の描画に必要な変換済み表示状態です。
    /// </summary>
    public sealed class GameplayResultViewStateDto
    {
        /// <summary>
        /// Result画面に表示する本文を取得します。
        /// </summary>
        public string ResultText { get; }

        /// <summary>
        /// 記録後のハイスコアを表示用の文字列で取得します。
        /// </summary>
        public string HighScoreText { get; }

        /// <summary>
        /// 今回のSessionでハイスコアを更新したかどうかを取得します。
        /// </summary>
        public bool IsNewHighScore { get; }

        /// <summary>
        /// Result画面の表示状態を生成します。
        /// </summary>
        /// <param name="resultText">Result画面に表示する本文。</param>
        /// <param name="highScoreText">記録後のハイスコアの表示文字列。</param>
        /// <param name="isNewHighScore">今回のSessionでハイスコアを更新したかどうか。</param>
        /// <exception cref="ArgumentNullException">表示文字列がnullの場合。</exception>
        public GameplayResultViewStateDto(
            string resultText,
            string highScoreText,
            bool isNewHighScore)
        {
            ResultText = resultText ?? throw new ArgumentNullException(nameof(resultText));
            HighScoreText = highScoreText ?? throw new ArgumentNullException(nameof(highScoreText));
            IsNewHighScore = isNewHighScore;
        }
    }
}
