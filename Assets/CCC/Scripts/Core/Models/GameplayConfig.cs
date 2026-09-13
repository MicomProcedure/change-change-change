using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// Gameplayセッション中に固定されるルール設定を保持します。
    /// </summary>
    public sealed class GameplayConfig
    {
        /// <summary>入力可能な最小値を取得します。</summary>
        public int MinimumNumber { get; }

        /// <summary>入力可能な最大値を取得します。</summary>
        public int MaximumNumber { get; }

        /// <summary>セッション開始時の式を取得します。</summary>
        public Expression InitialExpression { get; }

        /// <summary>セッション開始時のLifeを取得します。</summary>
        public int InitialLife { get; }

        /// <summary>Sectionタイマー設定を取得します。</summary>
        public TimerConfig Timer { get; }

        /// <summary>SectionごとのTier進行定義を取得します。</summary>
        public TierProgression TierProgression { get; }

        /// <summary>現在の回答を画面へ表示するかを取得します。</summary>
        public bool ShowCurrentAnswer { get; }

        /// <summary>カタログ登録に必要な最小式適合割合を取得します。</summary>
        public float MinimumExpressionMatchRatio { get; }

        /// <summary>カタログ登録を許可する最大式適合割合を取得します。</summary>
        public float MaximumExpressionMatchRatio { get; }

        /// <summary>通常Missionのカタログ登録に必要な最小適合件数を取得します。</summary>
        public int MinimumExpressionMatchCount { get; }

        /// <summary>ZeroおよびPrimeのカタログ登録に必要な最小適合件数を取得します。</summary>
        public int ZeroAndPrimeMinimumExpressionMatchCount { get; }

        /// <summary>現在Tierを選択する相対重みを取得します。</summary>
        public float CurrentTierDrawWeight { get; }

        /// <summary>下位Tierを選択する相対重みを取得します。</summary>
        public float LowerTierDrawWeight { get; }

        /// <summary>
        /// 本実装版Mission抽選で優先する式適合割合を取得します。
        /// </summary>
        public float TargetExpressionMatchRatio { get; }

        /// <summary>
        /// 肯定Missionの基準重み1.00に対するNegatedMissionの相対重みを取得します。
        /// </summary>
        public float NegatedMissionSelectionWeight { get; }

        /// <summary>
        /// 目標式適合割合に近い順で最終抽選対象に含める候補数を取得します。
        /// </summary>
        public int MissionParameterSelectionPoolSize { get; }

        /// <summary>
        /// 検証済みのGameplayルール設定を生成します。
        /// </summary>
        /// <param name="minimumNumber">入力可能な最小値。</param>
        /// <param name="maximumNumber">入力可能な最大値。</param>
        /// <param name="initialExpression">セッション開始時の式。</param>
        /// <param name="initialLife">セッション開始時のLife。</param>
        /// <param name="timer">Sectionタイマー設定。</param>
        /// <param name="tierProgression">SectionごとのTier進行定義。</param>
        /// <param name="showCurrentAnswer">現在の回答を表示するか。</param>
        /// <param name="minimumExpressionMatchRatio">カタログ登録に必要な最小式適合割合。</param>
        /// <param name="maximumExpressionMatchRatio">カタログ登録を許可する最大式適合割合。</param>
        /// <param name="minimumExpressionMatchCount">通常Missionの最小適合件数。</param>
        /// <param name="zeroAndPrimeMinimumExpressionMatchCount">ZeroおよびPrimeの最小適合件数。</param>
        /// <param name="currentTierDrawWeight">現在Tierの相対抽選重み。</param>
        /// <param name="lowerTierDrawWeight">下位Tierの相対抽選重み。</param>
        /// <param name="targetExpressionMatchRatio">本実装版抽選で優先する式適合割合。</param>
        /// <param name="negatedMissionSelectionWeight">NegatedMissionの相対抽選重み。</param>
        /// <param name="missionParameterSelectionPoolSize">最終抽選プールへ含める候補数。</param>
        /// <exception cref="ArgumentNullException">必須の参照設定がnullの場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">数値設定が許容範囲外の場合。</exception>
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
            float lowerTierDrawWeight,
            float targetExpressionMatchRatio,
            float negatedMissionSelectionWeight,
            int missionParameterSelectionPoolSize)
        {
            ValidateNumberRange(minimumNumber, maximumNumber);
            ValidateInitialExpression(initialExpression, minimumNumber, maximumNumber);
            ValidateLife(initialLife);
            ValidateRequiredDependency(timer, nameof(timer));
            ValidateRequiredDependency(tierProgression, nameof(tierProgression));
            ValidateMatchRatios(minimumExpressionMatchRatio, maximumExpressionMatchRatio);
            ValidateExpressionMatchCounts(minimumExpressionMatchCount, zeroAndPrimeMinimumExpressionMatchCount);
            ValidateDrawWeights(currentTierDrawWeight, lowerTierDrawWeight);
            ValidateFullMissionDrawSettings(
                targetExpressionMatchRatio,
                negatedMissionSelectionWeight,
                missionParameterSelectionPoolSize);

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
            TargetExpressionMatchRatio = targetExpressionMatchRatio;
            NegatedMissionSelectionWeight = negatedMissionSelectionWeight;
            MissionParameterSelectionPoolSize = missionParameterSelectionPoolSize;
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

        private static void ValidateFullMissionDrawSettings(
            float targetExpressionMatchRatio,
            float negatedMissionSelectionWeight,
            int missionParameterSelectionPoolSize)
        {
            if (float.IsNaN(targetExpressionMatchRatio) ||
                float.IsInfinity(targetExpressionMatchRatio) ||
                targetExpressionMatchRatio < 0f ||
                targetExpressionMatchRatio > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetExpressionMatchRatio),
                    targetExpressionMatchRatio,
                    "Target expression match ratio must be finite and between zero and one.");
            }

            if (float.IsNaN(negatedMissionSelectionWeight) ||
                float.IsInfinity(negatedMissionSelectionWeight) ||
                negatedMissionSelectionWeight < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(negatedMissionSelectionWeight),
                    negatedMissionSelectionWeight,
                    "Negated mission selection weight must be finite and zero or positive.");
            }

            if (missionParameterSelectionPoolSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(missionParameterSelectionPoolSize),
                    missionParameterSelectionPoolSize,
                    "Mission parameter selection pool size must be greater than zero.");
            }
        }

        /// <summary>
        /// MissionKindに対応するカタログ登録時の最小適合件数を取得します。
        /// </summary>
        /// <param name="kind">最小適合件数を取得するMissionKind。</param>
        /// <returns>対象MissionKindの最小適合件数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">未定義のMissionKindの場合。</exception>
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

    /// <summary>
    /// GameplayConfigの既定プリセットを提供します。
    /// </summary>
    public static class GameplayConfigDefaults
    {
        /// <summary>
        /// Normal難易度の既定GameplayConfigを生成します。
        /// </summary>
        /// <returns>Normal難易度のGameplayConfig。</returns>
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
                lowerTierDrawWeight: 0.40f,
                targetExpressionMatchRatio: 0.20f,
                negatedMissionSelectionWeight: 0.60f,
                missionParameterSelectionPoolSize: 3);
        }
    }
}
