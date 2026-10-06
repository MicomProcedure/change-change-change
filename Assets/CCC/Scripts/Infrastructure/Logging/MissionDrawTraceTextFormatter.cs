using System;
using System.Globalization;
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
        private const string RatioFormat = "0.0000";
        private const string PositivePolarityName = "Positive";
        private const string NegatedPolarityName = "Negated";

        private readonly int _candidateCount;

        /// <summary>
        /// Log出力に含める順位上位の候補数から整形器を生成します。
        /// </summary>
        /// <param name="candidateCount">Log出力に含める順位上位の候補数。</param>
        /// <exception cref="ArgumentOutOfRangeException">候補数が負の場合。</exception>
        public MissionDrawTraceTextFormatter(int candidateCount)
        {
            if (candidateCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidateCount),
                    candidateCount,
                    "Candidate count must not be negative.");
            }

            _candidateCount = candidateCount;
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
                .Append(", Polarity=").Append(FormatPolarity(trace.IsNegatedPolaritySelected))
                .Append('(').Append(trace.PolarityStageCandidateCount).Append(')')
                .Append(", RepeatFiltered=").Append(trace.RepeatFilteredCandidateCount)
                .Append(", Kind=").Append(trace.SelectedKind)
                .Append('(').Append(trace.RankedCandidates.Count).Append(')')
                .AppendLine();
            text.Append("Selection: SelectedRank=").Append(trace.SelectedRank)
                .Append(", PoolSize=").Append(trace.PoolSize)
                .Append(", TargetRatio=").Append(FormatRatio(trace.Config.TargetExpressionMatchRatio));

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
                .Append(" Ratio=").Append(FormatRatio(candidate.ExpressionMatchRatio))
                .Append(" Difference=").Append(FormatRatio(
                    Math.Abs(candidate.ExpressionMatchRatio - trace.Config.TargetExpressionMatchRatio)));

            if (rank <= trace.PoolSize)
            {
                text.Append(PoolMarker);
            }

            if (ReferenceEquals(candidate, trace.SelectedCandidate))
            {
                text.Append(SelectedMarker);
            }
        }

        private static string FormatRatio(float ratio)
        {
            return ratio.ToString(RatioFormat, CultureInfo.InvariantCulture);
        }

        private static string FormatPolarity(bool isNegated)
        {
            return isNegated ? NegatedPolarityName : PositivePolarityName;
        }
    }
}
