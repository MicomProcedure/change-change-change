using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ccc.Core.Model;
using Ccc.Presentation;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// ミッション表示文言をUnity Editorで編集するScriptableObjectです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Mission Text")]
    public sealed class MissionTextAsset : ScriptableObject, IMissionTextSource
    {
        [SerializeField]
        private List<MissionTextEntry> entries = new List<MissionTextEntry>();

        [NonSerialized]
        private HashSet<string> _reportedFallbacks;

        /// <summary>
        /// 指定されたミッション種類の表示テンプレートを取得します。
        /// 否定用テンプレートが未設定の場合は、正条件用テンプレートを使用します。
        /// </summary>
        /// <param name="kind">ミッションの種類。</param>
        /// <param name="isNegated">否定条件用のテンプレートを取得するかどうか。</param>
        /// <returns>表示テンプレート。</returns>
        /// <exception cref="ArgumentOutOfRangeException">未定義のミッション種類の場合。</exception>
        public string GetTemplate(MissionKind kind, bool isNegated)
        {
            if (!Enum.IsDefined(typeof(MissionKind), kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            MissionTextEntry entry = entries == null
                ? null
                : entries.FirstOrDefault(candidate => candidate != null && candidate.Kind == kind);
            if (entry == null)
            {
                return CreateFallbackTemplate(
                    kind,
                    isNegated,
                    "Mission text entry is missing.");
            }

            string template = isNegated && !string.IsNullOrWhiteSpace(entry.NegatedTemplate)
                ? entry.NegatedTemplate
                : entry.PositiveTemplate;
            if (string.IsNullOrWhiteSpace(template))
            {
                return CreateFallbackTemplate(
                    kind,
                    isNegated,
                    "Mission text entry has no usable template.");
            }

            var templateErrors = new List<string>();
            ValidateTemplate(
                templateErrors,
                kind,
                template,
                isNegated ? "negatedTemplate" : "positiveTemplate",
                isRequired: true);
            if (templateErrors.Count > 0)
            {
                return CreateFallbackTemplate(
                    kind,
                    isNegated,
                    templateErrors[0]);
            }

            return template;
        }

        /// <summary>
        /// Assetの文言定義を検証し、エラー一覧を返します。
        /// </summary>
        /// <returns>検出された検証エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            var registeredKinds = new HashSet<MissionKind>();

            if (entries == null)
            {
                errors.Add("Mission text asset entries are missing.");
                return errors.ToArray();
            }

            for (int index = 0; index < entries.Count; index++)
            {
                MissionTextEntry entry = entries[index];
                if (entry == null)
                {
                    errors.Add($"Mission text entry at index {index} is null.");
                    continue;
                }

                if (!Enum.IsDefined(typeof(MissionKind), entry.Kind))
                {
                    errors.Add(
                        $"Mission text entry at index {index} has an undefined MissionKind value: {entry.Kind}.");
                    continue;
                }

                if (!registeredKinds.Add(entry.Kind))
                {
                    errors.Add($"MissionKind.{entry.Kind} is duplicated in the mission text asset.");
                }

                ValidateTemplate(
                    errors,
                    entry.Kind,
                    entry.PositiveTemplate,
                    "positiveTemplate",
                    isRequired: true);
                ValidateTemplate(
                    errors,
                    entry.Kind,
                    entry.NegatedTemplate,
                    "negatedTemplate",
                    isRequired: false);
            }

            foreach (MissionKind kind in Enum.GetValues(typeof(MissionKind)))
            {
                if (!registeredKinds.Contains(kind))
                {
                    errors.Add($"MissionKind.{kind} is missing from the mission text asset.");
                }
            }

            return errors.Distinct().ToArray();
        }

        private void OnEnable()
        {
            _reportedFallbacks = new HashSet<string>();
            LogValidationErrors();
        }

        private void OnValidate()
        {
            LogValidationErrors();
        }

        private void LogValidationErrors()
        {
            IReadOnlyList<string> errors = Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                Debug.LogWarning($"[MissionTextAsset] {name}: {errors[index]}");
            }
        }

        private string CreateFallbackTemplate(
            MissionKind kind,
            bool isNegated,
            string reason)
        {
            if (_reportedFallbacks == null)
            {
                _reportedFallbacks = new HashSet<string>();
            }

            string fallbackKey = $"{kind}:{isNegated}:{reason}";
            if (_reportedFallbacks.Add(fallbackKey))
            {
                Debug.LogWarning(
                    $"[MissionTextAsset] {name}: {reason} " +
                    $"Using fallback text for MissionKind.{kind}.", this);
            }

            return $"MissionKind.{kind}";
        }

        private static void ValidateTemplate(
            ICollection<string> errors,
            MissionKind kind,
            string template,
            string templateName,
            bool isRequired)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                if (isRequired)
                {
                    errors.Add($"MissionKind.{kind} {templateName} is empty.");
                }

                return;
            }

            var placeholderIndices = new List<int>();
            if (!TryReadPlaceholderIndices(template, placeholderIndices))
            {
                errors.Add($"MissionKind.{kind} {templateName} contains an invalid placeholder.");
                return;
            }

            var requiredIndices = GetRequiredPlaceholderIndices(kind);
            foreach (int placeholderIndex in placeholderIndices.Distinct())
            {
                if (!requiredIndices.Contains(placeholderIndex))
                {
                    errors.Add(
                        $"MissionKind.{kind} {templateName} contains an invalid placeholder number: {{{placeholderIndex}}}.");
                }
            }

            foreach (int requiredIndex in requiredIndices)
            {
                if (!placeholderIndices.Contains(requiredIndex))
                {
                    errors.Add(
                        $"MissionKind.{kind} {templateName} is missing placeholder {{{requiredIndex}}}.");
                }
            }

            try
            {
                _ = string.Format(CultureInfo.InvariantCulture, template, 0, 0);
            }
            catch (FormatException)
            {
                errors.Add($"MissionKind.{kind} {templateName} is not a valid format string.");
            }
        }

        private static HashSet<int> GetRequiredPlaceholderIndices(MissionKind kind)
        {
            if (kind == MissionKind.Between || kind == MissionKind.Modulo)
            {
                return new HashSet<int> { 0, 1 };
            }

            if (MissionCondition.IsSingleParameterKind(kind))
            {
                return new HashSet<int> { 0 };
            }

            return new HashSet<int>();
        }

        private static bool TryReadPlaceholderIndices(
            string template,
            ICollection<int> placeholderIndices)
        {
            for (int index = 0; index < template.Length;)
            {
                char current = template[index];
                if (current == '{')
                {
                    if (index + 1 < template.Length && template[index + 1] == '{')
                    {
                        index += 2;
                        continue;
                    }

                    int closingBraceIndex = template.IndexOf('}', index + 1);
                    if (closingBraceIndex < 0)
                    {
                        return false;
                    }

                    string formatItem = template.Substring(
                        index + 1,
                        closingBraceIndex - index - 1);
                    int separatorIndex = FindFormatItemSeparator(formatItem);
                    string indexText = separatorIndex < 0
                        ? formatItem
                        : formatItem.Substring(0, separatorIndex);
                    if (!int.TryParse(
                            indexText.Trim(),
                            NumberStyles.None,
                            CultureInfo.InvariantCulture,
                            out int placeholderIndex) ||
                        placeholderIndex < 0)
                    {
                        return false;
                    }

                    placeholderIndices.Add(placeholderIndex);
                    index = closingBraceIndex + 1;
                    continue;
                }

                if (current == '}')
                {
                    if (index + 1 < template.Length && template[index + 1] == '}')
                    {
                        index += 2;
                        continue;
                    }

                    return false;
                }

                index++;
            }

            return true;
        }

        private static int FindFormatItemSeparator(string formatItem)
        {
            for (int index = 0; index < formatItem.Length; index++)
            {
                if (formatItem[index] == ',' || formatItem[index] == ':')
                {
                    return index;
                }
            }

            return -1;
        }
    }

    /// <summary>
    /// 1種類のミッション表示テンプレートを保持するAssetデータです。
    /// </summary>
    [Serializable]
    public sealed class MissionTextEntry
    {
        [SerializeField]
        private MissionKind kind;

        [SerializeField]
        private string positiveTemplate;

        [SerializeField]
        private string negatedTemplate;

        internal MissionKind Kind => kind;
        internal string PositiveTemplate => positiveTemplate;
        internal string NegatedTemplate => negatedTemplate;
    }
}
