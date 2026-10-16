using System;
using System.Collections.Generic;
using System.Text;
using Ccc.Infrastructure.Assets;
using Ccc.Infrastructure.Logging;
using Ccc.Simulation;
using UnityEditor;
using UnityEngine;

namespace Ccc.Editor.Simulation
{
    /// <summary>
    /// 設定Assetを指定するか、Window上で直接入力した設定でMission抽選シミュレーションを実行するEditorWindowです。
    /// </summary>
    public sealed class MissionDrawSimulationWindow : EditorWindow
    {
        private const string MenuPath = "CCC/Mission Draw Simulation";
        private const string WindowTitle = "Mission Draw Simulation";
        private const string SettingsAssetLabel = "Settings Asset";
        private const string DirectInputMessage =
            "Settings Asset is not assigned. The values below are used for this run and can be saved as an asset.";
        private const string SaveButtonLabel = "Save As Asset";
        private const string RunButtonLabel = "Run";
        private const string SavePanelTitle = "Save Mission Draw Simulation Settings";
        private const string DefaultAssetName = "MissionDrawSimulationSettings";
        private const string AssetExtension = "asset";
        private const string LogPrefix = "[MissionDrawSimulation] ";

        [SerializeField]
        private MissionDrawSimulationSettingsAsset settingsAsset;

        private MissionDrawSimulationSettingsAsset _directInputSettings;
        private UnityEditor.Editor _settingsEditor;
        private Vector2 _scrollPosition;
        private string _lastResultMessage;
        private MessageType _lastResultMessageType;

        /// <summary>
        /// シミュレーションWindowを開きます。
        /// </summary>
        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<MissionDrawSimulationWindow>(WindowTitle);
        }

        private void OnDisable()
        {
            DestroyImmediate(_settingsEditor);
            _settingsEditor = null;
            DestroyImmediate(_directInputSettings);
            _directInputSettings = null;
        }

        private void OnGUI()
        {
            settingsAsset = (MissionDrawSimulationSettingsAsset)EditorGUILayout.ObjectField(
                SettingsAssetLabel,
                settingsAsset,
                typeof(MissionDrawSimulationSettingsAsset),
                false);
            MissionDrawSimulationSettingsAsset activeSettings =
                settingsAsset != null ? settingsAsset : GetDirectInputSettings();
            if (settingsAsset == null)
            {
                EditorGUILayout.HelpBox(DirectInputMessage, MessageType.Info);
            }

            UnityEditor.Editor.CreateCachedEditor(activeSettings, null, ref _settingsEditor);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            _settingsEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (settingsAsset == null && GUILayout.Button(SaveButtonLabel))
                {
                    SaveDirectInputSettings();
                }

                if (GUILayout.Button(RunButtonLabel))
                {
                    Run(activeSettings);
                }
            }

            if (!string.IsNullOrEmpty(_lastResultMessage))
            {
                EditorGUILayout.HelpBox(_lastResultMessage, _lastResultMessageType);
            }
        }

        private MissionDrawSimulationSettingsAsset GetDirectInputSettings()
        {
            if (_directInputSettings != null)
            {
                return _directInputSettings;
            }

            _directInputSettings = CreateInstance<MissionDrawSimulationSettingsAsset>();
            _directInputSettings.hideFlags = HideFlags.DontSave;
            _directInputSettings.AssignMissingReferences(
                FindFirstAsset<GameplayConfigEntry>(),
                FindFirstAsset<LogSettingsAsset>());
            return _directInputSettings;
        }

        private void SaveDirectInputSettings()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                SavePanelTitle,
                DefaultAssetName,
                AssetExtension,
                string.Empty);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            MissionDrawSimulationSettingsAsset savedSettings = Instantiate(_directInputSettings);
            savedSettings.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(savedSettings, path);
            AssetDatabase.SaveAssets();
            settingsAsset = savedSettings;
        }

        private void Run(MissionDrawSimulationSettingsAsset settings)
        {
            try
            {
                (MissionDrawSimulationSummary summary, string outputDirectoryPath) =
                    MissionDrawSimulationExecutor.Execute(settings);
                _lastResultMessage = FormatSummary(summary, outputDirectoryPath);
                _lastResultMessageType = summary.FailureCount == 0 ? MessageType.Info : MessageType.Warning;
                Debug.Log(LogPrefix + _lastResultMessage);
            }
            catch (Exception exception)
            {
                _lastResultMessage = exception.Message;
                _lastResultMessageType = MessageType.Error;
                Debug.LogException(exception);
            }
        }

        private static string FormatSummary(MissionDrawSimulationSummary summary, string outputDirectoryPath)
        {
            var message = new StringBuilder();
            message.Append("Draws=").Append(summary.DrawCount)
                .Append(", Failures=").Append(summary.FailureCount)
                .AppendLine()
                .Append("Output: ").Append(outputDirectoryPath);
            foreach (KeyValuePair<string, int> failure in summary.FailureCounts)
            {
                message.AppendLine().Append(failure.Value).Append("x ").Append(failure.Key);
            }

            return message.ToString();
        }

        private static T FindFirstAsset<T>()
            where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            return guids.Length == 0
                ? null
                : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
