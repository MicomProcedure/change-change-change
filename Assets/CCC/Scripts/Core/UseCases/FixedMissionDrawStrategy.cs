using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 開発確認用に常に同じMission候補を返すStrategyです。
    /// </summary>
    public sealed class FixedMissionDrawStrategy : IMissionDrawStrategy
    {
        private readonly MissionCandidate _candidate;

        /// <summary>
        /// 固定するMission候補からStrategyを生成します。
        /// </summary>
        /// <param name="candidate">固定する候補。</param>
        /// <exception cref="ArgumentNullException">候補がnullの場合。</exception>
        public FixedMissionDrawStrategy(MissionCandidate candidate)
        {
            _candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
        }

        /// <summary>
        /// Sectionに関係なく固定候補を返します。
        /// </summary>
        /// <param name="context">抽選Context。</param>
        /// <returns>固定候補の成功Result。</returns>
        public MissionDrawResult Draw(MissionDrawContext context)
        {
            return context == null
                ? MissionDrawResult.Failure("Mission draw context is null.")
                : MissionDrawResult.Success(_candidate);
        }
    }
}
