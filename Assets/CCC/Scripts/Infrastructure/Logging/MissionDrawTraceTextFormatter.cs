using System;
using System.Text;
using Ccc.Core.Model;
using Ccc.Core.UseCases;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// Mission抽選Traceを、段階別の選定結果と順位上位候補を並べた複数行テキストへ整形します。
    /// </summary>
    public sealed class MissionDrawTraceTextFormatter : IDiagnosticsTextFormatter<MissionDrawTrace>
    {
        private const string PoolMarker = " [Pool]";
        private const string SelectedMarker = " [Selected]";

        private readonly int _candidateCount;

        /// <summary>
        /// Mission抽選診断設定から整形器を生成します。
        /// </summary>
        /// <param name="options">Log出力に含める候補数を持つMission抽選診断設定。</param>
        /// <exception cref="ArgumentNullException">設定がnullの場合。</exception>
        public MissionDrawTraceTextFormatter(MissionDrawDiagnosticsOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _candidateCount = options.LogCandidateCount;
        }

        /// <summary>
        /// Mission抽選Traceを複数行テキストへ整形します。
        /// </summary>
        /// <param name="trace">整形する抽選Trace。</param>
        /// <returns>見出し、段階別の選定結果、順位上位候補を含むテキスト。</returns>
        /// <exception cref="ArgumentNullException">Traceがnullの場合。</exception>
        public string Format(MissionDrawTrace trace)
        {
            if (trace == null)
            {
                throw new ArgumentNullException(nameof(trace));
            }

            var text = new StringBuilder();
            text.Append("Mission draw: Section=").Append(trace.Section)
                .Append(", CurrentTier=").Append(trace.CurrentTier)
                .Append(", IsInitialDraw=").Append(trace.IsInitialDraw)
                .AppendLine();
            text.Append("Stages: Enabled=").Append(trace.EnabledCandidateCount)
                .Append(", Unlocked=").Append(trace.UnlockedCandidateCount)
                .Append(", Expression=").Append(trace.SelectedExpressionGroup)
                .Append('(').Append(trace.ExpressionStageCandidateCount)
                .Append("; Satisfied=").Append(trace.SatisfiedCandidateCount)
                .Append(", Unmatched=").Append(trace.UnmatchedCandidateCount).Append(')')
                .Append(", Tier=").Append(trace.SelectedTier)
                .Append('(').Append(trace.TierStageCandidateCount).Append(')')
                .Append(", Polarity=").Append(MissionDrawTraceValueFormatter.FormatPolarity(
                    trace.IsNegatedPolaritySelected))
                .Append('(').Append(trace.PolarityStageCandidateCount).Append(')')
                .Append(", RepeatFiltered=").Append(trace.RepeatFilteredCandidateCount)
                .Append(", Kind=").Append(trace.SelectedKind)
                .Append('(').Append(trace.RankedCandidates.Count).Append(')')
                .AppendLine();
            text.Append("Selection: SelectedRank=").Append(trace.SelectedRank)
                .Append(", PoolSize=").Append(trace.PoolSize)
                .Append(", TargetRatio=").Append(MissionDrawTraceValueFormatter.FormatRatio(
                    trace.Config.TargetExpressionMatchRatio));

            int displayedCount = Math.Min(_candidateCount, trace.RankedCandidates.Count);
            for (int index = 0; index < displayedCount; index++)
            {
                AppendCandidateLine(text, trace, index + 1, trace.RankedCandidates[index]);
            }

            return text.ToString();
        }

        private static void AppendCandidateLine(
            StringBuilder text,
            MissionDrawTrace trace,
            int rank,
            MissionCandidate candidate)
        {
            MissionCondition condition = candidate.Condition;
            text.AppendLine()
                .Append('#').Append(rank)
                .Append(" Tier=").Append(candidate.Tier)
                .Append(" Kind=").Append(condition.Kind)
                .Append(" ParameterA=").Append(condition.ParameterA)
                .Append(" ParameterB=").Append(condition.ParameterB)
                .Append(" Negated=").Append(condition.IsNegated)
                .Append(" Match=").Append(candidate.ExpressionMatchCount)
                .Append('/').Append(candidate.EvaluatedExpressionCount)
                .Append(" Ratio=").Append(MissionDrawTraceValueFormatter.FormatRatio(
                    candidate.ExpressionMatchRatio))
                .Append(" Difference=").Append(MissionDrawTraceValueFormatter.FormatRatio(
                    MissionDrawTraceValueFormatter.CalculateTargetDifference(trace, candidate)));

            if (MissionDrawTraceValueFormatter.IsInPool(trace, rank))
            {
                text.Append(PoolMarker);
            }

            if (MissionDrawTraceValueFormatter.IsSelected(trace, candidate))
            {
                text.Append(SelectedMarker);
            }
        }
    }
}
