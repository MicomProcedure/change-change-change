namespace Ccc.Core.Model
{
    /// <summary>
    /// Gameplayで選択された難易度プリセットを表します。
    /// </summary>
    public enum GameDifficulty
    {
        /// <summary>
        /// 計算結果を表示する簡易難易度です。
        /// </summary>
        Easy = 0,

        /// <summary>
        /// プロトスプリントリーグの標準難易度です。
        /// </summary>
        Normal,

        /// <summary>
        /// 負数を含む難易度です。
        /// </summary>
        Hard
    }
}
