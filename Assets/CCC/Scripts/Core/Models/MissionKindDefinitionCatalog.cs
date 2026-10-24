using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ccc.Core.Model
{
    /// <summary>
    /// 全MissionKindの固定仕様を一元管理します。
    /// </summary>
    public static class MissionKindDefinitionCatalog
    {
        private static readonly MissionKindDefinition[] DefinitionArray =
        {
            Define(MissionKind.Even, MissionParameterStructure.None, 1, true, 2),
            Define(MissionKind.Odd, MissionParameterStructure.None, 1, true, 2),
            Define(MissionKind.Integer, MissionParameterStructure.None, 1, true, 2),
            Define(MissionKind.Decimal, MissionParameterStructure.None, 1, true, 2),
            Define(MissionKind.Positive, MissionParameterStructure.None, 1, true, 2),
            Define(MissionKind.Negative, MissionParameterStructure.None, 1, true, 2),
            Define(MissionKind.Prime, MissionParameterStructure.None, 2, false, 0),
            Define(MissionKind.Zero, MissionParameterStructure.None, 2, false, 0),
            Define(MissionKind.Square, MissionParameterStructure.None, 2, false, 0),
            Define(MissionKind.Composite, MissionParameterStructure.None, 3, false, 0),
            Define(MissionKind.AtLeast, MissionParameterStructure.Single, 1, true, 3),
            Define(MissionKind.AtMost, MissionParameterStructure.Single, 1, true, 3),
            Define(MissionKind.Exact, MissionParameterStructure.Single, 2, false, 0),
            Define(MissionKind.Multiple, MissionParameterStructure.Single, 2, false, 0),
            Define(MissionKind.Absolute, MissionParameterStructure.Single, 3, false, 0),
            Define(MissionKind.OnesDigit, MissionParameterStructure.Single, 3, false, 0),
            Define(MissionKind.DecimalFirstDigit, MissionParameterStructure.Single, 4, false, 0),
            Define(MissionKind.Between, MissionParameterStructure.Pair, 0, true, 4),
            Define(MissionKind.Modulo, MissionParameterStructure.Pair, 4, false, 0),
            Define(MissionKind.Divisor, MissionParameterStructure.Single, 3, true, 4),
            Define(MissionKind.Power, MissionParameterStructure.Single, 3, true, 4),
            Define(MissionKind.BetweenZeroAndOne, MissionParameterStructure.None, 3, true, 4),
            Define(MissionKind.TwoDigit, MissionParameterStructure.None, 1, true, 2),
            Define(MissionKind.TensDigit, MissionParameterStructure.Single, 2, true, 3),
            Define(MissionKind.DigitSum, MissionParameterStructure.Single, 3, true, 4),
            Define(MissionKind.RepeatingDecimal, MissionParameterStructure.None, 4, true, 4)
        };

        private static readonly IReadOnlyList<MissionKindDefinition> ReadOnlyDefinitions =
            new ReadOnlyCollection<MissionKindDefinition>(DefinitionArray);

        /// <summary>数値順に並んだ全MissionKind定義を取得します。</summary>
        public static IReadOnlyList<MissionKindDefinition> Definitions => ReadOnlyDefinitions;

        /// <summary>
        /// 指定されたMissionKindの固定定義を取得します。
        /// </summary>
        /// <param name="kind">取得対象のMissionKind。</param>
        /// <returns>対応する固定定義。</returns>
        /// <exception cref="ArgumentOutOfRangeException">未定義のMissionKindの場合。</exception>
        public static MissionKindDefinition Get(MissionKind kind)
        {
            if (!Enum.IsDefined(typeof(MissionKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            return DefinitionArray[(int)kind];
        }

        private static MissionKindDefinition Define(
            MissionKind kind,
            MissionParameterStructure parameterStructure,
            int fixedPositiveTier,
            bool canGenerateNegated,
            int negatedTier)
        {
            return new MissionKindDefinition(
                kind,
                parameterStructure,
                fixedPositiveTier,
                canGenerateNegated,
                negatedTier);
        }
    }
}
