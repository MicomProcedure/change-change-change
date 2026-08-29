using System;

namespace Ccc.Core.Model
{
    public sealed class TierProgression
    {
        public TierProgression()
        {
        }

        public int GetTier(int section)
        {
            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }

            if (section <= 3)
            {
                return 1;
            }

            if (section <= 7)
            {
                return 2;
            }

            if (section <= 11)
            {
                return 3;
            }

            return 4;
        }
    }
}
