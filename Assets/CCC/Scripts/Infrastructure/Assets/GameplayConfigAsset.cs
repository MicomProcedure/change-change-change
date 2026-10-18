using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Model;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// Gameplay設定をUnity Inspectorで編集するScriptableObjectです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Gameplay Config")]
    public sealed class GameplayConfigAsset : ScriptableObject
    {
        private const int DefaultMinimumNumber = 1;
        private const int DefaultMaximumNumber = 5;
        private const int DefaultInitialLeft = 1;
        private const int DefaultInitialRight = 1;
        private const int DefaultInitialLife = 3;
        private const float DefaultInitialDurationSeconds = 10f;
        private const float DefaultReductionPerSectionSeconds = 0.5f;
        private const float DefaultMinimumDurationSeconds = 3f;
        private const float DefaultBonusRatio = 0f;
        private const float DefaultMaximumBonusRatio = 1f;
        private const float DefaultManualSubmitBonusSeconds = 0f;
        private const float DefaultMinimumExpressionMatchRatio = 0.10f;
        private const float DefaultMaximumExpressionMatchRatio = 0.60f;
        private const float DefaultCurrentTierDrawWeight = 0.60f;
        private const float DefaultLowerTierDrawWeight = 0.40f;
        private const float DefaultCurrentExpressionSatisfiedMissionSelectionRatio = 0.10f;
        private const float DefaultTargetExpressionMatchRatio = 0.20f;
        private const float DefaultNegatedMissionSelectionWeight = 0.60f;
        private const int DefaultMissionParameterSelectionPoolSize = 3;

        [SerializeField]
        private int minimumNumber = DefaultMinimumNumber;

        [SerializeField]
        private int maximumNumber = DefaultMaximumNumber;

        [SerializeField]
        private int initialLeft = DefaultInitialLeft;

        [SerializeField]
        private OperatorType initialOperator = OperatorType.Add;

        [SerializeField]
        private int initialRight = DefaultInitialRight;

        [SerializeField]
        private int initialLife = DefaultInitialLife;

        [SerializeField]
        private float initialDurationSeconds = DefaultInitialDurationSeconds;

        [SerializeField]
        private float reductionPerSectionSeconds = DefaultReductionPerSectionSeconds;

        [SerializeField]
        private float minimumDurationSeconds = DefaultMinimumDurationSeconds;

        [SerializeField]
        private float bonusRatio = DefaultBonusRatio;

        [SerializeField]
        private float maximumBonusRatio = DefaultMaximumBonusRatio;

        [SerializeField]
        private float manualSubmitBonusSeconds = DefaultManualSubmitBonusSeconds;

        [SerializeField, FormerlySerializedAs("isBonusCarryOverEnabled")]
        private bool isBonusCarryOverOnSuccessEnabled;

        [SerializeField, FormerlySerializedAs("shouldKeepBonusOnFailure")]
        private bool isBonusCarryOverOnFailureEnabled;

        [SerializeField]
        private List<int> tierStartSections = new List<int> { 1, 4, 8, 12 };

        [SerializeField]
        private bool showCurrentAnswer;

        [SerializeField]
        private float minimumExpressionMatchRatio = DefaultMinimumExpressionMatchRatio;

        [SerializeField]
        private float maximumExpressionMatchRatio = DefaultMaximumExpressionMatchRatio;

        [SerializeField]
        private int minimumExpressionMatchCount = 2;

        [SerializeField]
        private int zeroAndPrimeMinimumExpressionMatchCount = 1;

        [SerializeField]
        private float currentTierDrawWeight = DefaultCurrentTierDrawWeight;

        [SerializeField]
        private float lowerTierDrawWeight = DefaultLowerTierDrawWeight;

        [SerializeField]
        private float currentExpressionSatisfiedMissionSelectionRatio =
            DefaultCurrentExpressionSatisfiedMissionSelectionRatio;

        [SerializeField]
        private float targetExpressionMatchRatio = DefaultTargetExpressionMatchRatio;

        [SerializeField]
        private float negatedMissionSelectionWeight = DefaultNegatedMissionSelectionWeight;

        [SerializeField]
        private int missionParameterSelectionPoolSize = DefaultMissionParameterSelectionPoolSize;

        /// <summary>
        /// Inspector設定を検証し、検出されたエラー一覧を返します。
        /// </summary>
        /// <returns>技術的な設定エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (minimumNumber <= 0)
            {
                errors.Add("Minimum number must be greater than zero.");
            }

            if (maximumNumber < minimumNumber)
            {
                errors.Add("Maximum number must be greater than or equal to minimum number.");
            }

            if (initialLife <= 0)
            {
                errors.Add("Initial life must be greater than zero.");
            }

            if (!Enum.IsDefined(typeof(OperatorType), initialOperator))
            {
                errors.Add($"Initial operator is undefined: {initialOperator}.");
            }

            if (minimumNumber <= maximumNumber &&
                (initialLeft < minimumNumber || initialLeft > maximumNumber ||
                 initialRight < minimumNumber || initialRight > maximumNumber))
            {
                errors.Add("Initial expression numbers must be within the configured number range.");
            }

            ValidateTimer(errors);
            ValidateTierBoundaries(errors);
            ValidateRatio(errors, minimumExpressionMatchRatio, "Minimum expression match ratio");
            ValidateRatio(errors, maximumExpressionMatchRatio, "Maximum expression match ratio");
            if (IsFinite(minimumExpressionMatchRatio) && IsFinite(maximumExpressionMatchRatio) &&
                (minimumExpressionMatchRatio > maximumExpressionMatchRatio))
            {
                errors.Add("Maximum expression match ratio must be greater than or equal to minimum ratio.");
            }

            if (minimumExpressionMatchCount <= 0)
            {
                errors.Add("Minimum expression match count must be greater than zero.");
            }

            if (zeroAndPrimeMinimumExpressionMatchCount <= 0)
            {
                errors.Add("Zero and prime minimum expression match count must be greater than zero.");
            }

            ValidateWeight(errors, currentTierDrawWeight, "Current tier draw weight");
            ValidateWeight(errors, lowerTierDrawWeight, "Lower tier draw weight");
            if (IsFinite(currentTierDrawWeight) && IsFinite(lowerTierDrawWeight) &&
                currentTierDrawWeight + lowerTierDrawWeight <= 0f)
            {
                errors.Add("Current and lower tier draw weights cannot both be zero.");
            }

            ValidateRatio(
                errors,
                currentExpressionSatisfiedMissionSelectionRatio,
                "Current expression satisfied mission selection ratio");
            ValidateRatio(errors, targetExpressionMatchRatio, "Target expression match ratio");
            ValidateWeight(
                errors,
                negatedMissionSelectionWeight,
                "Negated mission selection weight");
            if (missionParameterSelectionPoolSize <= 0)
            {
                errors.Add("Mission parameter selection pool size must be greater than zero.");
            }

            return errors.Distinct().ToArray();
        }

        /// <summary>
        /// 検証済みInspector値から純C#のGameplayConfigを1回生成します。
        /// </summary>
        /// <returns>Runtime用GameplayConfig。</returns>
        /// <exception cref="InvalidOperationException">Asset検証に失敗した場合。</exception>
        public GameplayConfig CreateRuntimeConfig()
        {
            IReadOnlyList<string> errors = Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Gameplay config asset validation failed: " + string.Join(" | ", errors));
            }

            return new GameplayConfig(
                minimumNumber,
                maximumNumber,
                new Expression(initialLeft, initialOperator, initialRight),
                initialLife,
                new TimerConfig(
                TimeSpan.FromSeconds(initialDurationSeconds),
                TimeSpan.FromSeconds(reductionPerSectionSeconds),
                TimeSpan.FromSeconds(minimumDurationSeconds),
                bonusRatio,
                maximumBonusRatio,
                isBonusCarryOverOnSuccessEnabled,
                isBonusCarryOverOnFailureEnabled,
                TimeSpan.FromSeconds(manualSubmitBonusSeconds)),
                new TierProgression(tierStartSections),
                showCurrentAnswer,
                minimumExpressionMatchRatio,
                maximumExpressionMatchRatio,
                minimumExpressionMatchCount,
                zeroAndPrimeMinimumExpressionMatchCount,
                currentTierDrawWeight,
                lowerTierDrawWeight,
                currentExpressionSatisfiedMissionSelectionRatio,
                targetExpressionMatchRatio,
                negatedMissionSelectionWeight,
                missionParameterSelectionPoolSize);
        }

        private void OnValidate()
        {
            IReadOnlyList<string> errors = Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                Debug.LogError($"[GameplayConfigAsset] {name}: {errors[index]}");
            }
        }

        private void ValidateTimer(ICollection<string> errors)
        {
            if (!IsFinite(initialDurationSeconds) || initialDurationSeconds <= 0f)
            {
                errors.Add("Initial duration must be finite and greater than zero.");
            }

            if (!IsFinite(reductionPerSectionSeconds) || reductionPerSectionSeconds < 0f)
            {
                errors.Add("Reduction per section must be finite and zero or positive.");
            }

            if (!IsFinite(minimumDurationSeconds) || minimumDurationSeconds <= 0f)
            {
                errors.Add("Minimum duration must be finite and greater than zero.");
            }

            if (IsFinite(initialDurationSeconds) && IsFinite(minimumDurationSeconds) &&
                minimumDurationSeconds > initialDurationSeconds)
            {
                errors.Add("Minimum duration must not exceed initial duration.");
            }

            if (!IsFinite(bonusRatio) || bonusRatio < 0f)
            {
                errors.Add("Bonus ratio must be finite and zero or positive.");
            }

            if (!IsFinite(maximumBonusRatio) || maximumBonusRatio < 0f)
            {
                errors.Add("Maximum bonus ratio must be finite and zero or positive.");
            }

            if (!IsFinite(manualSubmitBonusSeconds) || manualSubmitBonusSeconds < 0f)
            {
                errors.Add("Manual submit bonus seconds must be finite and zero or positive.");
            }
        }

        private void ValidateTierBoundaries(ICollection<string> errors)
        {
            if (tierStartSections == null || tierStartSections.Count == 0)
            {
                errors.Add("Tier boundaries must contain at least one tier beginning at section 1.");
                return;
            }

            if (tierStartSections[0] != 1)
            {
                errors.Add("Tier 1 must begin at section 1.");
            }

            for (int index = 0; index < tierStartSections.Count; index++)
            {
                if (tierStartSections[index] <= 0 ||
                    (index > 0 && tierStartSections[index] <= tierStartSections[index - 1]))
                {
                    errors.Add("Tier boundaries must be positive and strictly ascending.");
                    break;
                }
            }
        }

        private static void ValidateRatio(ICollection<string> errors, float ratio, string label)
        {
            if (!IsFinite(ratio) || ratio < 0f || ratio > 1f)
            {
                errors.Add($"{label} must be finite and between zero and one.");
            }
        }

        private static void ValidateWeight(ICollection<string> errors, float weight, string label)
        {
            if (!IsFinite(weight) || weight < 0f)
            {
                errors.Add($"{label} must be finite and zero or positive.");
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
