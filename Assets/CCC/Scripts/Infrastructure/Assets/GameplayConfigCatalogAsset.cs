using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Presentation.Navigation;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// GameModeとGameDifficultyからGameplayConfigEntryを解決するCatalogです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Gameplay Config Catalog")]
    public sealed class GameplayConfigCatalogAsset : ScriptableObject
    {
        [SerializeField]
        private List<GameplayConfigEntry> entries = new List<GameplayConfigEntry>();

        [SerializeField]
        private GameplayConfigEntry defaultEntry;

        /// <summary>
        /// Catalogに登録されたEntry一覧を取得します。
        /// </summary>
        public IReadOnlyList<GameplayConfigEntry> Entries
        {
            get { return entries ?? (IReadOnlyList<GameplayConfigEntry>)Array.Empty<GameplayConfigEntry>(); }
        }

        /// <summary>
        /// GameSceneを直接起動した場合に使用する既定Entryを取得します。
        /// </summary>
        public GameplayConfigEntry DefaultEntry
        {
            get { return defaultEntry; }
        }

        /// <summary>
        /// ModeとDifficultyに対応するEntryを検索します。
        /// </summary>
        /// <param name="gameMode">検索するゲームモード。</param>
        /// <param name="difficulty">検索するゲーム難易度。</param>
        /// <param name="entry">見つかったEntry。</param>
        /// <returns>対応するEntryが存在する場合はtrue。</returns>
        public bool TryGetEntry(
            GameMode gameMode,
            GameDifficulty difficulty,
            out GameplayConfigEntry entry)
        {
            entry = entries == null
                ? null
                : entries.FirstOrDefault(candidate =>
                    candidate != null &&
                    candidate.GameMode == gameMode &&
                    candidate.Difficulty == difficulty);
            return entry != null;
        }

        /// <summary>
        /// Navigation Requestに対応するEntryを検索します。
        /// </summary>
        /// <param name="request">検索対象のGameplay開始Request。</param>
        /// <param name="entry">見つかったEntry。</param>
        /// <returns>対応するEntryが存在する場合はtrue。</returns>
        public bool TryGetEntry(
            GameplayNavigationRequest request,
            out GameplayConfigEntry entry)
        {
            if (request == null)
            {
                entry = null;
                return false;
            }

            return TryGetEntry(request.GameMode, request.Difficulty, out entry);
        }

        /// <summary>
        /// 既定EntryからGameScene直起動用のRequestを生成します。
        /// </summary>
        /// <returns>既定Entryに対応するGameplay開始Request。</returns>
        /// <exception cref="InvalidOperationException">既定Entryが未設定の場合。</exception>
        public GameplayNavigationRequest CreateDefaultRequest()
        {
            if (defaultEntry == null)
            {
                throw new InvalidOperationException(
                    $"Gameplay config catalog {name} has no default entry.");
            }

            return defaultEntry.CreateDefaultRequest();
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
                errors.Add("Gameplay config catalog entries are missing.");
            }
            else
            {
                var registeredPairs = new HashSet<(GameMode, GameDifficulty)>();
                for (int index = 0; index < entries.Count; index++)
                {
                    GameplayConfigEntry entry = entries[index];
                    if (entry == null)
                    {
                        errors.Add($"Gameplay config catalog entry at index {index} is null.");
                        continue;
                    }

                    IReadOnlyList<string> entryErrors = entry.Validate();
                    for (int errorIndex = 0; errorIndex < entryErrors.Count; errorIndex++)
                    {
                        errors.Add(
                            $"Entry {entry.DisplayName}: {entryErrors[errorIndex]}");
                    }

                    if (!registeredPairs.Add((entry.GameMode, entry.Difficulty)))
                    {
                        errors.Add(
                            $"GameMode.{entry.GameMode} and GameDifficulty.{entry.Difficulty} are duplicated.");
                    }
                }
            }

            if (defaultEntry == null)
            {
                errors.Add("Gameplay config catalog default entry is missing.");
            }
            else if (entries == null || !entries.Contains(defaultEntry))
            {
                errors.Add("Gameplay config catalog default entry is not registered in entries.");
            }

            return errors.Distinct().ToArray();
        }

        private void OnEnable()
        {
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
                Debug.LogError($"[GameplayConfigCatalogAsset] {name}: {errors[index]}");
            }
        }
    }
}
