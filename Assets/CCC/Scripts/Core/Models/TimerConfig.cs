using System;

namespace Ccc.Core.Model
{
    public sealed class TimerConfig
    {
        public TimeSpan InitialDuration { get; }
        public TimeSpan ReductionPerSection { get; }
        public TimeSpan MinimumDuration { get; }

        public TimerConfig(TimeSpan initialDuration, TimeSpan reductionPerSection, TimeSpan minimumDuration)
        {
            if (initialDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(initialDuration), initialDuration, "Initial duration must be positive.");
            }

            if (minimumDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumDuration), minimumDuration, "Minimum duration must be positive.");
            }

            if (reductionPerSection < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(reductionPerSection), reductionPerSection, "Reduction per section must be zero or positive.");
            }

            if (minimumDuration > initialDuration)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumDuration), minimumDuration, "Minimum duration must not exceed the initial duration.");
            }

            InitialDuration = initialDuration;
            ReductionPerSection = reductionPerSection;
            MinimumDuration = minimumDuration;
        }

        public TimeSpan GetDuration(int section)
        {
            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }

            var reducedDuration = InitialDuration - (ReductionPerSection * (section - 1));
            if (reducedDuration < MinimumDuration)
            {
                return MinimumDuration;
            }

            return reducedDuration;
        }
    }
}
