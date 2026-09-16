using Ccc.Core.Logic;
using Ccc.Core.Model;
using UnityEngine;
using System.Collections.Generic;
public class GameplaySetupExample
{
    public GameplaySessionContext InitializeSession(GameplayConfig config)
    {
        // ----------------------------------------------------
        // Step 1: ゲーム内で出現しうる全計算式候補を列挙 (0除算は自動除外)
        // ----------------------------------------------------
        // 例: 最小値 1 〜 最大値 9 の場合、全 1〜9 の四則演算式が生成される
        IReadOnlyList<ExpressionCandidate> expressions =
            ExpressionCandidateGenerator.Generate(config.MinimumNumber, config.MaximumNumber);

        // ----------------------------------------------------
        // Step 2: プリセット定義から出題させたいミッション候補リストを用意
        // ----------------------------------------------------
        var parameterCandidates = new List<MissionParameterCandidate>
        {
            // Tier 1 (パラメータなし)
            new MissionParameterCandidate(MissionCondition.Create(MissionKind.Even)),
            new MissionParameterCandidate(MissionCondition.Create(MissionKind.Odd)),
            new MissionParameterCandidate(MissionCondition.Create(MissionKind.Integer)),
            new MissionParameterCandidate(MissionCondition.Create(MissionKind.Positive)),

            // Tier 1 (1パラメータ)
            new MissionParameterCandidate(MissionCondition.Create(MissionKind.AtLeast, 5)),

            // Tier 2 (0, 素数, 平方数)
            new MissionParameterCandidate(MissionCondition.Create(MissionKind.Zero)),
            new MissionParameterCandidate(MissionCondition.Create(MissionKind.Prime)),
            new MissionParameterCandidate(MissionCondition.Create(MissionKind.Square)),

            // Tier 2 (Between: 3以上 3+5=8以下)
            new MissionParameterCandidate(MissionCondition.Between(3, 5)),
        };

        // ----------------------------------------------------
        // Step 3: カタログをビルド (適合率・件数のフィルタリングとソート)
        // ----------------------------------------------------
        MissionCatalogBuildResult buildResult =
            MissionCatalogBuilder.Build(
                expressions,
                parameterCandidates,
                Ccc.Tests.MissionTestSettings.CreateEvaluationPolicy(config),
                Rational.FromSingle(0.25f));

        // ----------------------------------------------------
        // Step 4: ビルド成否の確認
        // ----------------------------------------------------
        if (!buildResult.IsSuccess)
        {
            Debug.LogError($"カタログ構築失敗: {buildResult.FailureKind} - {buildResult.FailureMessage}");
            // 例外処理やタイトル画面へのフォールバック等
            return null;
        }

        MissionCatalog catalog = buildResult.Catalog;

        // ----------------------------------------------------
        // Step 5: セッションコンテキストに格納してゲーム開始
        // ----------------------------------------------------
        var sessionContext = new GameplaySessionContext(config, catalog);
        return sessionContext;
    }
}
