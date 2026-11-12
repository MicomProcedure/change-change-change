namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 実行中のアプリケーションバージョンを提供します。
    /// </summary>
    public interface IApplicationVersionProvider
    {
        /// <summary>
        /// 実行中のアプリケーションバージョンを取得します。
        /// </summary>
        string CurrentVersion { get; }
    }
}
