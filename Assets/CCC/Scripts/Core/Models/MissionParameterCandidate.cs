#nullable enable
using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// カタログ構築前のTier未確定なミッション条件です。
    /// </summary>
    public readonly struct MissionParameterCandidate : IEquatable<MissionParameterCandidate>
    {
        public MissionCondition Condition { get; }

        /// <summary>
        /// 指定された条件からTier未確定の候補を生成します。
        /// </summary>
        /// <param name="condition">候補条件。</param>
        public MissionParameterCandidate(MissionCondition condition)
        {
            Condition = condition;
        }

        public bool Equals(MissionParameterCandidate other) => Condition.Equals(other.Condition);
        public override bool Equals(object? obj) => obj is MissionParameterCandidate other && Equals(other);
        public override int GetHashCode() => Condition.GetHashCode();
        public static bool operator ==(MissionParameterCandidate left, MissionParameterCandidate right) => left.Equals(right);
        public static bool operator !=(MissionParameterCandidate left, MissionParameterCandidate right) => !left.Equals(right);
    }
}
