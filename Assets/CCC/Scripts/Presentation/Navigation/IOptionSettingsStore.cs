namespace Ccc.Presentation.Navigation
{
    /// <summary>
    /// Option設定の読み込みと保存を行います。
    /// </summary>
    public interface IOptionSettingsStore
    {
        /// <summary>
        /// 保存されているOption設定を読み込みます。保存値がない場合は既定値を返します。
        /// </summary>
        /// <returns>読み込んだOption設定。</returns>
        OptionSettingsSnapshot Load();

        /// <summary>
        /// Option設定を保存します。
        /// </summary>
        /// <param name="snapshot">保存するOption設定。</param>
        void Save(OptionSettingsSnapshot snapshot);
    }
}