#nullable enable
using System;
using System.Collections.Generic;
using Ccc.Core.Model;

namespace Ccc.Simulation
{
    /// <summary>
    /// Kind、パラメータA/B、否定の有無でMission条件を指定し、Catalog内の候補を特定する値です。
    /// </summary>
    public readonly struct MissionConditionSpec
    {
        /// <summary>
        /// 指定する条件のMissionKindを取得します。
        /// </summary>
        public MissionKind Kind { get; }

        /// <summary>
        /// 指定する条件のパラメータAを取得します。パラメータを持たないKindでは0です。
        /// </summary>
        public int ParameterA { get; }

        /// <summary>
        /// 指定する条件のパラメータBを取得します。2つ目のパラメータを持たないKindでは0です。
        /// </summary>
        public int ParameterB { get; }

        /// <summary>
        /// 指定する条件が否定条件かどうかを取得します。
        /// </summary>
        public bool IsNegated { get; }

        /// <summary>
        /// 条件の各要素から指定値を生成します。
        /// </summary>
        /// <param name="kind">MissionKind。</param>
        /// <param name="parameterA">パラメータA。</param>
        /// <param name="parameterB">パラメータB。</param>
        /// <param name="isNegated">否定条件かどうか。</param>
        public MissionConditionSpec(MissionKind kind, int parameterA, int parameterB, bool isNegated)
        {
            Kind = kind;
            ParameterA = parameterA;
            ParameterB = parameterB;
            IsNegated = isNegated;
        }

        /// <summary>
        /// 指定値と一致する条件を持つ候補をCatalogから探します。
        /// </summary>
        /// <param name="candidates">検索するCatalog候補一覧。</param>
        /// <returns>一致した候補。</returns>
        /// <exception cref="ArgumentNullException">候補一覧がnullの場合。</exception>
        /// <exception cref="ArgumentException">一致する候補がCatalogに存在しない場合。</exception>
        public MissionCandidate FindCandidate(IReadOnlyList<MissionCandidate> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            for (int index = 0; index < candidates.Count; index++)
            {
                MissionCondition condition = candidates[index].Condition;
                if (condition.Kind == Kind &&
                    condition.ParameterA == ParameterA &&
                    condition.ParameterB == ParameterB &&
                    condition.IsNegated == IsNegated)
                {
                    return candidates[index];
                }
            }

            throw new ArgumentException(
                $"Mission condition {Kind}({ParameterA}, {ParameterB}, Negated={IsNegated}) is not in the catalog.",
                nameof(candidates));
        }
    }
}
