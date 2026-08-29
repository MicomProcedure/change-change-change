using System;

namespace Ccc.Core.Model
{
    public readonly struct MissionCondition : IEquatable<MissionCondition>
    {
        public MissionKind Kind { get; }
        public int ParameterA { get; }
        public int ParameterB { get; }
        public bool IsNegated { get; }

        private MissionCondition(MissionKind kind, int parameterA, int parameterB, bool isNegated)
        {
            if (!Enum.IsDefined(typeof(MissionKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            Kind = kind;
            ParameterA = parameterA;
            ParameterB = parameterB;
            IsNegated = isNegated;
        }

        public static MissionCondition Create(MissionKind kind)
        {
            if (!IsNoParameterKind(kind))
            {
                if (Enum.IsDefined(typeof(MissionKind), kind))
                {
                    throw new ArgumentException("This MissionKind does not accept a parameterless factory.", nameof(kind));
                }

                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            return new MissionCondition(kind, 0, 0, false);
        }

        public static MissionCondition Create(MissionKind kind, int parameter)
        {
            if (!IsSingleParameterKind(kind))
            {
                if (Enum.IsDefined(typeof(MissionKind), kind))
                {
                    throw new ArgumentException("This MissionKind does not accept a single parameter.", nameof(kind));
                }

                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            switch (kind)
            {
                case MissionKind.Multiple:
                    if (parameter < 2)
                    {
                        throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
                    }
                    break;
                case MissionKind.Absolute:
                    if (parameter < 1)
                    {
                        throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
                    }
                    break;
                case MissionKind.OnesDigit:
                case MissionKind.DecimalFirstDigit:
                    if (parameter < 0 || parameter > 9)
                    {
                        throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
                    }
                    break;
                case MissionKind.AtLeast:
                case MissionKind.AtMost:
                case MissionKind.Exact:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            return new MissionCondition(kind, parameter, 0, false);
        }

        public static MissionCondition Between(int lowerBound, int width)
        {
            if (width < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, null);
            }

            checked
            {
                var upperBound = lowerBound + width;
                _ = upperBound;
            }

            return new MissionCondition(MissionKind.Between, lowerBound, width, false);
        }

        public static MissionCondition Modulo(int divisor, int remainder)
        {
            if (divisor < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(divisor), divisor, null);
            }

            if (remainder < 0 || remainder >= divisor)
            {
                throw new ArgumentOutOfRangeException(nameof(remainder), remainder, null);
            }

            return new MissionCondition(MissionKind.Modulo, divisor, remainder, false);
        }

        public static bool IsNoParameterKind(MissionKind kind)
        {
            switch (kind)
            {
                case MissionKind.Even:
                case MissionKind.Odd:
                case MissionKind.Integer:
                case MissionKind.Decimal:
                case MissionKind.Positive:
                case MissionKind.Negative:
                case MissionKind.Prime:
                case MissionKind.Zero:
                case MissionKind.Square:
                case MissionKind.Composite:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSingleParameterKind(MissionKind kind)
        {
            switch (kind)
            {
                case MissionKind.AtLeast:
                case MissionKind.AtMost:
                case MissionKind.Exact:
                case MissionKind.Multiple:
                case MissionKind.Absolute:
                case MissionKind.OnesDigit:
                case MissionKind.DecimalFirstDigit:
                    return true;
                default:
                    return false;
            }
        }

        public bool Equals(MissionCondition other)
        {
            return Kind == other.Kind && ParameterA == other.ParameterA && ParameterB == other.ParameterB && IsNegated == other.IsNegated;
        }

        public override bool Equals(object obj)
        {
            return obj is MissionCondition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Kind;
                hashCode = (hashCode * 397) ^ ParameterA;
                hashCode = (hashCode * 397) ^ ParameterB;
                hashCode = (hashCode * 397) ^ IsNegated.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MissionCondition left, MissionCondition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissionCondition left, MissionCondition right)
        {
            return !(left == right);
        }
    }
}
