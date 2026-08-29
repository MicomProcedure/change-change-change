using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// 式とその評価結果のペアを表す不変値オブジェクト。
    /// </summary>
    public readonly struct ExpressionCandidate : IEquatable<ExpressionCandidate>
    {
        public Expression Expression { get; }
        public CalculationResult Result { get; }

        public ExpressionCandidate(Expression expression, CalculationResult result)
        {
            Expression = expression;
            Result = result;
        }

        public bool Equals(ExpressionCandidate other)
        {
            return Expression.Equals(other.Expression) && Result.Equals(other.Result);
        }

        public override bool Equals(object? obj) => obj is ExpressionCandidate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Expression, Result);
        public static bool operator ==(ExpressionCandidate left, ExpressionCandidate right) => left.Equals(right);
        public static bool operator !=(ExpressionCandidate left, ExpressionCandidate right) => !left.Equals(right);
    }
}
