namespace Ccc.Presentation
{
    /// <summary>
    /// Gameplay開始前のCountdown表示と入力状態を扱うView契約です。
    /// </summary>
    public interface IGameplayCountdownView
    {
        /// <summary>
        /// 残り秒数をCountdownとして表示します。
        /// </summary>
        /// <param name="remainingSeconds">表示する残り秒数。</param>
        void ShowCountdown(int remainingSeconds);

        /// <summary>
        /// Countdown表示を非表示にします。
        /// </summary>
        void HideCountdown();

        /// <summary>
        /// Gameplay入力の受付状態を切り替えます。
        /// </summary>
        /// <param name="isEnabled">入力を受け付ける場合はtrue。</param>
        void SetInputEnabled(bool isEnabled);
    }
}
