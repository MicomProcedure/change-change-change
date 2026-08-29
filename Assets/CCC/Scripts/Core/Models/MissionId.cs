using System;

namespace Ccc.Core.Model
{
    public readonly struct MissionId : IEquatable<MissionId>
    {
        public int Value { get; }

        public MissionId(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            Value = value;
        }

        public bool Equals(MissionId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is MissionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(MissionId left, MissionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissionId left, MissionId right)
        {
            return !(left == right);
        }
    }
}
