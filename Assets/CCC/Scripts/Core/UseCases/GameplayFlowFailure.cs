using System;

namespace Ccc.Core.UseCases
{
    /// <summary>
    /// Gameplayフローの開始または進行を継続できない理由を表します。
    /// </summary>
    public sealed class GameplayFlowFailure
    {
        /// <summary>
        /// 失敗の種類を取得します。
        /// </summary>
        public GameplayFlowFailureKind Kind { get; }

        /// <summary>
        /// ログ向けの技術的な失敗理由を取得します。
        /// </summary>
        public string Message { get; }

        private GameplayFlowFailure(GameplayFlowFailureKind kind, string message)
        {
            if (!Enum.IsDefined(typeof(GameplayFlowFailureKind), kind) ||
                kind == GameplayFlowFailureKind.None)
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Failure message cannot be empty.", nameof(message));
            }

            Kind = kind;
            Message = message;
        }

        /// <summary>
        /// フロー失敗を生成します。
        /// </summary>
        /// <param name="kind">失敗の種類。</param>
        /// <param name="message">ログ向けの技術的な理由。</param>
        /// <returns>生成された失敗値。</returns>
        internal static GameplayFlowFailure Create(GameplayFlowFailureKind kind, string message)
        {
            return new GameplayFlowFailure(kind, message);
        }
    }

    /// <summary>
    /// Gameplayフロー失敗の種類を表します。
    /// </summary>
    public enum GameplayFlowFailureKind
    {
        /// <summary>
        /// 成功Resultで使用される値です。
        /// </summary>
        None = 0,

        /// <summary>
        /// 既存セッションが実行中です。
        /// </summary>
        SessionAlreadyRunning,

        /// <summary>
        /// Gameplay設定が不正です。
        /// </summary>
        InvalidConfiguration,

        /// <summary>
        /// MissionCatalogを構築できませんでした。
        /// </summary>
        CatalogBuildFailed,

        /// <summary>
        /// Missionを抽選できませんでした。
        /// </summary>
        MissionDrawFailed
    }
}
