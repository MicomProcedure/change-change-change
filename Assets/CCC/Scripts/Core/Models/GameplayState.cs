#nullable enable
using System;

namespace Ccc.Core.Model
{
    public enum GameplayStatus
    {
        Playing = 0,
        Transitioning = 1,
        GameOver = 2
    }

    public enum SubmissionTrigger
    {
        ManualSubmit = 0,
        TimeExpired = 1
    }

    public sealed class GameplayState
    {
        public GameplayStatus Status { get; }
        public int Section { get; }
        public int Life { get; }
        public Mission CurrentMission { get; }
        public SubmissionResolution? LastResolution { get; }

        public int CorrectAnswerCount => Math.Max(0, Section - 1);
        public bool IsAcceptingSubmit => Status == GameplayStatus.Playing;
        public bool IsTransitioning => Status == GameplayStatus.Transitioning;

        public GameplayState(
            GameplayStatus status,
            int section,
            int life,
            Mission currentMission,
            SubmissionResolution? lastResolution)
        {
            if (!Enum.IsDefined(typeof(GameplayStatus), status))
            {
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            if (section <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }

            if (life < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(life), life, null);
            }

            if (currentMission == null)
            {
                throw new ArgumentNullException(nameof(currentMission));
            }

            if (status == GameplayStatus.Playing)
            {
                if (life <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(life), life, "Playing state requires a positive life value.");
                }
            }

            if (status == GameplayStatus.Transitioning || status == GameplayStatus.GameOver)
            {
                if (lastResolution == null)
                {
                    throw new ArgumentNullException(nameof(lastResolution), "Transitioning and GameOver require a last resolution.");
                }

                if (currentMission.Id != lastResolution.MissionId)
                {
                    throw new ArgumentException("Current mission must match the last resolution mission id.", nameof(currentMission));
                }
            }

            if (status == GameplayStatus.GameOver)
            {
                if (life != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(life), life, "GameOver state requires zero life.");
                }
            }

            Status = status;
            Section = section;
            Life = life;
            CurrentMission = currentMission;
            LastResolution = lastResolution;
        }
    }
}
