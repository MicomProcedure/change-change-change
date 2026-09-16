using System;

namespace Ccc.Core.Model
{
    /// <summary>
    /// 1つのMissionKindに対する調整可能な候補生成規則を表します。
    /// </summary>
    public sealed class MissionKindGenerationRule
    {
        /// <summary>対象MissionKindを取得します。</summary>
        public MissionKind Kind { get; }

        /// <summary>候補生成を有効にするかを取得します。</summary>
        public bool IsEnabled { get; }

        /// <summary>Catalog採用時にグローバル適合率範囲を適用するかを取得します。</summary>
        public bool AppliesMatchRatioRange { get; }

        /// <summary>肯定候補に続けてNegated候補を生成するかを取得します。</summary>
        public bool GeneratesNegated { get; }

        /// <summary>このKindに許容する生成候補数上限を取得します。</summary>
        public int MaximumCandidateCount { get; }

        /// <summary>ParameterAのドメインを取得します。不要な構造ではnullです。</summary>
        public IntegerParameterDomain ParameterADomain { get; }

        /// <summary>ParameterBのドメインを取得します。不要な構造ではnullです。</summary>
        public IntegerParameterDomain ParameterBDomain { get; }

        /// <summary>
        /// MissionKind別の候補生成規則を生成します。
        /// </summary>
        /// <param name="kind">対象MissionKind。</param>
        /// <param name="isEnabled">候補生成を有効にするか。</param>
        /// <param name="appliesMatchRatioRange">適合率範囲を適用するか。</param>
        /// <param name="generatesNegated">Negated候補を生成するか。</param>
        /// <param name="maximumCandidateCount">Negated展開後の候補上限。</param>
        /// <param name="parameterADomain">ParameterAドメイン。</param>
        /// <param name="parameterBDomain">ParameterBドメイン。</param>
        public MissionKindGenerationRule(
            MissionKind kind,
            bool isEnabled,
            bool appliesMatchRatioRange,
            bool generatesNegated,
            int maximumCandidateCount,
            IntegerParameterDomain parameterADomain,
            IntegerParameterDomain parameterBDomain)
        {
            MissionKindDefinition definition = MissionKindDefinitionCatalog.Get(kind);
            if (maximumCandidateCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumCandidateCount),
                    maximumCandidateCount,
                    null);
            }

            if (generatesNegated && !definition.CanGenerateNegated)
            {
                throw new ArgumentException(
                    $"MissionKind.{kind} does not allow Negated generation.",
                    nameof(generatesNegated));
            }

            ValidateDomains(definition.ParameterStructure, parameterADomain, parameterBDomain);
            Kind = kind;
            IsEnabled = isEnabled;
            AppliesMatchRatioRange = appliesMatchRatioRange;
            GeneratesNegated = generatesNegated;
            MaximumCandidateCount = maximumCandidateCount;
            ParameterADomain = parameterADomain;
            ParameterBDomain = parameterBDomain;
        }

        private static void ValidateDomains(
            MissionParameterStructure parameterStructure,
            IntegerParameterDomain parameterADomain,
            IntegerParameterDomain parameterBDomain)
        {
            bool isValid;
            switch (parameterStructure)
            {
                case MissionParameterStructure.None:
                    isValid = parameterADomain == null && parameterBDomain == null;
                    break;
                case MissionParameterStructure.Single:
                    isValid = parameterADomain != null && parameterBDomain == null;
                    break;
                case MissionParameterStructure.Pair:
                    isValid = parameterADomain != null && parameterBDomain != null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameterStructure),
                        parameterStructure,
                        null);
            }

            if (!isValid)
            {
                throw new ArgumentException(
                    $"Parameter domains do not match {parameterStructure} structure.",
                    nameof(parameterADomain));
            }
        }
    }
}
