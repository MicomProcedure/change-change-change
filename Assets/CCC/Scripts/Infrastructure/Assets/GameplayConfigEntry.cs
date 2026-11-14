using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Presentation;
using Ccc.Presentation.Audio;
using Ccc.Presentation.Navigation;
using Ccc.Presentation.ScoreRank;
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
        private string _displayName;

        [SerializeField]
        private GameMode _gameMode;

        [SerializeField]
        private GameDifficulty _difficulty;

        [SerializeField]
        private GameplayConfigAsset _gameplayConfigAsset;

        [SerializeField]
        private MissionParameterSourceMode _missionParameterSourceMode =
            MissionParameterSourceMode.Automatic;

        [SerializeField]
        private MissionParameterGenerationProfileAsset _missionParameterGenerationProfileAsset;

        [SerializeField]
        private MissionPresetAsset _missionPresetAsset;

        [SerializeField]
        private MissionTextAsset _missionTextAsset;

        [SerializeField]
        private AudioKey _gameplayBgmKey = DefaultGameplayBgmKey;

        [SerializeField]
        private ExpressionDisplayStyle _expressionDisplayStyle;

        [Tooltip("評価段階ごとに必要なスコア。低い段階から順に、1以上で厳密に増加する値を設定する。要素数が評価段階の数になる。")]
        [SerializeField]
        private List<int> _scoreRankThresholds = new List<int>();

        /// <summary>
        /// Inspectorやデバッグログで使用する表示名を取得します。
        /// </summary>
        public string DisplayName
        {
            get { return string.IsNullOrWhiteSpace(_displayName) ? name : _displayName; }
        }

        /// <summary>
        /// 対応するゲームモードを取得します。
        /// </summary>
        public GameMode GameMode
        {
            get { return _gameMode; }
        }

        /// <summary>
        /// 対応するゲーム難易度を取得します。
        /// </summary>
        public GameDifficulty Difficulty
        {
            get { return _difficulty; }
        }

        /// <summary>
        /// 対応するGameplay設定Assetを取得します。
        /// </summary>
        public GameplayConfigAsset GameplayConfigAsset
        {
            get { return _gameplayConfigAsset; }
        }

        /// <summary>
        /// Missionパラメータ候補の取得経路を取得します。
        /// </summary>
        public MissionParameterSourceMode MissionParameterSourceMode
        {
            get { return _missionParameterSourceMode; }
        }

        /// <summary>
        /// Missionパラメータ生成・評価Profile Assetを取得します。
        /// </summary>
        public MissionParameterGenerationProfileAsset MissionParameterGenerationProfileAsset
        {
            get { return _missionParameterGenerationProfileAsset; }
        }

        /// <summary>
        /// 対応するMission候補Preset Assetを取得します。
        /// </summary>
        public MissionPresetAsset MissionPresetAsset
        {
            get { return _missionPresetAsset; }
        }

        /// <summary>
        /// 対応するMission表示文言Assetを取得します。
        /// 未設定の場合はデバッグ用Fallback文言が使用されます。
        /// </summary>
        public MissionTextAsset MissionTextAsset
        {
            get { return _missionTextAsset; }
        }

        /// <summary>
        /// Gameplay中に再生するBGMのAudioKeyを取得します。
        /// </summary>
        public AudioKey GameplayBgmKey
        {
            get { return _gameplayBgmKey; }
        }

        /// <summary>
        /// Gameplay画面で式を表示するスタイルを取得します。
        /// </summary>
        public ExpressionDisplayStyle ExpressionDisplayStyle
        {
            get { return _expressionDisplayStyle; }
        }

        /// <summary>
        /// Entryの必須設定を検証し、検出されたエラー一覧を返します。
        /// </summary>
        /// <returns>検出された検証エラー一覧。</returns>
        public IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            if (!Enum.IsDefined(typeof(GameMode), _gameMode))
            {
                errors.Add($"GameMode is undefined: {_gameMode}.");
            }

            if (!Enum.IsDefined(typeof(GameDifficulty), _difficulty))
            {
                errors.Add($"GameDifficulty is undefined: {_difficulty}.");
            }

            if (_gameplayConfigAsset == null)
            {
                errors.Add("Gameplay config asset is missing.");
            }

            if (!Enum.IsDefined(
                    typeof(MissionParameterSourceMode),
                    _missionParameterSourceMode))
            {
                errors.Add(
                    $"Mission parameter source mode is undefined: {_missionParameterSourceMode}.");
            }

            if (_missionParameterGenerationProfileAsset == null)
            {
                errors.Add("Mission parameter generation profile asset is missing.");
            }

            if (_missionParameterSourceMode == MissionParameterSourceMode.PresetDebug &&
                _missionPresetAsset == null)
            {
                errors.Add("PresetDebug requires a Mission preset asset.");
            }

            if (!Enum.IsDefined(typeof(AudioKey), _gameplayBgmKey) ||
                _gameplayBgmKey == AudioKey.None)
            {
                errors.Add($"Gameplay BGM key is not configured: {_gameplayBgmKey}.");
            }

            if (!Enum.IsDefined(typeof(ExpressionDisplayStyle), _expressionDisplayStyle))
            {
                errors.Add($"Expression display style is undefined: {_expressionDisplayStyle}.");
            }

            // 基準の規則はScoreRankThresholds自身が持つため、生成を試みて違反を検出する
            try
            {
                CreateScoreRankThresholds();
            }
            catch (ArgumentException exception)
            {
                errors.Add($"Score rank thresholds are invalid: {exception.Message}");
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

            if (_missionParameterGenerationProfileAsset == null)
            {
                throw new InvalidOperationException(
                    "GameplayConfigEntry has no MissionParameterGenerationProfileAsset.");
            }

            MissionParameterGenerationSettings generationSettings =
                _missionParameterGenerationProfileAsset.CreateGenerationSettings();
            MissionCatalogEvaluationPolicy evaluationPolicy =
                _missionParameterGenerationProfileAsset.CreateEvaluationPolicy(config);
            switch (_missionParameterSourceMode)
            {
                case MissionParameterSourceMode.Automatic:
                    return MissionCatalogPreparationSettings.CreateAutomatic(
                        generationSettings,
                        evaluationPolicy);

                case MissionParameterSourceMode.PresetDebug:
                    if (_missionPresetAsset == null)
                    {
                        throw new InvalidOperationException(
                            "GameplayConfigEntry has no MissionPresetAsset for PresetDebug.");
                    }

                    return MissionCatalogPreparationSettings.CreatePresetDebug(
                        generationSettings,
                        evaluationPolicy,
                        _missionPresetAsset.CreateCandidates());

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(_missionParameterSourceMode),
                        _missionParameterSourceMode,
                        null);
            }
        }

        /// <summary>
        /// Inspectorで設定された必要スコアから評価段階の基準を生成します。
        /// </summary>
        /// <returns>評価段階の基準。</returns>
        /// <exception cref="ArgumentException">必要スコアが未設定、または基準の規則に違反している場合。</exception>
        public ScoreRankThresholds CreateScoreRankThresholds()
        {
            return new ScoreRankThresholds(_scoreRankThresholds);
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

            return _gameMode == request.GameMode && _difficulty == request.Difficulty;
        }

        /// <summary>
        /// Inspectorからの直接起動で使用する既定のGameplay開始Requestを生成します。
        /// </summary>
        /// <returns>既定音量を使用したGameplay開始Request。</returns>
        public GameplayNavigationRequest CreateDefaultRequest()
        {
            return new GameplayNavigationRequest(
                _gameMode,
                _difficulty,
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
                _gameMode,
                _difficulty,
                optionSettings);
        }
    }
}
