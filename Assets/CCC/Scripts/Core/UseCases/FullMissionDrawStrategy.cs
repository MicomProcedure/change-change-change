#nullable enable
using System;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// 現在式による候補群の確率選択、Tier、極性、直前Mission、Kind、適合率を順に考慮し、
    /// 設定の目標式適合割合に近い候補から選ぶ本実装版Mission抽選Strategyです。
    /// </summary>
    public sealed class FullMissionDrawStrategy : IMissionDrawStrategy
    {
        private readonly MissionDrawPipeline _pipeline;

        /// <summary>
        /// 乱数提供元と診断Reporterから本実装版Strategyを生成します。
        /// </summary>
        /// <param name="randomSource">0以上1未満の乱数提供元。</param>
        /// <param name="diagnosticsReporter">抽選成功時のTraceを受け取る診断Reporter。</param>
        /// <exception cref="ArgumentNullException">乱数提供元または診断Reporterがnullの場合。</exception>
        public FullMissionDrawStrategy(
            IRandomSource randomSource,
            IDiagnosticsReporter<MissionDrawTrace> diagnosticsReporter)
        {
            _pipeline = new MissionDrawPipeline(
                randomSource,
                new FixedTargetExpressionMatchRatioRangeResolver(),
                diagnosticsReporter);
        }

        /// <summary>
        /// 抽選Contextに基づいて本実装版の規則でMission候補を1件選出します。
        /// </summary>
        /// <param name="context">抽選Context。</param>
        /// <returns>抽選結果。</returns>
        public MissionDrawResult Draw(MissionDrawContext context)
        {
            return _pipeline.Draw(context);
        }
    }
}
