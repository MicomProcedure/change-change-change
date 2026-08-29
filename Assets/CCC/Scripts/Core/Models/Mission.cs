using System;

namespace Ccc.Core.Model
{
    public sealed class Mission
    {
        public MissionId Id { get; }
        public MissionCondition Condition { get; }
        public int Tier { get; }

        public Mission(MissionId id, MissionCondition condition, int tier)
        {
            if (tier <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tier), tier, null);
            }

            Id = id;
            Condition = condition;
            Tier = tier;
        }
    }
}
