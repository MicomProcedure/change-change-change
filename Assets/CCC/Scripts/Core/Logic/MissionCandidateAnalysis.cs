#nullable enable
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 1候補を正常なExpressionCandidate全件に対して一度だけ分析した結果です。
    /// </summary>
    internal readonly struct MissionCandidateAnalysis
    {
        public int MatchCount { get; }
        public int NonMatchCount { get; }
        public int EvaluatedExpressionCount { get; }
        public Rational MatchRatio { get; }
        public ExpressionCandidate? FirstMatch { get; }

        public MissionCandidateAnalysis(
            int matchCount,
            int nonMatchCount,
            int evaluatedExpressionCount,
            Rational matchRatio,
            ExpressionCandidate? firstMatch)
        {
            MatchCount = matchCount;
            NonMatchCount = nonMatchCount;
            EvaluatedExpressionCount = evaluatedExpressionCount;
            MatchRatio = matchRatio;
            FirstMatch = firstMatch;
        }
    }
}
