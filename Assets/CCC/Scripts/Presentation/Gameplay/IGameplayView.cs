using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace Ccc.Presentation
{
    /// <summary>
    /// Gameplay画面全体がPresenterへ提供する受動的なView契約です。
    /// </summary>
    public interface IGameplayView
    {
        /// <summary>
        /// Gameplay画面で発生した入力事実のストリームを取得します。
        /// </summary>
        Observable<GameplayInput> Inputs { get; }

        /// <summary>
        /// GameModeとGameDifficultyの組ごとに決まる表示設定を適用します。
        /// </summary>
        /// <param name="settings">適用する表示設定。</param>
        void ApplyDisplaySettings(GameplayDisplaySettingsDto settings);

        /// <summary>
        /// 完成済みの表示状態を描画します。
        /// </summary>
        /// <param name="state">描画対象の表示状態。</param>
        void Render(GameplayViewStateDto state);

        /// <summary>
        /// Result画面を表示します。
        /// </summary>
        /// <param name="result">Result画面の表示状態。</param>
        void ShowResult(GameplayResultViewStateDto result);

        /// <summary>
        /// 提出結果のフィードバック演出を再生します。
        /// </summary>
        /// <param name="feedback">演出に必要な表示データ。</param>
        /// <param name="cancellationToken">演出をキャンセルするトークン。</param>
        /// <returns>演出完了を表す非同期処理。</returns>
        UniTask PlayFeedbackAsync(
            FeedbackViewDataDto feedback,
            CancellationToken cancellationToken);

        /// <summary>
        /// Gameplay設定またはAssetの検証エラーを表示します。
        /// </summary>
        /// <param name="errors">表示するエラー一覧。</param>
        void ShowConfigurationError(IReadOnlyList<string> errors);

        /// <summary>
        /// 回復不能なエラーを表示します。
        /// </summary>
        /// <param name="message">表示するエラーメッセージ。</param>
        void ShowFatalError(string message);
    }
}
