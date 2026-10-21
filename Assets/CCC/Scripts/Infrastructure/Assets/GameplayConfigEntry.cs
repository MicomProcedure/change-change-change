using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Presentation.Audio;
using Ccc.Presentation.Navigation;
using UnityEngine;

namespace Ccc.Infrastructure.Assets
{
    /// <summary>
    /// GameMode、GameDifficultyとGameplay関連Assetの対応を保持するエントリです。
    /// </summary>
    [CreateAssetMenu(menuName = "CCC/Gameplay Config Entry")]
    public sealed class GameplayConfigEntry : ScriptableObject
    {
        private const AudioKey DefaultGameplayBgmKey = AudioKey.BgmGameplayNormal;

        [SerializeField]
        private string displayName;

        [SerializeField]
        private GameMode gameMode;

        [SerializeField]
        private GameDifficulty difficulty;

        [SerializeField]
        private GameplayConfigAsset gameplayConfigAsset;

        [SerializeField]
        private MissionParameterSourceMode missionParameterSourceMode =
            MissionParameterSourceMode.Automatic;

        [SerializeField]
        private MissionParameterGenerationProfileAsset missionParameterGenerationProfileAsset;

        [SerializeField]
        private MissionPresetAsset missionPresetAsset;

        [SerializeField]
        private MissionTextAsset missionTextAsset;

        [SerializeField]
        private AudioKey gameplayBgmKey = DefaultGameplayBgmKey;

        /// <summary>
        /// Inspectorやデバッグログで使用する表示名を取得します。
        /// </summary>
        public string DisplayName
        {
            get { return string.IsNullOrWhiteSpace(displayName) ? name : displayName; }
        }

        /// <summary>
        /// 対応するゲームモードを取得します。
        /// </summary>
        public GameMode GameMode
        {
            get { return gameMode; }
        }

        /// <summary>
        /// 対応するゲーム難易度を取得します。
        /// </summary>
        public GameDifficulty Difficulty
        {
            get { return difficulty; }
        }

        /// <summary>
        /// 対応するGameplay設定Assetを取得します。
        /// </summary>
        public GameplayConfigAsset GameplayConfigAsset
        {
            get { return gameplayConfigAsset; }
        }

        /// <summary>
        /// Missionパラメータ候補の取得経路を取得します。
        /// </summary>
        public MissionParameterSourceMode MissionParameterSourceMode
        {
            get { return missionParameterSourceMode; }
        }

        /// <summary>
        /// Missionパラメータ生成・評価Profile Assetを取得します。
        /// </summary>
        public MissionParameterGenerationProfileAsset MissionParameterGenerationProfileAsset
        {
            get { return missionParameterGenerationProfileAsset; }
        }

        /// <summary>
        /// 対応するMission候補Preset Assetを取得します。
        /// </summary>
        public MissionPresetAsset MissionPresetAsset
        {
            get { return missionPresetAsset; }
        }

        /// <summary>
        /// 対応するMission表示文言Assetを取得します。
        /// 未設定の場合はデバッグ用Fallback文言が使用されます。
        /// </summary>
        public MissionTextAsset MissionTextAsset
        {
            get { return missionTextAsset; }
        }

        /// <summary>
        /// Gameplay中に再生するBGMのAudioKeyを取得します。
        /// </summary>
        public AudioKey GameplayBgmKey
        {
            get { return gameplayBgmKey; }
        }

        /// <summary>
        /// Entryの必須設定を検証し、検出されたエラー一覧を返します。
        /// </summary>
        /// <returns>検出された検証エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (!Enum.IsDefined(typeof(GameMode), gameMode))
            {
                errors.Add($"GameMode is undefined: {gameMode}.");
            }

            if (!Enum.IsDefined(typeof(GameDifficulty), difficulty))
            {
                errors.Add($"GameDifficulty is undefined: {difficulty}.");
            }

            if (gameplayConfigAsset == null)
            {
                errors.Add("Gameplay config asset is missing.");
            }

            if (!Enum.IsDefined(
                    typeof(MissionParameterSourceMode),
                    missionParameterSourceMode))
            {
                errors.Add(
                    $"Mission parameter source mode is undefined: {missionParameterSourceMode}.");
            }

