using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Model;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// Missionパラメータ生成範囲とCatalog評価適用規則を編集するScriptableObjectです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Mission Parameter Generation Profile")]
    public sealed class MissionParameterGenerationProfileAsset : ScriptableObject
    {
        private const int DefaultCandidateLimit = 1;
        private const float DefaultBetweenTierTwoThreshold = 0.25f;

        [SerializeField]
        private int maximumTotalCandidateCount = DefaultCandidateLimit;

        [SerializeField]
        private float betweenTierTwoThreshold = DefaultBetweenTierTwoThreshold;

        [SerializeField]
        private List<MissionKindGenerationProfileEntry> kindRules =
            new List<MissionKindGenerationProfileEntry>();

        /// <summary>
        /// Profile構造を検証し、検出された設定エラー一覧を返します。
        /// </summary>
        /// <returns>設定エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (maximumTotalCandidateCount <= 0)
            {
                errors.Add("Maximum total candidate count must be greater than zero.");
            }

            if (!IsRatio(betweenTierTwoThreshold))
            {
                errors.Add("Between Tier 2 threshold must be finite and between zero and one.");
            }

            if (kindRules == null || kindRules.Count == 0)
            {
                errors.Add("Mission kind generation rules are missing.");
                return errors;
            }

            var registeredKinds = new HashSet<MissionKind>();
            bool hasEnabledKind = false;
            for (int index = 0; index < kindRules.Count; index++)
            {
                MissionKindGenerationProfileEntry rule = kindRules[index];
                if (rule == null)
                {
                    errors.Add($"Mission kind generation rule at index {index} is null.");
                    continue;
                }

                if (!Enum.IsDefined(typeof(MissionKind), rule.Kind))
                {
                    errors.Add($"Mission kind generation rule at index {index} has an undefined kind.");
                    continue;
                }

                if (!registeredKinds.Add(rule.Kind))
                {
                    errors.Add($"MissionKind.{rule.Kind} is duplicated in the profile.");
                }

                hasEnabledKind |= rule.IsEnabled;
                ValidateRule(rule, index, errors);
            }

            if (!hasEnabledKind)
            {
                errors.Add("At least one MissionKind must be enabled.");
            }

            return errors.Distinct().ToArray();
        }

        /// <summary>
        /// 検証済みProfileから純C#候補生成設定を生成します。
        /// </summary>
        /// <returns>Runtime用候補生成設定。</returns>
        public MissionParameterGenerationSettings CreateGenerationSettings()
        {
            ThrowIfInvalid();
            MissionKindGenerationRule[] rules = kindRules
                .Select(rule => new MissionKindGenerationRule(
                    rule.Kind,
                    rule.IsEnabled,
                    rule.AppliesMatchRatioRange,
                    rule.GeneratesNegated,
                    rule.MaximumCandidateCount,
                    rule.ParameterA.CreateDomainOrNull(),
                    rule.ParameterB.CreateDomainOrNull()))
                .ToArray();
            return new MissionParameterGenerationSettings(
                rules,
                maximumTotalCandidateCount,
                Rational.FromSingle(betweenTierTwoThreshold));
        }

        /// <summary>
        /// 検証済みProfileとGameplayConfigから純C# Catalog評価規則を生成します。
        /// </summary>
        /// <param name="config">同一Sessionで使用するGameplayConfig。</param>
        /// <returns>Runtime用Catalog評価規則。</returns>
        public MissionCatalogEvaluationPolicy CreateEvaluationPolicy(GameplayConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            ThrowIfInvalid();
            MissionKind[] ratioAppliedKinds = kindRules
                .Where(rule => rule.IsEnabled && rule.AppliesMatchRatioRange)
                .Select(rule => rule.Kind)
                .ToArray();
            return new MissionCatalogEvaluationPolicy(
                Rational.FromSingle(config.MinimumExpressionMatchRatio),
                Rational.FromSingle(config.MaximumExpressionMatchRatio),
                config.MinimumExpressionMatchCount,
                config.ZeroAndPrimeMinimumExpressionMatchCount,
                ratioAppliedKinds);
        }

        private static void ValidateRule(
            MissionKindGenerationProfileEntry rule,
            int index,
            ICollection<string> errors)
        {
            MissionKindDefinition definition = MissionKindDefinitionCatalog.Get(rule.Kind);
            if (rule.MaximumCandidateCount <= 0)
            {
                errors.Add($"MissionKind.{rule.Kind} maximum candidate count must be greater than zero.");
            }

            if (rule.GeneratesNegated && !definition.CanGenerateNegated)
            {
                errors.Add($"MissionKind.{rule.Kind} does not allow Negated generation.");
            }

            bool expectsParameterA =
                definition.ParameterStructure != MissionParameterStructure.None;
            bool expectsParameterB =
                definition.ParameterStructure == MissionParameterStructure.Pair;
            if (rule.ParameterA == null || rule.ParameterB == null)
            {
                errors.Add(
                    $"Mission kind generation rule at index {index} has missing domain data.");
                return;
            }

            if (rule.ParameterA.IsConfigured != expectsParameterA)
            {
                errors.Add(
                    $"Mission kind generation rule at index {index} ParameterA does not match " +
                    $"{definition.ParameterStructure} structure.");
            }

            if (rule.ParameterB.IsConfigured != expectsParameterB)
            {
                errors.Add(
                    $"Mission kind generation rule at index {index} ParameterB does not match " +
                    $"{definition.ParameterStructure} structure.");
            }

            rule.ParameterA.AddValidationErrors(
                $"MissionKind.{rule.Kind} ParameterA",
                errors);
            rule.ParameterB.AddValidationErrors(
                $"MissionKind.{rule.Kind} ParameterB",
                errors);
        }

        private void ThrowIfInvalid()
        {
            IReadOnlyList<string> errors = Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Mission parameter generation profile validation failed: " +
                    string.Join(" | ", errors));
            }
        }

        private void OnValidate()
        {
            IReadOnlyList<string> errors = Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                Debug.LogError(
                    $"[MissionParameterGenerationProfileAsset] {name}: {errors[index]}");
            }
        }

        private static bool IsRatio(float ratio)
        {
            return !float.IsNaN(ratio) &&
                   !float.IsInfinity(ratio) &&
                   ratio >= 0f &&
                   ratio <= 1f;
        }

        [Serializable]
        private sealed class MissionKindGenerationProfileEntry
        {
            [SerializeField] private MissionKind kind;
            [SerializeField] private bool isEnabled;
            [SerializeField] private bool appliesMatchRatioRange = true;
            [SerializeField] private bool generatesNegated;
            [SerializeField] private int maximumCandidateCount = DefaultCandidateLimit;
            [SerializeField] private IntegerParameterDomainAssetData parameterA =
                new IntegerParameterDomainAssetData();
            [SerializeField] private IntegerParameterDomainAssetData parameterB =
                new IntegerParameterDomainAssetData();

            public MissionKind Kind => kind;
            public bool IsEnabled => isEnabled;
            public bool AppliesMatchRatioRange => appliesMatchRatioRange;
            public bool GeneratesNegated => generatesNegated;
            public int MaximumCandidateCount => maximumCandidateCount;
            public IntegerParameterDomainAssetData ParameterA => parameterA;
            public IntegerParameterDomainAssetData ParameterB => parameterB;
        }

        private enum IntegerParameterDomainMode
        {
            ExplicitValues = 0,
            InclusiveRange = 1
        }

        [Serializable]
        private sealed class IntegerParameterDomainAssetData
        {
            [SerializeField] private bool isConfigured;
            [SerializeField] private IntegerParameterDomainMode mode;
            [SerializeField] private List<int> explicitValues = new List<int>();
            [SerializeField] private int minimum;
            [SerializeField] private int maximum;
            [SerializeField] private int step = 1;

            public bool IsConfigured => isConfigured;

            public IntegerParameterDomain CreateDomainOrNull()
            {
                if (!isConfigured)
                {
                    return null;
                }

                switch (mode)
                {
                    case IntegerParameterDomainMode.ExplicitValues:
                        return IntegerParameterDomain.FromExplicitValues(explicitValues);
                    case IntegerParameterDomainMode.InclusiveRange:
                        return IntegerParameterDomain.FromInclusiveRange(minimum, maximum, step);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }

            public void AddValidationErrors(
                string label,
                ICollection<string> errors)
            {
                if (!isConfigured)
                {
                    return;
                }

                if (!Enum.IsDefined(typeof(IntegerParameterDomainMode), mode))
                {
                    errors.Add($"{label} has an undefined domain mode.");
                    return;
                }

                if (mode == IntegerParameterDomainMode.ExplicitValues &&
                    (explicitValues == null || explicitValues.Count == 0))
                {
                    errors.Add($"{label} explicit values are missing.");
                }

                if (mode == IntegerParameterDomainMode.InclusiveRange)
                {
                    if (maximum < minimum)
                    {
                        errors.Add($"{label} maximum must be greater than or equal to minimum.");
                    }

                    if (step <= 0)
                    {
                        errors.Add($"{label} step must be greater than zero.");
                    }
                }
            }
        }
    }
}
