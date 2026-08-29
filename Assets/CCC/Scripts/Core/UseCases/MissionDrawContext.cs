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
        /// セッションコンテキストとSectionから抽選Contextを生成します。
        /// </summary>
        /// <param name="sessionContext">セッションコンテキスト。</param>
        /// <param name="section">抽選対象のSection。</param>
        /// <exception cref="ArgumentNullException">Contextがnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Sectionが1未満の場合。</exception>
        public MissionDrawContext(GameplaySessionContext sessionContext, int section)
        {
            SessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, "Section must be greater than zero.");
            }

            Section = section;
        }
    }
}
