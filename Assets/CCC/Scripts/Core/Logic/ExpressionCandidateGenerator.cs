using System;
using System.Collections.Generic;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// 指定された数値範囲と演算子から、評価可能な全ExpressionCandidateを決定的に列挙・生成するクラス。
    /// </summary>
    public static class ExpressionCandidateGenerator
    {
        private static readonly OperatorType[] OperatorTypes = (OperatorType[])Enum.GetValues(typeof(OperatorType));

        /// <summary>
        /// minimumNumber 〜 maximumNumber の数値と全演算子の組み合わせを列挙し、0除算以外の候補を返します。
        /// 列挙順は Left 昇順 -> Right 昇順 -> OperatorType enum順 で固定されます。
        /// </summary>
        public static IReadOnlyList<ExpressionCandidate> Generate(int minimumNumber, int maximumNumber)
        {
            if (minimumNumber > maximumNumber)
            {
                throw new ArgumentException("minimumNumber cannot be greater than maximumNumber.", nameof(minimumNumber));
            }

            long numberCount = (long)maximumNumber - minimumNumber + 1;
            long totalCombinations = numberCount * numberCount * OperatorTypes.Length;
            if (totalCombinations > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumNumber), "The number of combinations exceeds the valid range.");
            }

            var candidates = new List<ExpressionCandidate>();

            for (int left = minimumNumber; left <= maximumNumber; left++)
            {
                for (int right = minimumNumber; right <= maximumNumber; right++)
                {
                    foreach (var op in OperatorTypes)
                    {
                        var expression = new Expression(left, op, right);
                        var result = ExpressionCalculator.Evaluate(expression);

                        // 0除算を除外し、正常に定義された候補のみ追加
                        if (result.IsDefined)
                        {
                            candidates.Add(new ExpressionCandidate(expression, result));
                        }
                    }
                }
            }

            return candidates.ToArray();
        }
    }
}
