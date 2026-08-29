using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// Sessionの最終結果を表す不変データです。
    /// </summary>
    public sealed class SessionResult
    {
        /// <summary>
        /// Sessionが終了した理由を取得します。
        /// </summary>
        public SessionTerminationReason TerminationReason { get; }

        /// <summary>
        /// 最終Sectionを取得します。
        /// </summary>
        public int FinalSection { get; }

        /// <summary>
        /// 最後に提示されたMissionを取得します。
        /// </summary>
        public Mission LastMission { get; }

        /// <summary>
        /// 最後の提出判定を取得します。
        /// </summary>
        public SubmissionResolution LastResolution { get; }

        /// <summary>
        /// 最後に提出されたExpressionを取得します。
        /// </summary>
        public Expression FinalExpression => LastResolution.SubmittedExpression;

        /// <summary>
        /// 正解数を取得します。
        /// </summary>
        public int CorrectAnswerCount => Math.Max(0, FinalSection - 1);

        /// <summary>
        /// SessionResultを生成します。
        /// </summary>
        /// <param name="terminationReason">Sessionが終了した理由。</param>
        /// <param name="finalSection">終了時のSection。</param>
        /// <param name="lastMission">最後に提示されたMission。</param>
        /// <param name="lastResolution">最後の提出判定。</param>
        /// <exception cref="ArgumentOutOfRangeException">終了理由またはSectionが不正な場合。</exception>
        /// <exception cref="ArgumentNullException">Missionまたは提出判定がnullの場合。</exception>
        /// <exception cref="ArgumentException">Missionと提出判定のMissionIdが一致しない場合。</exception>
        public SessionResult(
            SessionTerminationReason terminationReason,
            int finalSection,
            Mission lastMission,
            SubmissionResolution lastResolution)
        {
            if (!Enum.IsDefined(typeof(SessionTerminationReason), terminationReason))
            {
                throw new ArgumentOutOfRangeException(nameof(terminationReason), terminationReason, null);
            }

            if (finalSection <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(finalSection), finalSection, null);
            }

            if (lastMission == null)
            {
                throw new ArgumentNullException(nameof(lastMission));
            }

            if (lastResolution == null)
            {
                throw new ArgumentNullException(nameof(lastResolution));
            }

            if (lastMission.Id != lastResolution.MissionId)
            {
                throw new ArgumentException("Last mission must match the last resolution mission id.", nameof(lastResolution));
            }

            TerminationReason = terminationReason;
            FinalSection = finalSection;
            LastMission = lastMission;
            LastResolution = lastResolution;
        }
    }
}
