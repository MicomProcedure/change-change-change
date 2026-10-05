using System;
using System.Globalization;
using Ccc.Core.Model;
using Ccc.Core.UseCases;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// Mission抽選Traceのテキスト整形とCSV整形で共有する値の算出と文字列化を提供します。
    /// </summary>
    internal static class MissionDrawTraceValueFormatter
    {
        private const string RatioFormat = "0.0000";
        private const string PositivePolarityName = "Positive";
        private const string NegatedPolarityName = "Negated";

        /// <summary>
        /// 候補の適合率と目標適合率の差の絶対値を算出します。
        /// </summary>
        /// <param name="trace">目標適合率を含む抽選Trace。</param>
        /// <param name="candidate">差を算出する候補。</param>
        /// <returns>適合率と目標適合率の差の絶対値。</returns>
        internal static float CalculateTargetDifference(MissionDrawTrace trace, MissionCandidate candidate)
        {
            return Math.Abs(candidate.ExpressionMatchRatio - trace.Config.TargetExpressionMatchRatio);
        }

        /// <summary>
        /// 比率を固定小数点の文字列へ変換します。
        /// </summary>
        /// <param name="ratio">変換する比率。</param>
        /// <returns>カルチャに依存しない比率文字列。</returns>
        internal static string FormatRatio(float ratio)
        {
            return ratio.ToString(RatioFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 極性段階の選択結果を名前へ変換します。
        /// </summary>
        /// <param name="isNegated">Negated候補群が選ばれたかどうか。</param>
        /// <returns>極性名。</returns>
        internal static string FormatPolarity(bool isNegated)
        {
            return isNegated ? NegatedPolarityName : PositivePolarityName;
        }

        /// <summary>
        /// 1始まりの順位がPool内かどうかを判定します。
        /// </summary>
        /// <param name="trace">PoolSizeを含む抽選Trace。</param>
        /// <param name="rank">1始まりの順位。</param>
        /// <returns>順位がPool内であればtrue。</returns>
        internal static bool IsInPool(MissionDrawTrace trace, int rank)
        {
            return rank <= trace.PoolSize;
        }

        /// <summary>
        /// 候補が選出候補かどうかを判定します。
        /// </summary>
        /// <param name="trace">選出候補を含む抽選Trace。</param>
        /// <param name="candidate">判定する候補。</param>
        /// <returns>選出候補であればtrue。</returns>
        internal static bool IsSelected(MissionDrawTrace trace, MissionCandidate candidate)
        {
            return ReferenceEquals(candidate, trace.SelectedCandidate);
        }
    }
}
