using System;

namespace Ccc.Core.Model
{
    public sealed class GameplayConfig
    {
        public int MinimumNumber { get; }
        public int MaximumNumber { get; }
        public Expression InitialExpression { get; }
        public int InitialLife { get; }
        public TimerConfig Timer { get; }
        public TierProgression TierProgression { get; }
        public bool ShowCurrentAnswer { get; }
        public float MinimumExpressionMatchRatio { get; }
        public float MaximumExpressionMatchRatio { get; }
        public int MinimumExpressionMatchCount { get; }
        public int ZeroAndPrimeMinimumExpressionMatchCount { get; }
        public float CurrentTierDrawWeight { get; }
        public float LowerTierDrawWeight { get; }

        public GameplayConfig(
            int minimumNumber,
            int maximumNumber,
            Expression initialExpression,
            int initialLife,
            TimerConfig timer,
            TierProgression tierProgression,
            bool showCurrentAnswer,
            float minimumExpressionMatchRatio,
            float maximumExpressionMatchRatio,
            int minimumExpressionMatchCount,
            int zeroAndPrimeMinimumExpressionMatchCount,
            float currentTierDrawWeight,
            float lowerTierDrawWeight)
        {
            ValidateNumberRange(minimumNumber, maximumNumber);
            ValidateInitialExpression(initialExpression, minimumNumber, maximumNumber);
            ValidateLife(initialLife);
            ValidateRequiredDependency(timer, nameof(timer));
            ValidateRequiredDependency(tierProgression, nameof(tierProgression));
            ValidateMatchRatios(minimumExpressionMatchRatio, maximumExpressionMatchRatio);
            ValidateExpressionMatchCounts(minimumExpressionMatchCount, zeroAndPrimeMinimumExpressionMatchCount);
            ValidateDrawWeights(currentTierDrawWeight, lowerTierDrawWeight);

            MinimumNumber = minimumNumber;
            MaximumNumber = maximumNumber;
            InitialExpression = initialExpression;
            InitialLife = initialLife;
            Timer = timer;
            TierProgression = tierProgression;
            ShowCurrentAnswer = showCurrentAnswer;
            MinimumExpressionMatchRatio = minimumExpressionMatchRatio;
            MaximumExpressionMatchRatio = maximumExpressionMatchRatio;
            MinimumExpressionMatchCount = minimumExpressionMatchCount;
            ZeroAndPrimeMinimumExpressionMatchCount = zeroAndPrimeMinimumExpressionMatchCount;
            CurrentTierDrawWeight = currentTierDrawWeight;
            LowerTierDrawWeight = lowerTierDrawWeight;
        }

        private static void ValidateNumberRange(int minimumNumber, int maximumNumber)
        {
            if (minimumNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumNumber), minimumNumber, null);
            }

            if (maximumNumber < minimumNumber)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumNumber), maximumNumber, null);
            }
        }

        private static void ValidateInitialExpression(Expression initialExpression, int minimumNumber, int maximumNumber)
        {
            if (initialExpression.Left < minimumNumber || initialExpression.Left > maximumNumber || initialExpression.Right < minimumNumber || initialExpression.Right > maximumNumber)
            {
                throw new ArgumentOutOfRangeException(nameof(initialExpression), initialExpression, "Initial expression values must be within the configured number range.");
            }
        }

        private static void ValidateLife(int initialLife)
        {
            if (initialLife <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialLife), initialLife, null);
            }
        }

        private static void ValidateRequiredDependency(object value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateMatchRatios(float minimumExpressionMatchRatio, float maximumExpressionMatchRatio)
        {
            if (float.IsNaN(minimumExpressionMatchRatio) || float.IsNaN(maximumExpressionMatchRatio) || float.IsInfinity(minimumExpressionMatchRatio) || float.IsInfinity(maximumExpressionMatchRatio))
            {
                throw new ArgumentOutOfRangeException(nameof(minimumExpressionMatchRatio), minimumExpressionMatchRatio, "Match ratios must be finite.");
            }

            if (minimumExpressionMatchRatio < 0f || minimumExpressionMatchRatio > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumExpressionMatchRatio), minimumExpressionMatchRatio, "Minimum expression match ratio must be between 0 and 1.");
            }

            if (maximumExpressionMatchRatio < 0f || maximumExpressionMatchRatio > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumExpressionMatchRatio), maximumExpressionMatchRatio, "Maximum expression match ratio must be between 0 and 1.");
            }

            if (minimumExpressionMatchRatio > maximumExpressionMatchRatio)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumExpressionMatchRatio), maximumExpressionMatchRatio, "Maximum ratio must be greater than or equal to the minimum ratio.");
            }
        }

        private static void ValidateExpressionMatchCounts(int minimumExpressionMatchCount, int zeroAndPrimeMinimumExpressionMatchCount)
        {
            if (minimumExpressionMatchCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumExpressionMatchCount), minimumExpressionMatchCount, null);
            }

            if (zeroAndPrimeMinimumExpressionMatchCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(zeroAndPrimeMinimumExpressionMatchCount), zeroAndPrimeMinimumExpressionMatchCount, null);
            }
        }

        private static void ValidateDrawWeights(float currentTierDrawWeight, float lowerTierDrawWeight)
        {
            if (float.IsNaN(currentTierDrawWeight) || float.IsNaN(lowerTierDrawWeight) || float.IsInfinity(currentTierDrawWeight) || float.IsInfinity(lowerTierDrawWeight))
            {
                throw new ArgumentOutOfRangeException(nameof(currentTierDrawWeight), currentTierDrawWeight, "Draw weights must be finite.");
            }

            if (currentTierDrawWeight < 0f || lowerTierDrawWeight < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(currentTierDrawWeight), currentTierDrawWeight, "Draw weights must be zero or positive.");
            }

            if (currentTierDrawWeight + lowerTierDrawWeight <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(currentTierDrawWeight), currentTierDrawWeight, "Draw weights must sum to a positive value.");
            }
        }

        public int GetMinimumExpressionMatchCount(MissionKind kind)
        {
            if (!Enum.IsDefined(typeof(MissionKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            switch (kind)
            {
                case MissionKind.Zero:
                case MissionKind.Prime:
                    return ZeroAndPrimeMinimumExpressionMatchCount;
                default:
                    return MinimumExpressionMatchCount;
            }
        }
    }

    public static class GameplayConfigDefaults
    {
        public static GameplayConfig CreateNormal()
        {
            return new GameplayConfig(
                minimumNumber: 1,
                maximumNumber: 5,
                initialExpression: new Expression(1, OperatorType.Add, 1),
                initialLife: 3,
                timer: new TimerConfig(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(3)),
                tierProgression: new TierProgression(),
                showCurrentAnswer: false,
                minimumExpressionMatchRatio: 0.10f,
                maximumExpressionMatchRatio: 0.60f,
                minimumExpressionMatchCount: 2,
                zeroAndPrimeMinimumExpressionMatchCount: 1,
                currentTierDrawWeight: 0.60f,
                lowerTierDrawWeight: 0.40f);
        }
    }
}
