using System;

namespace Ccc.Core.Model
{
    public readonly struct Expression : IEquatable<Expression>
    {
        public int Left { get; }
        public OperatorType Operator { get; }
        public int Right { get; }

        public Expression(int left, OperatorType @operator, int right)
        {
            if (!Enum.IsDefined(typeof(OperatorType), @operator))
            {
                throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
            }

            Left = left;
            Operator = @operator;
            Right = right;
        }

        public bool Equals(Expression other)
        {
            return Left == other.Left && Operator == other.Operator && Right == other.Right;
        }

        public override bool Equals(object obj)
        {
            return obj is Expression other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Left;
                hashCode = (hashCode * 397) ^ (int)Operator;
                hashCode = (hashCode * 397) ^ Right;
                return hashCode;
            }
        }

        public static bool operator ==(Expression left, Expression right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Expression left, Expression right)
        {
            return !(left == right);
        }
    }
}
