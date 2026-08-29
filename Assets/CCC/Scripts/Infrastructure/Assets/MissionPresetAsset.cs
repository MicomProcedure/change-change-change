using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Core.Model;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// Mission候補定義をUnity Inspectorで編集するScriptableObjectです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Mission Preset")]
    public sealed class MissionPresetAsset : ScriptableObject
    {
        [SerializeField]
        private List<MissionPresetEntry> entries = new List<MissionPresetEntry>();

        /// <summary>
        /// Mission候補定義を検証し、検出されたエラー一覧を返します。
        /// </summary>
        /// <returns>技術的な設定エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            var duplicateChecker = new HashSet<(MissionCondition, int)>();
            if (entries == null || entries.Count == 0)
            {
                errors.Add("Mission preset entries are missing.");
                return errors.ToArray();
            }

            for (int index = 0; index < entries.Count; index++)
            {
                MissionPresetEntry entry = entries[index];
                if (entry == null)
                {
                    errors.Add($"Mission preset entry at index {index} is null.");
                    continue;
                }

                if (entry.Tier <= 0)
                {
                    errors.Add($"Mission preset entry at index {index} has an invalid tier.");
                }

                if (!TryCreateCondition(entry, out MissionCondition condition, out string conditionError))
                {
                    errors.Add($"Mission preset entry at index {index}: {conditionError}");
                    continue;
                }

                if (!duplicateChecker.Add((condition, entry.Tier)))
                {
                    errors.Add(
                        $"Mission preset entry at index {index} duplicates MissionKind.{condition.Kind} in tier {entry.Tier}.");
                }
            }

            return errors.Distinct().ToArray();
        }

        /// <summary>
        /// 検証済みのMissionParameterCandidate列を生成します。
        /// </summary>
        /// <returns>Runtime用のMission候補定義。</returns>
        /// <exception cref="InvalidOperationException">Asset検証に失敗した場合。</exception>
        public IReadOnlyList<MissionParameterCandidate> CreateCandidates()
        {
            IReadOnlyList<string> errors = Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Mission preset asset validation failed: " + string.Join(" | ", errors));
            }

            var candidates = new MissionParameterCandidate[entries.Count];
            for (int index = 0; index < entries.Count; index++)
            {
                TryCreateCondition(entries[index], out MissionCondition condition, out _);
                candidates[index] = new MissionParameterCandidate(condition, entries[index].Tier);
            }

            return candidates;
        }

        private void OnValidate()
        {
            IReadOnlyList<string> errors = Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                Debug.LogError($"[MissionPresetAsset] {name}: {errors[index]}");
            }
        }

        private static bool TryCreateCondition(
            MissionPresetEntry entry,
            out MissionCondition condition,
            out string errorMessage)
        {
            condition = default;
            errorMessage = null;
            if (!Enum.IsDefined(typeof(MissionKind), entry.Kind))
            {
                errorMessage = $"MissionKind is undefined: {entry.Kind}.";
                return false;
            }

            try
            {
                if (MissionCondition.IsNoParameterKind(entry.Kind))
                {
                    if (entry.ParameterA != 0 || entry.ParameterB != 0)
                    {
                        errorMessage = "A parameterless MissionKind cannot contain parameters.";
                        return false;
                    }

                    condition = MissionCondition.Create(entry.Kind, entry.IsNegated);
                    return true;
                }

                if (entry.Kind == MissionKind.Between)
                {
                    if (entry.ParameterB < 1)
                    {
                        errorMessage = "Between width must be at least one.";
                        return false;
                    }

                    condition = MissionCondition.Between(
                        entry.ParameterA,
                        entry.ParameterB,
                        entry.IsNegated);
                    return true;
                }

                if (entry.Kind == MissionKind.Modulo)
                {
                    condition = MissionCondition.Modulo(
                        entry.ParameterA,
                        entry.ParameterB,
                        entry.IsNegated);
                    return true;
                }

                if (MissionCondition.IsSingleParameterKind(entry.Kind))
                {
                    if (entry.ParameterB != 0)
                    {
                        errorMessage = "A single-parameter MissionKind cannot contain ParameterB.";
                        return false;
                    }

                    condition = MissionCondition.Create(
                        entry.Kind,
                        entry.ParameterA,
                        entry.IsNegated);
                    return true;
                }

                errorMessage = $"MissionKind.{entry.Kind} is not supported by the runtime condition factory.";
                return false;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }
    }

    /// <summary>
    /// 1件分のMission条件とTierを保持するAssetデータです。
    /// </summary>
    [Serializable]
    public sealed class MissionPresetEntry
    {
        [SerializeField]
        private MissionKind kind;

        [SerializeField]
        private int parameterA;

        [SerializeField]
        private int parameterB;

        [SerializeField]
        private bool isNegated;

        [SerializeField]
        private int tier = 1;

        internal MissionKind Kind => kind;
        internal int ParameterA => parameterA;
        internal int ParameterB => parameterB;
        internal bool IsNegated => isNegated;
        internal int Tier => tier;
    }
}
