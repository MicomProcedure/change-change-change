namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// アプリ外のWebページを開く操作を抽象化するインターフェースです。
    /// </summary>
    public interface IExternalPageOpener
    {
        /// <summary>
        /// プライバシーポリシーのWebページを開きます。
        /// </summary>
        void OpenPrivacyPolicy();
    }
}
