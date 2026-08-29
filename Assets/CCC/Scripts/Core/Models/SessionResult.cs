using System;

namespace Ccc.Core.Model
{
    public sealed class SessionResult
    {
        public int FinalSection { get; }
        public Mission LastMission { get; }
        public SubmissionResolution LastResolution { get; }

        public Expression FinalExpression => LastResolution.SubmittedExpression;
        public int CorrectAnswerCount => Math.Max(0, FinalSection - 1);

        public SessionResult(
            int finalSection,
            Mission lastMission,
            SubmissionResolution lastResolution)
        {
            if (finalSection <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(finalSection), finalSection, null);
            }

            if (lastMission == null)
            {
                throw new ArgumentNullException(nameof(lastMission));
            }

            if (lastResolution == null)
            {
                throw new ArgumentNullException(nameof(lastResolution));
            }

            if (lastMission.Id != lastResolution.MissionId)
            {
                throw new ArgumentException("Last mission must match the last resolution mission id.", nameof(lastResolution));
            }

            FinalSection = finalSection;
            LastMission = lastMission;
            LastResolution = lastResolution;
        }
    }
}
