using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// カタログ構築前の事前定義されたミッション条件とTierのペア。
    /// </summary>
    public readonly struct MissionParameterCandidate : IEquatable<MissionParameterCandidate>
    {
        public MissionCondition Condition { get; }
        public int Tier { get; }

        public MissionParameterCandidate(MissionCondition condition, int tier)
        {
            if (tier <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tier), "Tier must be greater than zero.");
            }

            Condition = condition;
            Tier = tier;
        }

        public bool Equals(MissionParameterCandidate other) => Condition.Equals(other.Condition) && Tier == other.Tier;
        public override bool Equals(object? obj) => obj is MissionParameterCandidate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Condition, Tier);
        public static bool operator ==(MissionParameterCandidate left, MissionParameterCandidate right) => left.Equals(right);
        public static bool operator !=(MissionParameterCandidate left, MissionParameterCandidate right) => !left.Equals(right);
    }
}
