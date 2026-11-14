#nullable enable
using System;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 本実装版と同じ段階で候補を絞り込み、Sectionが進むほど下限が下がる範囲から抽選ごとに選んだ目標式適合割合に近い候補から選ぶMission抽選Strategyです。
    /// </summary>
    public sealed class SectionDecliningMatchRatioMissionDrawStrategy : IMissionDrawStrategy
    {
        private readonly MissionDrawPipeline _pipeline;

        /// <summary>
        /// 乱数提供元と診断ReporterからSection低下版Strategyを生成します。
        /// </summary>
        /// <param name="randomSource">0以上1未満の乱数提供元。</param>
        /// <param name="diagnosticsReporter">抽選成功時のTraceを受け取る診断Reporter。</param>
        /// <exception cref="ArgumentNullException">乱数提供元または診断Reporterがnullの場合。</exception>
        public SectionDecliningMatchRatioMissionDrawStrategy(
            IRandomSource randomSource,
            IDiagnosticsReporter<MissionDrawTrace> diagnosticsReporter)
        {
            _pipeline = new MissionDrawPipeline(
                randomSource,
                new SectionDecliningTargetExpressionMatchRatioRangeResolver(),
                diagnosticsReporter);
        }

        /// <summary>
        /// 抽選Contextに基づいて、Sectionに応じた範囲から目標式適合割合を選んでMission候補を1件選出します。
        /// </summary>
        /// <param name="context">抽選Context。</param>
        /// <returns>抽選結果。</returns>
        public MissionDrawResult Draw(MissionDrawContext context)
        {
            return _pipeline.Draw(context);
        }
    }
}
