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
        /// Result画面の表示状態を生成します。
        /// </summary>
        /// <param name="resultText">Result画面に表示する本文。</param>
        /// <exception cref="ArgumentNullException">表示本文がnullの場合。</exception>
        public GameplayResultViewStateDto(string resultText)
        {
            ResultText = resultText ?? throw new ArgumentNullException(nameof(resultText));
        }
    }
}
