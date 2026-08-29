using System;
using Ccc.Core.Model;

namespace Ccc.Core.Logic
{
    /// <summary>
    /// Expression を評価し CalculationResult を返す同期・決定的な計算ロジック。
    /// </summary>
    public static class ExpressionCalculator
    {
        public static CalculationResult Evaluate(Expression expression)
        {
            var left = Rational.Create(expression.Left, 1);
            var right = Rational.Create(expression.Right, 1);

            switch (expression.Operator)
            {
                case OperatorType.Add:
                    return CalculationResult.Defined(RationalMath.Add(left, right));

                case OperatorType.Subtract:
                    return CalculationResult.Defined(RationalMath.Subtract(left, right));

                case OperatorType.Multiply:
                    return CalculationResult.Defined(RationalMath.Multiply(left, right));

                case OperatorType.Divide:
                    if (expression.Right == 0)
                    {
                        return CalculationResult.DivisionByZero();
                    }
                    return CalculationResult.Defined(RationalMath.Divide(left, right));

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(expression),
                        $"Undefined OperatorType: {expression.Operator}");
            }
        }
    }
}
