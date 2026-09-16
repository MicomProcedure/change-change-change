#if UNITY_EDITOR
using System;
using Ccc.Core.Model;
using Ccc.Infrastructure.Assets;
using UnityEditor;
using UnityEngine;

namespace Ccc.Editor
{
    /// <summary>
    /// MissionKindのパラメータ構造に応じて、生成Profileの入力欄を出し分けるInspectorです。
    /// </summary>
    [CustomEditor(typeof(MissionParameterGenerationProfileAsset))]
    internal sealed class MissionParameterGenerationProfileAssetEditor : UnityEditor.Editor
    {
        private const int ExplicitValuesMode = 0;
        private const int InclusiveRangeMode = 1;

        private const string ScriptPropertyName = "m_Script";
        private const string MaximumTotalCandidateCountPropertyName =
            "maximumTotalCandidateCount";
        private const string BetweenTierTwoThresholdPropertyName =
            "betweenTierTwoThreshold";
        private const string KindRulesPropertyName = "kindRules";
        private const string KindPropertyName = "kind";
        private const string IsEnabledPropertyName = "isEnabled";
        private const string AppliesMatchRatioRangePropertyName =
            "appliesMatchRatioRange";
        private const string GeneratesNegatedPropertyName = "generatesNegated";
        private const string MaximumCandidateCountPropertyName =
            "maximumCandidateCount";
        private const string ParameterAPropertyName = "parameterA";
        private const string ParameterBPropertyName = "parameterB";
        private const string IsConfiguredPropertyName = "isConfigured";
        private const string ModePropertyName = "mode";
        private const string ExplicitValuesPropertyName = "explicitValues";
        private const string MinimumPropertyName = "minimum";
        private const string MaximumPropertyName = "maximum";
        private const string StepPropertyName = "step";

        private SerializedProperty _scriptProperty;
        private SerializedProperty _maximumTotalCandidateCountProperty;
        private SerializedProperty _betweenTierTwoThresholdProperty;
        private SerializedProperty _kindRulesProperty;

        private void OnEnable()
        {
            _scriptProperty = serializedObject.FindProperty(ScriptPropertyName);
            _maximumTotalCandidateCountProperty =
                serializedObject.FindProperty(MaximumTotalCandidateCountPropertyName);
            _betweenTierTwoThresholdProperty =
                serializedObject.FindProperty(BetweenTierTwoThresholdPropertyName);
            _kindRulesProperty = serializedObject.FindProperty(KindRulesPropertyName);
        }

        /// <summary>
        /// MissionKindの固定定義に従って、必要なパラメータ入力欄だけを描画します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_scriptProperty);
            }

