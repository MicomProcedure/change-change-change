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
            return Create(kind, false);
        }

        /// <summary>
        /// パラメータなしのMissionConditionを生成します。
        /// </summary>
        /// <param name="kind">ミッションの種類。</param>
        /// <param name="isNegated">条件を否定するかどうか。</param>
        /// <returns>生成された条件。</returns>
        public static MissionCondition Create(MissionKind kind, bool isNegated)
        {
            MissionKindDefinition definition = GetDefinition(kind);
            if (definition.ParameterStructure != MissionParameterStructure.None)
            {
                throw new ArgumentException(
                    "This MissionKind does not accept a parameterless factory.",
                    nameof(kind));
            }

            return new MissionCondition(kind, 0, 0, isNegated);
        }

        public static MissionCondition Create(MissionKind kind, int parameter)
        {
            return Create(kind, parameter, false);
        }

        /// <summary>
        /// 1パラメータのMissionConditionを生成します。
        /// </summary>
        /// <param name="kind">ミッションの種類。</param>
        /// <param name="parameter">条件パラメータ。</param>
        /// <param name="isNegated">条件を否定するかどうか。</param>
        /// <returns>生成された条件。</returns>
        public static MissionCondition Create(MissionKind kind, int parameter, bool isNegated)
        {
            MissionKindDefinition definition = GetDefinition(kind);
            if (definition.ParameterStructure != MissionParameterStructure.Single)
            {
                throw new ArgumentException(
                    "This MissionKind does not accept a single parameter.",
                    nameof(kind));
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
                case MissionKind.Divisor:
                case MissionKind.DigitSum:
                    if (parameter < 1)
                    {
                        throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
                    }
                    break;
                case MissionKind.Power:
                    if (parameter < 2)
                    {
                        throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null);
                    }
                    break;
                case MissionKind.OnesDigit:
                case MissionKind.TensDigit:
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

            return new MissionCondition(kind, parameter, 0, isNegated);
        }

        public static MissionCondition Between(int lowerBound, int width)
        {
            return Between(lowerBound, width, false);
        }

        /// <summary>
        /// 範囲条件のMissionConditionを生成します。
        /// </summary>
        /// <param name="lowerBound">下限。</param>
        /// <param name="width">下限から上限までの幅。</param>
        /// <param name="isNegated">条件を否定するかどうか。</param>
        /// <returns>生成された条件。</returns>
        public static MissionCondition Between(int lowerBound, int width, bool isNegated)
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

            return new MissionCondition(MissionKind.Between, lowerBound, width, isNegated);
        }

        public static MissionCondition Modulo(int divisor, int remainder)
        {
            return Modulo(divisor, remainder, false);
        }

        /// <summary>
        /// 剰余条件のMissionConditionを生成します。
        /// </summary>
        /// <param name="divisor">法。</param>
        /// <param name="remainder">余り。</param>
        /// <param name="isNegated">条件を否定するかどうか。</param>
        /// <returns>生成された条件。</returns>
        public static MissionCondition Modulo(int divisor, int remainder, bool isNegated)
        {
            if (divisor < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(divisor), divisor, null);
            }

            if (remainder < 0 || remainder >= divisor)
            {
                throw new ArgumentOutOfRangeException(nameof(remainder), remainder, null);
            }

            return new MissionCondition(MissionKind.Modulo, divisor, remainder, isNegated);
        }

        public static bool IsNoParameterKind(MissionKind kind)
        {
            if (!Enum.IsDefined(typeof(MissionKind), kind))
            {
                return false;
            }

            return MissionKindDefinitionCatalog.Get(kind).ParameterStructure ==
                   MissionParameterStructure.None;
        }

        public static bool IsSingleParameterKind(MissionKind kind)
        {
            if (!Enum.IsDefined(typeof(MissionKind), kind))
            {
                return false;
            }

            return MissionKindDefinitionCatalog.Get(kind).ParameterStructure ==
                   MissionParameterStructure.Single;
        }

        private static MissionKindDefinition GetDefinition(MissionKind kind)
        {
            return MissionKindDefinitionCatalog.Get(kind);
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
