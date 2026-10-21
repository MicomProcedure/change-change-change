using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Presentation;
using UnityEngine;

namespace Ccc.Infrastructure.Navigation
{
    /// <summary>
    /// 画面の種類とプラットフォームからロードするScene名を解決するCatalogです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Scene Catalog")]
    public sealed class SceneCatalogAsset : ScriptableObject
    {
        [SerializeField]
        private List<SceneCatalogEntry> entries = new List<SceneCatalogEntry>();

        /// <summary>
        /// 画面の種類とプラットフォームに対応するScene名を取得します。
        /// </summary>
        /// <param name="sceneKind">遷移先となる画面の種類。</param>
        /// <param name="playPlatform">プレイ環境のプラットフォーム。</param>
        /// <returns>ロードするScene名。</returns>
        /// <exception cref="InvalidOperationException">対応するEntryが登録されていない場合。</exception>
        public string GetSceneName(SceneKind sceneKind, PlayPlatform playPlatform)
        {
            SceneCatalogEntry entry = entries?.FirstOrDefault(candidate =>
                candidate != null &&
                candidate.SceneKind == sceneKind &&
                candidate.PlayPlatform == playPlatform);
            if (entry == null)
            {
                throw new InvalidOperationException(
                    $"Scene catalog {name} has no entry for SceneKind.{sceneKind} and PlayPlatform.{playPlatform}.");
            }

            return entry.SceneName;
        }

        /// <summary>
        /// Scene名が単一のプラットフォームにだけ登録されている場合に、そのプラットフォームを取得します。
        /// </summary>
        /// <param name="sceneName">検索するScene名。</param>
        /// <param name="playPlatform">Sceneが属するプラットフォーム。</param>
        /// <returns>Sceneが単一のプラットフォームにだけ登録されている場合はtrue。未登録または複数のプラットフォームで共有されている場合はfalse。</returns>
        public bool TryGetUniquePlayPlatform(string sceneName, out PlayPlatform playPlatform)
        {
            PlayPlatform[] platforms = entries == null
                ? Array.Empty<PlayPlatform>()
                : entries
                    .Where(entry => entry != null && entry.SceneName == sceneName)
                    .Select(entry => entry.PlayPlatform)
                    .Distinct()
                    .ToArray();
            if (platforms.Length == 1)
            {
                playPlatform = platforms[0];
                return true;
            }

            playPlatform = default;
            return false;
        }

        /// <summary>
        /// Catalogの構成を検証し、検出されたエラー一覧を返します。
        /// </summary>
        /// <returns>検出された検証エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (entries == null || entries.Count == 0)
            {
                errors.Add("Scene catalog entries are missing.");
                return errors;
            }

            var registeredPairs = new HashSet<(SceneKind, PlayPlatform)>();
            for (int index = 0; index < entries.Count; index++)
            {
                SceneCatalogEntry entry = entries[index];
                if (entry == null)
                {
                    errors.Add($"Scene catalog entry at index {index} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.SceneName))
                {
                    errors.Add($"Scene catalog entry at index {index} has a blank scene name.");
                }

                if (!registeredPairs.Add((entry.SceneKind, entry.PlayPlatform)))
                {
                    errors.Add(
                        $"SceneKind.{entry.SceneKind} and PlayPlatform.{entry.PlayPlatform} are duplicated.");
                }
            }

            foreach (SceneKind sceneKind in Enum.GetValues(typeof(SceneKind)))
            {
                foreach (PlayPlatform playPlatform in Enum.GetValues(typeof(PlayPlatform)))
                {
                    if (!registeredPairs.Contains((sceneKind, playPlatform)))
                    {
                        errors.Add(
                            $"SceneKind.{sceneKind} and PlayPlatform.{playPlatform} are not registered.");
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// 登録されたSceneがすべてBuild Settingsに含まれているかを検証します。
        /// </summary>
        /// <returns>Build Settingsに含まれていないScene名のエラー一覧。</returns>
        public IReadOnlyList<string> ValidateBuildInclusion()
        {
            var errors = new List<string>();
            if (entries == null)
            {
                return errors;
            }

            IEnumerable<string> sceneNames = entries
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.SceneName))
                .Select(entry => entry.SceneName)
                .Distinct();
            foreach (string sceneName in sceneNames)
            {
                if (!Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    errors.Add($"Scene {sceneName} is not included in the build settings.");
                }
            }

            return errors;
        }

        private void OnValidate()
        {
            IReadOnlyList<string> errors = Validate();
            for (int index = 0; index < errors.Count; index++)
            {
                Debug.LogError($"[SceneCatalogAsset] {name}: {errors[index]}");
            }
        }
    }
}