            EditorGUILayout.PropertyField(_maximumTotalCandidateCountProperty);
            EditorGUILayout.PropertyField(_betweenTierTwoThresholdProperty);
            DrawKindRules();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawKindRules()
        {
            _kindRulesProperty.isExpanded = EditorGUILayout.Foldout(
                _kindRulesProperty.isExpanded,
                _kindRulesProperty.displayName,
                true);
            if (!_kindRulesProperty.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            int requestedSize = EditorGUILayout.DelayedIntField(
                "Size",
                _kindRulesProperty.arraySize);
            if (requestedSize < 0)
            {
                EditorGUILayout.HelpBox(
                    "Kind RulesのSizeには0以上を指定してください。",
                    MessageType.Error);
            }
            else if (requestedSize != _kindRulesProperty.arraySize)
            {
                _kindRulesProperty.arraySize = requestedSize;
            }

            for (int index = 0; index < _kindRulesProperty.arraySize; index++)
            {
                DrawKindRule(_kindRulesProperty.GetArrayElementAtIndex(index), index);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawKindRule(SerializedProperty ruleProperty, int index)
        {
            SerializedProperty kindProperty =
                ruleProperty.FindPropertyRelative(KindPropertyName);
            string label = CreateRuleLabel(kindProperty, index);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                ruleProperty.isExpanded = EditorGUILayout.Foldout(
                    ruleProperty.isExpanded,
                    label,
                    true);
                if (!ruleProperty.isExpanded)
                {
                    return;
                }

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(kindProperty);
                EditorGUILayout.PropertyField(
                    ruleProperty.FindPropertyRelative(IsEnabledPropertyName));
                EditorGUILayout.PropertyField(
                    ruleProperty.FindPropertyRelative(
                        AppliesMatchRatioRangePropertyName));
                EditorGUILayout.PropertyField(
                    ruleProperty.FindPropertyRelative(GeneratesNegatedPropertyName));
                EditorGUILayout.PropertyField(
                    ruleProperty.FindPropertyRelative(
                        MaximumCandidateCountPropertyName));

                if (TryGetParameterStructure(
                        kindProperty,
                        out MissionParameterStructure parameterStructure))
                {
                    SerializedProperty parameterAProperty =
                        ruleProperty.FindPropertyRelative(ParameterAPropertyName);
                    SerializedProperty parameterBProperty =
                        ruleProperty.FindPropertyRelative(ParameterBPropertyName);
                    SynchronizeConfiguration(
                        parameterAProperty,
                        parameterBProperty,
                        parameterStructure);
                    DrawParameters(
                        parameterAProperty,
                        parameterBProperty,
                        parameterStructure);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "未定義のMissionKindです。パラメータ構造を判定できません。",
                        MessageType.Error);
                }

                EditorGUI.indentLevel--;
            }
        }

        private static string CreateRuleLabel(
            SerializedProperty kindProperty,
            int index)
        {
            if (!Enum.IsDefined(typeof(MissionKind), kindProperty.intValue))
            {
                return $"{index}: Undefined MissionKind ({kindProperty.intValue})";
            }

            return $"{index}: {(MissionKind)kindProperty.intValue}";
        }

        private static bool TryGetParameterStructure(
            SerializedProperty kindProperty,
            out MissionParameterStructure parameterStructure)
        {
            if (!Enum.IsDefined(typeof(MissionKind), kindProperty.intValue))
            {
                parameterStructure = default;
                return false;
            }

            MissionKind kind = (MissionKind)kindProperty.intValue;
            parameterStructure =
                MissionKindDefinitionCatalog.Get(kind).ParameterStructure;
            return true;
        }

        private static void SynchronizeConfiguration(
            SerializedProperty parameterAProperty,
            SerializedProperty parameterBProperty,
            MissionParameterStructure parameterStructure)
        {
            bool requiresParameterA =
                parameterStructure != MissionParameterStructure.None;
            bool requiresParameterB =
                parameterStructure == MissionParameterStructure.Pair;

            parameterAProperty
                .FindPropertyRelative(IsConfiguredPropertyName)
                .boolValue = requiresParameterA;
            parameterBProperty
                .FindPropertyRelative(IsConfiguredPropertyName)
                .boolValue = requiresParameterB;
        }

        private static void DrawParameters(
            SerializedProperty parameterAProperty,
            SerializedProperty parameterBProperty,
            MissionParameterStructure parameterStructure)
        {
            switch (parameterStructure)
            {
                case MissionParameterStructure.None:
                    return;
                case MissionParameterStructure.Single:
                    DrawParameterDomain(parameterAProperty, "Parameter A");
                    return;
                case MissionParameterStructure.Pair:
                    DrawParameterDomain(parameterAProperty, "Parameter A");
                    DrawParameterDomain(parameterBProperty, "Parameter B");
                    return;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(parameterStructure),
                        parameterStructure,
                        null);
            }
        }

        private static void DrawParameterDomain(
            SerializedProperty parameterProperty,
            string label)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            SerializedProperty modeProperty =
                parameterProperty.FindPropertyRelative(ModePropertyName);
            EditorGUILayout.PropertyField(modeProperty);

            switch (modeProperty.intValue)
            {
                case ExplicitValuesMode:
                    EditorGUILayout.PropertyField(
                        parameterProperty.FindPropertyRelative(
                            ExplicitValuesPropertyName),
                        true);
                    break;
                case InclusiveRangeMode:
                    EditorGUILayout.PropertyField(
                        parameterProperty.FindPropertyRelative(MinimumPropertyName));
                    EditorGUILayout.PropertyField(
                        parameterProperty.FindPropertyRelative(MaximumPropertyName));
                    EditorGUILayout.PropertyField(
                        parameterProperty.FindPropertyRelative(StepPropertyName));
                    break;
                default:
                    EditorGUILayout.HelpBox(
                        "未定義のParameter Domain Modeです。",
                        MessageType.Error);
                    break;
            }

            EditorGUI.indentLevel--;
        }
    }
}
#endif
