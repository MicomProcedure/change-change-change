using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 保存済みのハイスコア記録に対する扱いの決定内容を表す不変データです。
    /// </summary>
    public sealed class HighScoreCompatibilityDecision
    {
        /// <summary>
        /// 記録に対する扱いを取得します。
        /// </summary>
        public HighScoreCompatibilityAction Action { get; }

        /// <summary>
        /// Replaceの場合の変換後の記録を取得します。それ以外の場合はnullです。
        /// </summary>
        public HighScoreRecord ReplacementRecord { get; }

        private HighScoreCompatibilityDecision(
            HighScoreCompatibilityAction action,
            HighScoreRecord replacementRecord)
        {
            Action = action;
            ReplacementRecord = replacementRecord;
        }

        /// <summary>
        /// 記録をそのまま使い続ける決定を生成します。
        /// </summary>
        /// <returns>Keepの決定。</returns>
        public static HighScoreCompatibilityDecision Keep()
        {
            return new HighScoreCompatibilityDecision(HighScoreCompatibilityAction.Keep, null);
        }

        /// <summary>
        /// 記録を破棄する決定を生成します。
        /// </summary>
        /// <returns>Discardの決定。</returns>
        public static HighScoreCompatibilityDecision Discard()
        {
            return new HighScoreCompatibilityDecision(HighScoreCompatibilityAction.Discard, null);
        }

        /// <summary>
        /// 記録を残したまま、記録なしとして扱う決定を生成します。
        /// </summary>
        /// <returns>Ignoreの決定。</returns>
        public static HighScoreCompatibilityDecision Ignore()
        {
            return new HighScoreCompatibilityDecision(HighScoreCompatibilityAction.Ignore, null);
        }

        /// <summary>
        /// 記録を書き換える決定を生成します。
        /// </summary>
        /// <param name="replacementRecord">変換後の記録。</param>
        /// <returns>Replaceの決定。</returns>
        /// <exception cref="ArgumentNullException">replacementRecordがnullの場合。</exception>
        public static HighScoreCompatibilityDecision Replace(HighScoreRecord replacementRecord)
        {
            if (replacementRecord == null)
            {
                throw new ArgumentNullException(nameof(replacementRecord));
            }

            return new HighScoreCompatibilityDecision(HighScoreCompatibilityAction.Replace, replacementRecord);
        }
    }
}
