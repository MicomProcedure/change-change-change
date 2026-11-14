using System.Collections.Generic;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 式候補の計算結果から、到達可能な整数の集合を抽出します。
    /// </summary>
    internal static class ReachableIntegerCollector
    {
        /// <summary>
        /// 正常に計算できる式候補のうち、計算結果がint範囲の整数であるものを集めます。
        /// </summary>
        /// <param name="expressions">対象の式候補。</param>
        /// <returns>到達可能な整数の集合。</returns>
        internal static HashSet<int> Collect(IReadOnlyList<ExpressionCandidate> expressions)
        {
            var integers = new HashSet<int>();
            for (int index = 0; index < expressions.Count; index++)
            {
                CalculationResult result = expressions[index].Result;
                if (!result.IsDefined || !result.Value.IsInteger)
                {
                    continue;
                }

                long numerator = result.Value.Numerator;
                if (numerator >= int.MinValue && numerator <= int.MaxValue)
                {
                    integers.Add((int)numerator);
                }
            }

            return integers;
        }
    }
}
