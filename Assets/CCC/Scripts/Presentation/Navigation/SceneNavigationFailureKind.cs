namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// Scene遷移の失敗種別を表します。
    /// </summary>
    public enum SceneNavigationFailureKind
    {
        /// <summary>
        /// 失敗していません。
        /// </summary>
        None = 0,

        /// <summary>
        /// すでに別のSceneをロード中です。
        /// </summary>
        AlreadyLoading,

        /// <summary>
        /// Sceneロードに失敗しました。
        /// </summary>
        LoadFailed
    }
}