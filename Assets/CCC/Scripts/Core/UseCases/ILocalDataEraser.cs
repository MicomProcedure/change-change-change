namespace Ccc.Core.UseCases
{
    /// <summary>
    /// ローカルに永続化した情報を消去します。
    /// </summary>
    public interface ILocalDataEraser
    {
        /// <summary>
        /// ローカルに永続化した情報を全て消去します。
        /// </summary>
        void EraseAll();
    }
}
