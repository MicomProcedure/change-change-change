namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Mission抽選の現在式段階で選ばれた候補群を表します。
    /// </summary>
    public enum CurrentExpressionCandidateGroup
    {
        /// <summary>
        /// 初回抽選のため現在式で分割せず、解放済み候補全体を使用しました。
        /// </summary>
        AllUnlocked,

        /// <summary>
        /// 現在式で達成できる候補群を選びました。
        /// </summary>
        Satisfied,

        /// <summary>
        /// 現在式で達成できない候補群を選びました。
        /// </summary>
        Unmatched
    }
}
