#nullable enable
using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Mission抽選Strategyへ渡す不変のContextです。
    /// </summary>
    public sealed class MissionDrawContext
    {
        /// <summary>
        /// 抽選対象のセッションコンテキストを取得します。
        /// </summary>
        public GameplaySessionContext SessionContext { get; }

        /// <summary>
        /// 抽選対象のSectionを取得します。
        /// </summary>
        public int Section { get; }

        /// <summary>
        /// 初回Missionの抽選Contextであるかを取得します。
        /// </summary>
        public bool IsInitialDraw => PreviousMission == null;

        /// <summary>
        /// 次回抽選時に引き継がれた現在式を取得します。初回抽選時はnullです。
        /// </summary>
        public Expression? CurrentExpression { get; }

        /// <summary>
        /// 直前に出題されたMissionを取得します。初回抽選時はnullです。
        /// </summary>
        public Mission? PreviousMission { get; }

        /// <summary>
        /// セッションコンテキストと履歴状態から抽選Contextを生成します。
        /// </summary>
        /// <param name="sessionContext">セッションコンテキスト。</param>
        /// <param name="section">抽選対象のSection。</param>
        /// <param name="currentExpression">現在式。初回抽選時はnull。</param>
        /// <param name="previousMission">直前Mission。初回抽選時はnull。</param>
        /// <exception cref="ArgumentNullException">Contextがnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Sectionが1未満の場合。</exception>
        private MissionDrawContext(
            GameplaySessionContext sessionContext,
            int section,
            Expression? currentExpression,
            Mission? previousMission)
        {
            SessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, "Section must be greater than zero.");
            }

            Section = section;
            CurrentExpression = currentExpression;
            PreviousMission = previousMission;
        }

        /// <summary>
        /// 直前Mission情報を持たない初回Mission用の抽選Contextを生成します。
        /// </summary>
        /// <param name="sessionContext">セッションコンテキスト。</param>
        /// <param name="section">抽選対象のSection。</param>
        /// <returns>初回Mission用の抽選Context。</returns>
        /// <exception cref="ArgumentNullException">セッションコンテキストがnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Sectionが1未満の場合。</exception>
        public static MissionDrawContext CreateForInitialMission(
            GameplaySessionContext sessionContext,
            int section)
        {
            return new MissionDrawContext(sessionContext, section, null, null);
        }

        /// <summary>
        /// 引き継いだ現在式と直前Missionを持つ次回Mission用の抽選Contextを生成します。
        /// </summary>
        /// <param name="sessionContext">セッションコンテキスト。</param>
        /// <param name="section">抽選対象のSection。</param>
        /// <param name="currentExpression">直前回答から引き継いだ現在式。</param>
        /// <param name="previousMission">直前に出題されたMission。</param>
        /// <returns>次回Mission用の抽選Context。</returns>
        /// <exception cref="ArgumentNullException">セッションコンテキストまたは直前Missionがnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Sectionが1未満の場合。</exception>
        public static MissionDrawContext CreateForNextMission(
            GameplaySessionContext sessionContext,
            int section,
            Expression currentExpression,
            Mission previousMission)
        {
            if (previousMission == null)
            {
                throw new ArgumentNullException(nameof(previousMission));
            }

            return new MissionDrawContext(
                sessionContext,
                section,
                currentExpression,
                previousMission);
        }
    }
}
