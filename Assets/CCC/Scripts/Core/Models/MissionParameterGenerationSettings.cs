using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ccc.Core.Model
{
    /// <summary>
    /// Missionパラメータ自動生成に使用する検証済み設定を保持します。
    /// </summary>
    public sealed class MissionParameterGenerationSettings
    {
        private readonly IReadOnlyList<MissionKindGenerationRule> _rules;
        private readonly IReadOnlyDictionary<MissionKind, MissionKindGenerationRule> _rulesByKind;

        /// <summary>MissionKind数値順に並んだ生成規則を取得します。</summary>
        public IReadOnlyList<MissionKindGenerationRule> Rules => _rules;

        /// <summary>全Kind合計の候補上限を取得します。</summary>
        public int MaximumTotalCandidateCount { get; }

        /// <summary>Between肯定候補をTier 2とする適合率閾値を取得します。</summary>
        public Rational BetweenTierTwoThreshold { get; }

        /// <summary>
        /// 検証済み生成規則と全体上限から設定を生成します。
        /// </summary>
        /// <param name="rules">Kind別生成規則。</param>
        /// <param name="maximumTotalCandidateCount">全体候補上限。</param>
        /// <param name="betweenTierTwoThreshold">Between Tier判定閾値。</param>
        public MissionParameterGenerationSettings(
            IReadOnlyList<MissionKindGenerationRule> rules,
            int maximumTotalCandidateCount,
            Rational betweenTierTwoThreshold)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (maximumTotalCandidateCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumTotalCandidateCount),
                    maximumTotalCandidateCount,
                    null);
            }

            Rational zero = Rational.Create(0, 1);
            Rational one = Rational.Create(1, 1);
            if (betweenTierTwoThreshold.Denominator <= 0 ||
                betweenTierTwoThreshold < zero ||
                betweenTierTwoThreshold > one)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(betweenTierTwoThreshold),
                    betweenTierTwoThreshold,
                    null);
            }

            var byKind = new Dictionary<MissionKind, MissionKindGenerationRule>();
            for (int index = 0; index < rules.Count; index++)
            {
                MissionKindGenerationRule rule = rules[index];
                if (rule == null)
                {
                    throw new ArgumentException("Rules cannot contain null.", nameof(rules));
                }

                if (byKind.ContainsKey(rule.Kind))
                {
                    throw new ArgumentException(
                        $"MissionKind.{rule.Kind} is duplicated.",
                        nameof(rules));
                }

                byKind.Add(rule.Kind, rule);
            }

            if (byKind.Values.All(rule => !rule.IsEnabled))
            {
                throw new ArgumentException(
                    "At least one MissionKind must be enabled.",
                    nameof(rules));
            }

            MissionKindGenerationRule[] sortedRules = byKind.Values
                .OrderBy(rule => rule.Kind)
                .ToArray();
            _rules = new ReadOnlyCollection<MissionKindGenerationRule>(sortedRules);
            _rulesByKind = new ReadOnlyDictionary<MissionKind, MissionKindGenerationRule>(byKind);
            MaximumTotalCandidateCount = maximumTotalCandidateCount;
            BetweenTierTwoThreshold = betweenTierTwoThreshold;
        }

        /// <summary>
        /// 指定されたMissionKindの生成規則を取得します。
        /// </summary>
        /// <param name="kind">取得対象のMissionKind。</param>
        /// <returns>登録済み生成規則。</returns>
        /// <exception cref="KeyNotFoundException">規則が未登録の場合。</exception>
        public MissionKindGenerationRule GetRule(MissionKind kind)
        {
            if (!_rulesByKind.TryGetValue(kind, out MissionKindGenerationRule rule))
            {
                throw new KeyNotFoundException(
                    $"MissionKind.{kind} has no generation rule.");
            }

            return rule;
        }

        /// <summary>
        /// 指定されたMissionKindの生成規則を検索します。
        /// </summary>
        /// <param name="kind">検索対象のMissionKind。</param>
        /// <param name="rule">見つかった生成規則。</param>
        /// <returns>登録済みの場合はtrue。</returns>
        public bool TryGetRule(
            MissionKind kind,
            out MissionKindGenerationRule rule)
        {
            MissionKindDefinitionCatalog.Get(kind);
            return _rulesByKind.TryGetValue(kind, out rule);
        }
    }
}
