using System;

namespace Ccc.Core.Model
{
    public enum CalculationResultKind
    {
        Uninitialized = 0,
        Defined,
        DivisionByZero
    }

    /// <summary>
    /// 式の計算結果（正常値・0除算・未初期化）を表現する不変値オブジェクト。
    /// </summary>
    public readonly struct CalculationResult : IEquatable<CalculationResult>
    {
        private readonly Rational _value;

        public CalculationResultKind Kind { get; }
        public bool IsDefined => Kind == CalculationResultKind.Defined;

        /// <summary>
        /// 計算結果の Rational 値。
        /// Kind != Defined の場合は InvalidOperationException がスローされます。
        /// </summary>
        public Rational Value
        {
            get
            {
                if (Kind != CalculationResultKind.Defined)
                {
                    throw new InvalidOperationException($"Cannot access Value when Kind is {Kind}.");
                }
                return _value;
            }
        }

        private CalculationResult(CalculationResultKind kind, Rational value)
        {
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
            if (Kind != other.Kind)
            {
                return false;
            }

            if (Kind == CalculationResultKind.Defined)
            {
                return _value.Equals(other._value);
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is CalculationResult other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Kind == CalculationResultKind.Defined ? _value : default);
        }

        public override string ToString()
        {
            return Kind == CalculationResultKind.Defined ? _value.ToString() : Kind.ToString();
        }

        public static bool operator ==(CalculationResult left, CalculationResult right) => left.Equals(right);
        public static bool operator !=(CalculationResult left, CalculationResult right) => !left.Equals(right);
    }
}
