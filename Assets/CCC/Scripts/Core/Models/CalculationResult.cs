using System;

namespace Ccc.Core.Model
{
    public enum CalculationResultKind
    {
        Uninitialized = 0,
        Defined,
        DivisionByZero
    }

    public readonly struct CalculationResult : IEquatable<CalculationResult>
    {
        private readonly Rational _value;

        public CalculationResultKind Kind { get; }
        public bool IsDefined => Kind == CalculationResultKind.Defined;

        public Rational Value
        {
            get
            {
                if (Kind != CalculationResultKind.Defined)
                {
                    throw new InvalidOperationException($"CalculationResult kind '{Kind}' does not provide a defined value.");
                }

                return _value;
            }
        }

        private CalculationResult(CalculationResultKind kind, Rational value)
        {
            if (!Enum.IsDefined(typeof(CalculationResultKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            Kind = kind;
            _value = value;
        }

        public static CalculationResult Defined(Rational value)
        {
            return new CalculationResult(CalculationResultKind.Defined, value);
        }

        public static CalculationResult DivisionByZero()
        {
            return new CalculationResult(CalculationResultKind.DivisionByZero, default);
        }

        public bool Equals(CalculationResult other)
        {
            return Kind == other.Kind && ValueEquals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is CalculationResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Kind * 397) ^ (IsDefined ? _value.GetHashCode() : 0);
            }
        }

        public static bool operator ==(CalculationResult left, CalculationResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CalculationResult left, CalculationResult right)
        {
            return !(left == right);
        }

        private bool ValueEquals(CalculationResult other)
        {
            if (Kind != CalculationResultKind.Defined || other.Kind != CalculationResultKind.Defined)
            {
                return Kind == other.Kind;
            }

            return _value.Equals(other._value);
        }
    }
}