            if (missionParameterGenerationProfileAsset == null)
            {
                errors.Add("Mission parameter generation profile asset is missing.");
            }

            if (missionParameterSourceMode == MissionParameterSourceMode.PresetDebug &&
                missionPresetAsset == null)
            {
                errors.Add("PresetDebug requires a Mission preset asset.");
            }

            if (!Enum.IsDefined(typeof(AudioKey), gameplayBgmKey) ||
                gameplayBgmKey == AudioKey.None)
            {
                errors.Add($"Gameplay BGM key is not configured: {gameplayBgmKey}.");
            }

            return errors;
        }

        /// <summary>
        /// Missionパラメータ取得経路に応じて、Profile AssetとPreset AssetからCatalog準備設定を生成します。
        /// </summary>
        /// <param name="config">評価規則の適合率範囲を持つGameplay設定。</param>
        /// <returns>MissionCatalog準備設定。</returns>
        /// <exception cref="ArgumentNullException">Gameplay設定がnullの場合。</exception>
        /// <exception cref="InvalidOperationException">取得経路に必要なAssetが未設定の場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">取得経路が未定義の場合。</exception>
        public MissionCatalogPreparationSettings CreateCatalogPreparationSettings(GameplayConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (missionParameterGenerationProfileAsset == null)
            {
                throw new InvalidOperationException(
                    "GameplayConfigEntry has no MissionParameterGenerationProfileAsset.");
            }

            MissionParameterGenerationSettings generationSettings =
                missionParameterGenerationProfileAsset.CreateGenerationSettings();
            MissionCatalogEvaluationPolicy evaluationPolicy =
                missionParameterGenerationProfileAsset.CreateEvaluationPolicy(config);
            switch (missionParameterSourceMode)
            {
                case MissionParameterSourceMode.Automatic:
                    return MissionCatalogPreparationSettings.CreateAutomatic(
                        generationSettings,
                        evaluationPolicy);

                case MissionParameterSourceMode.PresetDebug:
                    if (missionPresetAsset == null)
                    {
                        throw new InvalidOperationException(
                            "GameplayConfigEntry has no MissionPresetAsset for PresetDebug.");
                    }

                    return MissionCatalogPreparationSettings.CreatePresetDebug(
                        generationSettings,
                        evaluationPolicy,
                        missionPresetAsset.CreateCandidates());

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(missionParameterSourceMode),
                        missionParameterSourceMode,
                        null);
            }
        }

        /// <summary>
        /// 指定されたNavigation RequestがこのEntryに対応するかを取得します。
        /// </summary>
        /// <param name="request">対応を確認するGameplay開始Request。</param>
        /// <returns>ModeとDifficultyが一致する場合はtrue。</returns>
        public bool Matches(GameplayNavigationRequest request)
        {
            if (request == null)
            {
                return false;
            }

            return gameMode == request.GameMode && difficulty == request.Difficulty;
        }

        /// <summary>
        /// Inspectorからの直接起動で使用する既定のGameplay開始Requestを生成します。
        /// </summary>
        /// <returns>既定音量を使用したGameplay開始Request。</returns>
        public GameplayNavigationRequest CreateDefaultRequest()
        {
            return new GameplayNavigationRequest(
                gameMode,
                difficulty,
                new OptionSettingsSnapshot(
                    OptionVolumeLevels.Default,
                    OptionVolumeLevels.Default));
        }

        /// <summary>
        /// 指定されたOption設定を使用してGameplay開始Requestを生成します。
        /// </summary>
        /// <param name="optionSettings">Gameplay開始時のOption設定。</param>
        /// <returns>Gameplay開始Request。</returns>
        /// <exception cref="ArgumentNullException">Option設定がnullの場合。</exception>
        public GameplayNavigationRequest CreateRequest(
            OptionSettingsSnapshot optionSettings)
        {
            if (optionSettings == null)
            {
                throw new ArgumentNullException(nameof(optionSettings));
            }

            return new GameplayNavigationRequest(
                gameMode,
                difficulty,
                optionSettings);
        }
    }
}
