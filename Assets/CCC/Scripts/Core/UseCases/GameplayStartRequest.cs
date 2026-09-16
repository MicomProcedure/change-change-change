using System;
using Ccc.Core.Model;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Gameplayセッション開始に必要な選択モードとMission Catalog準備設定を保持します。
    /// </summary>
    public sealed class GameplayStartRequest
    {
        /// <summary>Sessionで選択されたゲームモードを取得します。</summary>
        public GameMode GameMode { get; }

        /// <summary>Sessionで選択された難易度を取得します。</summary>
        public GameDifficulty GameDifficulty { get; }

        /// <summary>Session開始時のMission Catalog準備設定を取得します。</summary>
        public MissionCatalogPreparationSettings CatalogPreparationSettings { get; }

        /// <summary>
        /// ゲームモード、難易度、検証済みCatalog準備設定からRequestを生成します。
        /// </summary>
        /// <param name="gameMode">選択されたゲームモード。</param>
        /// <param name="gameDifficulty">選択された難易度。</param>
        /// <param name="catalogPreparationSettings">Catalog準備設定。</param>
        public GameplayStartRequest(
            GameMode gameMode,
            GameDifficulty gameDifficulty,
            MissionCatalogPreparationSettings catalogPreparationSettings)
        {
            if (!Enum.IsDefined(typeof(GameMode), gameMode))
            {
                throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, null);
            }

            if (!Enum.IsDefined(typeof(GameDifficulty), gameDifficulty))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(gameDifficulty),
                    gameDifficulty,
                    null);
            }

            GameMode = gameMode;
            GameDifficulty = gameDifficulty;
            CatalogPreparationSettings = catalogPreparationSettings ??
                throw new ArgumentNullException(nameof(catalogPreparationSettings));
        }
    }
}
